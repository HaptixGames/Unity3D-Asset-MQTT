using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace HG.iot.mqtt
{
	public interface IBrokerConnection
	{
		ConnectionOptions DefaultConnectionOptions { get; }
		IBrokerConnectionNotifications Notify { get; }
        bool QueueMessagesWhenDisconnected { get; }
        IEnumerable<GameObject> Receivers { get; }
		int ChangeTopicFilter<TTopic>(string oldValue, string newValue);
		IEnumerable<ITopic> GetTopicsByTopicType(Type filter);
		IEnumerable<ITopic> GetTopicsByMessageType(Type filter);
		IEnumerable<ITopic> GetTopicsByFilter(string filter);
		IEnumerable<ITopic> GetTopicsByUserDefined(string value);
		void AddTopic(ITopic topic);
        ITopic AddTopic(GameObject parent, Type type, string filter, string alias,
            IEnumerable<GameObject> receivers = null,
            QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort,
            bool allowEmptyMessage = false,
            int subscriptionTimeout = 15,
            bool subscribeNow = false,
            bool subscribeOnConnect = false);
		void RemoveTopic(ITopic topic);
		string ClientId { get; }
		bool Connect (ConnectionOptions options = null);
		bool Disconnect ();
		bool Reconnect ();
		bool IsConnected { get; }
		string Subscribe(ITopic topic);
		string Unsubscribe(ITopic topic);
		string Send(
			ITopic topic, 
			string message, 
			bool isRetained = false, 
			QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort);
	}
}