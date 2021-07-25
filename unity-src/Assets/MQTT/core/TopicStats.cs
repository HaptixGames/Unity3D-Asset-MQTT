using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HG.iot.mqtt
{
	[System.Serializable]
	public class TopicStats
	{
		public int MessagesQueued=0;
		public int MessagesSent=0;
		public int MessagesNotSent=0;
		public int MessagesReceived=0;
		public int MessagesDropped=0;

		public List<long> TransitTimesMs = new List<long>();

		public void AddTransitTime(long time)
		{
			TransitTimesMs.Add(time);
		}
	}
}
