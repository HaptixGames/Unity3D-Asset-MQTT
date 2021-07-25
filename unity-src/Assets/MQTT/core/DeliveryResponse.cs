using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	public class DeliveryResponse
	{
        public override string ToString()
        {
            return string.Format("filter: {0}, was-delivered: {1}, id:{2}", Topic.FilterAtRuntime, WasDelivered, Id);
        }

        public string Id;
        public bool WasDelivered = false;
        public ITopic Topic = null;
	}
}