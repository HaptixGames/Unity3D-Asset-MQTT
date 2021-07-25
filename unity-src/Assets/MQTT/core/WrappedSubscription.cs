using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	public class WrappedSubscription
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

		public QualityOfServiceEnum GrantedQualityOfService = QualityOfServiceEnum.Undefined;
		public ITopic Topic = null;

		public int ErrorCode = 0;
		public string ErrorMessage = string.Empty;

		public Action<WrappedSubscription> OnSuccess = (m) => { };
		public Action<WrappedSubscription> OnFailure = (m) => { };
	}
}