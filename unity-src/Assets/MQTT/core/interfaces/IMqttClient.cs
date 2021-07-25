using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	public interface IMqttClient
	{
		string ClientId { get; }
		void Init(IBrokerConnection connectionManager);
		bool Connect(ConnectionOptions options);
		bool Disconnect();
		bool Reconnect();
		OutboundMessage Send(OutboundMessage wrappedMessage);
		WrappedSubscription Subscribe(WrappedSubscription subscription);
		WrappedSubscription Unsubscribe(WrappedSubscription subscription);
	}
}