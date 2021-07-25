using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	public abstract class BaseMessage
	{
		public string Message = string.Empty;
		public bool IsRetained = false;
		public QualityOfServiceEnum QualityOfService = QualityOfServiceEnum.BestEffort;
	}
}
