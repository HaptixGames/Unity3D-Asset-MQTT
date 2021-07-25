using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HG.iot.mqtt {
	public struct MessageArrival
    {
        public override string ToString()
        {
            return string.Format("filter: {0}, message: {1}", Topic.FilterAtRuntime, Payload.OriginalMessage);
        }

        public ITopic Topic;
		public Message Payload;
	}
}