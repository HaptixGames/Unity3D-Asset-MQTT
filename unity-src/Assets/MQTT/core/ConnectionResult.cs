using UnityEngine;
using System.Collections;

namespace HG.iot.mqtt
{
	public class ConnectionResult
	{
        public override string ToString()
        {
            IBrokerConnection broker = BrokerConnection.Instance;
            return string.Format("broker: {0}:{1}, context: {2}, client-id:{3}, error-code: {4}, error-msg: {5}", broker.DefaultConnectionOptions.Host,broker.DefaultConnectionOptions.Port, ContextId, ClientId, ErrorCode, ErrorMessage);
        }

        public string ContextId = string.Empty;
		public string ClientId = string.Empty;
		public int ErrorCode = 0;
		public string ErrorMessage = string.Empty;
	}
}
