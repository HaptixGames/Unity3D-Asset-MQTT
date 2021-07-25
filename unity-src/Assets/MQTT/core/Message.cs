using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace HG.iot.mqtt
{
	[Serializable]
	public abstract class Message
	{
		public ITopic Topic { get; set; }

		[NonSerialized]
		public bool SerializationFailed = false;

		[NonSerialized]
		public bool ArrivedEmpty = false;

		[NonSerialized]
		public string ArrivedTopic = string.Empty;

		[NonSerialized]
		public string OriginalMessage = string.Empty;

		public Message()
		{
			setTimestamp();
		}

		public static long Epoch()
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToInt64((DateTime.UtcNow - epoch).TotalMilliseconds);
		}

		private void setTimestamp()
		{
			if(timestamp!=0)
				return;

			timestamp = Message.Epoch();
		}

		public long SetArrivalTimestamp()
		{
			arrivedTimestamp = Message.Epoch();
			return transitMs;
		}

		public long timestamp = 0;

		public long arrivedTimestamp { get; private set; }

		public long transitMs {
			get {
				return arrivedTimestamp - timestamp;
			}
		}
	}
}