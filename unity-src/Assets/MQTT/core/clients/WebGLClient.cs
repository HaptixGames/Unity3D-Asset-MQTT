using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HG.iot.mqtt.clients
{
	#if UNITY_EDITOR

	public class WebGLClient: MonoBehaviour
	{

	}

	#elif UNITY_WEBGL && !UNITY_EDITOR

	public class WebGLClient: MonoBehaviour, IMqttClient
	{
	IBrokerConnection _connection = null;
	ConnectionOptions _connectionOptions = null;

	#region platform specific data structures

	[Serializable]
	private class jsData
	{
		public int __instanceId = -1;
	}

	[Serializable]
	private class jsMessage : jsData
	{
		public string topic;
		public string payloadString;
		public bool isDuplicate;
		public bool isRetained;
		public QualityOfServiceEnum qualityOfService;
	}

	[Serializable]
	private class InvocationContext
	{
		public string ic = string.Empty;
	}

	[Serializable]
	private class jsContext : jsData
	{
		public InvocationContext invocationContext = null;
	}

	[Serializable]
	private class jsConnection : jsContext
	{
		public bool autoReconnect = false;
		public int reconnectCount = -1;
		public int reconnectAttempts = -1;
		public bool sessionLost = false;
		public string clientId = string.Empty;
		public int errorCode = 0;
		public string errorMessage = string.Empty;
	}

	[Serializable]
	private class jsSubscription : jsContext
	{
		public QualityOfServiceEnum grantedQos;
		public int errorCode = 0;
		public string errorMessage = string.Empty;
	}

	#endregion

	private Dictionary<string,OutboundMessage> _outstandingMessages = new Dictionary<string,OutboundMessage>();
	private Dictionary<string,WrappedSubscription> _outstandingSubscriptions = new Dictionary<string,WrappedSubscription>();

	public string ClientId {
		get {
			return _connectionOptions.ClientId;
		}
	}

	#region JS library interface

	// client instance in JS library
	public int InstanceId { get; private set; }

	public void Init (IBrokerConnection connectionManager)
	{
		_connection = connectionManager;
	}

	public bool Connect (ConnectionOptions options)
	{
		_connectionOptions = options;

		InstanceId = interop.HGIOTMQTTJS.Connect(
		gameObject.name,
		options.Host,
		options.Port,
		options.ClientId,
		options.Path,
		options.InvocationContext,
		options.KeepAliveInterval,
		options.CleanSession,
		options.Username,
		options.Password,
		options.UseSSL,
		options.AutoReconnect,
		options.ReconnectDelayMs,
		options.LwtTopic==null 
			? (byte)QualityOfServiceEnum.BestEffort 
			: (byte)options.ILwtTopic.RequestedQualityOfService,
		options.LwtTopic==null 
			? string.Empty 
			: options.ILwtTopic.FilterAtRuntime,
		options.LwtTopic==null 
			? string.Empty
			: options.ILwtTopic.DefaultMessage.ToJson());

		return InstanceId > -1;
	}

	public bool Disconnect ()
	{
		return interop.HGIOTMQTTJS.Disconnect(InstanceId);
	}

	public bool Reconnect ()
	{
		return interop.HGIOTMQTTJS.Reconnect(InstanceId);
	}

	// send message to broker
	public OutboundMessage Send (OutboundMessage message)
	{
		message.GenerateId();

		//if(message.QualityOfService == QualityOfServiceEnum.AtLeastOnce ||
		//   message.QualityOfService == QualityOfServiceEnum.ExactlyOnce) {
			Debug.Log("adding key: " + message.Topic.FilterAtRuntime+message.Message);
			_outstandingMessages.Add(
				message.Topic.FilterAtRuntime+message.Message,	//TODO: ambiguous, but no ID provided by JS library
				message);
		//}

		bool response = interop.HGIOTMQTTJS.Send(
			InstanceId, 
			message.Topic.FilterAtRuntime, 
			message.Message, 
			(int)message.QualityOfService, 
			message.IsRetained);

		return message;
	}

	public WrappedSubscription Subscribe (WrappedSubscription subscription)
	{
		subscription.GenerateId();

		_outstandingSubscriptions.Add(subscription.Id,subscription);

		bool response = interop.HGIOTMQTTJS.Subscribe(
			InstanceId,
			subscription.Topic.FilterAtRuntime,
			(int)subscription.Topic.RequestedQualityOfService,
			subscription.Id,
			subscription.Topic.SubscriptionTimeout);

		return subscription;
	}

	public WrappedSubscription Unsubscribe (WrappedSubscription subscription)
	{
		subscription.GenerateId();

		_outstandingSubscriptions.Add(subscription.Id,subscription);

		bool response = interop.HGIOTMQTTJS.Unsubscribe(
			InstanceId,
			subscription.Topic.FilterAtRuntime,
			subscription.Id,
			subscription.Topic.SubscriptionTimeout);

		return subscription;
	}

	#endregion

	#region callbacks from JS library

	private void HGIOTMQTTJS_onConnectSuccess(
		string data)
	{
		jsConnection c = JsonUtility.FromJson<jsConnection>(data);

		_connectionOptions.ClientId = c.clientId;

		_connection.Notify.onConnectSuccess(new ConnectionResult {
			ContextId = c.invocationContext.ic,
			ClientId = this.ClientId
		});
	}

	private void HGIOTMQTTJS_onConnectFailure(
		string data)
	{
		jsConnection c = JsonUtility.FromJson<jsConnection>(data);

		_connection.Notify.onConnectFailure(new ConnectionResult {
			ContextId = c.invocationContext.ic,
			ErrorCode = c.errorCode,
			ErrorMessage = c.errorMessage,
			ClientId = this.ClientId
		});
	}

	private void HGIOTMQTTJS_onReconnect(
		string data)
	{
		jsConnection c = JsonUtility.FromJson<jsConnection>(data);

		_connection.Notify.onReconnect(new ConnectionResult {
			ContextId = c.invocationContext.ic,
			ClientId = this.ClientId
			});
	}

	private void HGIOTMQTTJS_onConnectionLost(
		string data)
	{
		jsConnection c = JsonUtility.FromJson<jsConnection>(data);

		_connection.Notify.onConnectLost(new ConnectionResult {
			ErrorCode = c.errorCode,
			ErrorMessage = c.errorMessage,
			ClientId = this.ClientId
		});
	}

	// delivery notification of our message to broker
	private void HGIOTMQTTJS_onMessageDelivered(
		string data)
	{
		jsMessage m = JsonUtility.FromJson<jsMessage>(data);

		try {
			OutboundMessage wm = _outstandingMessages[m.topic+m.payloadString];
			_outstandingMessages.Remove(m.topic+m.payloadString);

			wm.WasDelivered = true;

			if(wm.OnSuccess!=null)
				wm.OnSuccess(wm);

		} catch (KeyNotFoundException kex) {
			Debug.LogWarning("key not found: " + m.topic+m.payloadString); 
		}
	}

	private void HGIOTMQTTJS_onMessageArrived(
		string data)
	{
		jsMessage m = JsonUtility.FromJson<jsMessage>(data);

		InboundMessage wm = new InboundMessage {
			Message = m.payloadString,
			Topic = m.topic,
			IsRetained = m.isRetained,
			IsDuplicate = m.isDuplicate,
			QualityOfService = m.qualityOfService,
			__instanceId = m.__instanceId
		};

		_connection.Notify.onMessageArrived(wm);
	}

	private void HGIOTMQTTJS_onSubscribeSuccess(
		string data)
	{
		jsSubscription s = JsonUtility.FromJson<jsSubscription>(data);

		WrappedSubscription ws = _outstandingSubscriptions[s.invocationContext.ic];

		_outstandingSubscriptions.Remove(ws.Id);

		ws.GrantedQualityOfService = s.grantedQos;

		if(ws.OnSuccess!=null)
			ws.OnSuccess(ws);
	}

	private void HGIOTMQTTJS_onSubscribeFailure(
		string data)
	{
		jsSubscription s = JsonUtility.FromJson<jsSubscription>(data);

		WrappedSubscription ws = _outstandingSubscriptions[s.invocationContext.ic];

		_outstandingSubscriptions.Remove(ws.Id);

		ws.ErrorCode = s.errorCode;
		ws.ErrorMessage = s.errorMessage;

		if(ws.OnFailure!=null)
			ws.OnFailure(ws);
	}

	private void HGIOTMQTTJS_onUnsubscribeSuccess(
		string data)
	{
		jsSubscription s = JsonUtility.FromJson<jsSubscription>(data);

		WrappedSubscription ws = _outstandingSubscriptions[s.invocationContext.ic];

		_outstandingSubscriptions.Remove(ws.Id);

		if(ws.OnSuccess!=null)
			ws.OnSuccess(ws);
	}

	private void HGIOTMQTTJS_onUnsubscribeFailure(
		string data)
	{
		jsSubscription s = JsonUtility.FromJson<jsSubscription>(data);

		WrappedSubscription ws = _outstandingSubscriptions[s.invocationContext.ic];

		_outstandingSubscriptions.Remove(ws.Id);

		ws.ErrorCode = s.errorCode;
		ws.ErrorMessage = s.errorMessage;

		if(ws.OnFailure!=null)
			ws.OnFailure(ws);
	}

	#endregion
	}

	#endif
}
