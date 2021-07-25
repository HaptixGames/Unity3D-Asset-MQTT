using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	public class InboundMessage: BaseMessage
	{
		public int __instanceId = -1;
		public string Topic = null;
		public bool IsDuplicate = false;
	}
}