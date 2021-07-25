using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Reflection;
using System.Net.Security;

namespace HG.iot.mqtt.clients
{
	#if UNITY_STANDALONE || UNITY_EDITOR

	public class DesktopClient: MonoBehaviour, IMqttClient
	{
		IBrokerConnection _connection = null;
		ConnectionOptions _connectionOptions = null;
		uPLibrary.Networking.M2Mqtt.MqttClient _platform = null;

		public string ClientId { get { return _connectionOptions.ClientId; } }

		public void Init (IBrokerConnection connectionManager)
		{
			_connection = connectionManager;
		}

		public bool Connect (ConnectionOptions options)
		{
			_connectionOptions = options;

			//TODO: port and SSL not supported
			if (options.UseSSL)
			{
				_platform = new uPLibrary.Networking.M2Mqtt.MqttClient(_connectionOptions.Host, _connectionOptions.Port, true, null, null, uPLibrary.Networking.M2Mqtt.MqttSslProtocols.TLSv1_0);

				//HACK: since we do not want to maintain the m2mqtt lib ourselves, we just have to inject our own cert callback
				if(options.AcceptInvalidServerCertificate)
				{
					MethodInfo mi = _platform.GetType().GetMethod("Init",BindingFlags.NonPublic | BindingFlags.Instance);
					RemoteCertificateValidationCallback certValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
					mi.Invoke(_platform, new object[] { _connectionOptions.Host, _connectionOptions.Port, true, null, null, uPLibrary.Networking.M2Mqtt.MqttSslProtocols.TLSv1_0, certValidationCallback, null });
				}
			}
			else
				_platform = new uPLibrary.Networking.M2Mqtt.MqttClient(_connectionOptions.Host, _connectionOptions.Port, false, null, null, uPLibrary.Networking.M2Mqtt.MqttSslProtocols.None);
				//TODO: regression test above ctor with port added

			_platform.ProtocolVersion = (uPLibrary.Networking.M2Mqtt.MqttProtocolVersion)options.ProtocolVersion;

			_platform.ConnectionClosed += _platform_ConnectionClosed;
			_platform.MqttMsgPublished += _platform_MqttMsgPublished;
			_platform.MqttMsgPublishReceived += _platform_MqttMsgPublishReceived;
			_platform.MqttMsgSubscribed += _platform_MqttMsgSubscribed;
			_platform.MqttMsgUnsubscribed += _platform_MqttMsgUnsubscribed;


            int code = -1;

            try
            {
                code = _platform.Connect(
                    this.ClientId,
                    options.Username,
                    options.Password,
                    false,
                    options.LwtTopic == null
                        ? (byte)QualityOfServiceEnum.BestEffort
                        : (byte)options.ILwtTopic.RequestedQualityOfService,
                    options.LwtTopic == null
                        ? false
                        : true,
                    options.LwtTopic == null
                        ? string.Empty
                        : options.ILwtTopic.FilterAtRuntime,
                    options.LwtTopic == null
                        ? "lwt"                             //TODO: passing empty string causes exception
                        : options.ILwtTopic.DefaultMessage.ToJson(),
                    options.CleanSession,
                    (ushort)options.KeepAliveInterval);

                _connectionOptions.ClientId = _platform.ClientId;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

			if(code==0)
				_connection.Notify.onConnectSuccess(new ConnectionResult { ClientId = this.ClientId });
			else
				_connection.Notify.onConnectFailure(new ConnectionResult { ErrorCode = (int)code });

			return code==0;
		}

		void _platform_ConnectionClosed (object sender, EventArgs e)
		{
			_connection.Notify.onConnectLost(new ConnectionResult { ClientId = this.ClientId });
		}

		public bool Disconnect ()
		{
			_platform.Disconnect();
			return true;
		}

		public bool Reconnect ()
		{
			throw new NotImplementedException ("MQTT reconnect logic not implemented.");
		}

		private Dictionary<string,OutboundMessage> _outstandingMessages = new Dictionary<string,OutboundMessage>();
		private Dictionary<string,WrappedSubscription> _outstandingSubscriptions = new Dictionary<string,WrappedSubscription>();

		public OutboundMessage Send (OutboundMessage wrappedMessage)
		{
			ushort id = _platform.Publish(
				wrappedMessage.Topic.FilterAtRuntime,
				Encoding.UTF8.GetBytes(wrappedMessage.Message),
				(byte)wrappedMessage.QualityOfService,
				wrappedMessage.IsRetained);

			wrappedMessage.SetId(id.ToString());
			if(wrappedMessage.QualityOfService == QualityOfServiceEnum.AtLeastOnce ||
			   wrappedMessage.QualityOfService == QualityOfServiceEnum.ExactlyOnce)
					_outstandingMessages.Add(wrappedMessage.Id, wrappedMessage);

			return wrappedMessage;
		}

		void _platform_MqttMsgPublished (object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishedEventArgs e)
		{
			OutboundMessage wm = _outstandingMessages[e.MessageId.ToString()];
			_outstandingMessages.Remove(wm.Id);

			wm.WasDelivered = e.IsPublished;

			if(wm.WasDelivered)
			{
				if(wm.OnSuccess!=null)
					wm.OnSuccess(wm);
			}
			else
			{
				if(wm.OnFailure!=null)
					wm.OnFailure(wm);
			}
		}

		void _platform_MqttMsgPublishReceived (object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
		{
			InboundMessage msg = new InboundMessage();
			msg.IsDuplicate = e.DupFlag;
			msg.IsRetained = e.Retain;
			msg.QualityOfService = (QualityOfServiceEnum) e.QosLevel;
			msg.Topic = e.Topic;
			msg.Message = Encoding.UTF8.GetString(e.Message);

			_connection.Notify.onMessageArrived(msg);
		}

		public WrappedSubscription Subscribe (WrappedSubscription subscription)
		{
			ushort msgId = _platform.Subscribe(
				new [] { subscription.Topic.FilterAtRuntime }, 
				new [] { (byte)subscription.Topic.RequestedQualityOfService });

			subscription.SetId(msgId.ToString());

			_outstandingSubscriptions.Add(subscription.Id,subscription);

			return subscription;
		}

		public WrappedSubscription Unsubscribe (WrappedSubscription subscription)
		{
			ushort msgId = _platform.Unsubscribe(
				new [] { subscription.Topic.FilterAtRuntime });

			subscription.SetId(msgId.ToString());

			_outstandingSubscriptions.Add(subscription.Id,subscription);

			return subscription;
		}

		void _platform_MqttMsgUnsubscribed (object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgUnsubscribedEventArgs e)
		{
			WrappedSubscription sub = _outstandingSubscriptions[e.MessageId.ToString()];

			_outstandingSubscriptions.Remove(sub.Id);

			if(sub.OnSuccess!=null)
				sub.OnSuccess(sub);
		}

		void _platform_MqttMsgSubscribed (object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgSubscribedEventArgs e)
		{
			WrappedSubscription sub = _outstandingSubscriptions[e.MessageId.ToString()];

			sub.GrantedQualityOfService = (QualityOfServiceEnum)e.GrantedQoSLevels[0];

			_outstandingSubscriptions.Remove(sub.Id);

			if(sub.OnSuccess!=null)
				sub.OnSuccess(sub);
		}
	}

	#else

	public class DesktopClient: MonoBehaviour
	{

	}

	#endif
}