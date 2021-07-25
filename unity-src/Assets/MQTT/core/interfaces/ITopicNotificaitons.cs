using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	public interface ITopicNotificaitons
	{
		void onMqttReady(Guid id);
		void onMqttMessageDelivered(Guid id, DeliveryResponse response);
		void onMqttMessageNotDelivered(Guid id, DeliveryResponse response);
		void onMqttMessageArrived(Guid id, string arrivalTopic, string message);
		void onMqttSubscriptionSuccess(Guid id, SubscriptionResponse response);
		void onMqttSubscriptionFailure(Guid id, SubscriptionResponse response);
		void onMqttUnsubscriptionSuccess(Guid id, SubscriptionResponse response);
		void onMqttUnsubscriptionFailure(Guid id, SubscriptionResponse response);
		void onMqttConnectSuccess(Guid id, ConnectionResult result);
		void onMqttConnectFailure(Guid id, ConnectionResult result);
		void onMqttConnectLost(Guid id, ConnectionResult result);
		void onMqttReconnect(Guid id, ConnectionResult result);
		Message onMqttMessageDeserialize(string arrivalTopic, string message);
		string onMqttMessageSerialize(Message message);
	}
}