using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace HG.iot.mqtt
{
	public enum QualityOfServiceEnum
	{
		Undefined = -1,
		BestEffort = 0,
		AtLeastOnce = 1,
		ExactlyOnce = 2
	}
}