using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	public class SubscriptionResponse
	{
        public override string ToString()
        {
            return string.Format("filter: {0}, qos: {1}, id:{2}, error-code: {3}, error-msg: {4}", Topic.FilterAtRuntime, GrantedQualityOfService.ToString(), Id, ErrorCode, ErrorMessage);
        }

        public string Id;
		public QualityOfServiceEnum GrantedQualityOfService = QualityOfServiceEnum.Undefined;
		public int ErrorCode = 0;
		public string ErrorMessage = string.Empty;
        public ITopic Topic = null;
	}
}