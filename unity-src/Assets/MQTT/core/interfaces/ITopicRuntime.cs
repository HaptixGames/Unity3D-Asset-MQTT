using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace HG.iot.mqtt
{
	public interface ITopicRuntime
	{
        void Configure(string filter, string alias, IEnumerable<GameObject> receivers, QualityOfServiceEnum qualityOfService, bool allowEmptyMessage, int subscriptionTimeout, bool subscribeOnConnect);
	}
}