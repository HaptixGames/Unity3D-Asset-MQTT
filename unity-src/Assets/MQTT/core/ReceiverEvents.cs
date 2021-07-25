using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	public enum ReceiverEvent
    {
        NONE,
        READY,
        BOOT,
        CONNECT_SUCCESS,
        CONNECT_FAILURE,
        CONNECT_LOST,
        RECONNECT,
        SUBSCRIPTION_SUCCESS,
        SUBSCRIPTION_FAILURE,
        UNSUBSCRIPTION_SUCCESS,
        UNSUBSCRIPTION_FAILURE,
        MESSAGE_DELIVERED,
        MESSAGE_NOT_DELIVERED,
        MESSAGE_ARRIVED
    }

    public class MqttEvent
    {
        public Guid ID;
        public ReceiverEvent EVENT;
        public object DATA;
    }
}