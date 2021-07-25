using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	public class SubscriptionOptions
	{
		public string Topic = string.Empty;
		public QualityOfServiceEnum QualityOfService = QualityOfServiceEnum.BestEffort;
		public string InvocationContext = string.Empty;
		public int Timeout = 15;
	}
}