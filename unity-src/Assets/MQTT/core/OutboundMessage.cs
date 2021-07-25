using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	public class OutboundMessage: BaseMessage
	{
		private string _id = string.Empty;
		public string Id { get { return _id; } private set { _id = value; } }

		public void GenerateId()
		{
			Id = Guid.NewGuid().ToString();
		}

		public void SetId(string id)
		{
			Id = id;
		}

		public bool WasDelivered = false;

		public ITopic Topic = null;


		public Action<OutboundMessage> OnSuccess = (m) => { };
		public Action<OutboundMessage> OnFailure = (m) => { };
	}
}