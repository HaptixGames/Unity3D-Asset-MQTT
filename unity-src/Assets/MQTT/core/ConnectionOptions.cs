using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	[Serializable]
	public class ConnectionOptions
	{
		#if UNITY_WEBGL && !UNITY_EDITOR
		public int Port = 61614;
		#else
		public int Port = 1883;
		#endif

		public string Host = "localhost";

		public bool RandomClientId = true;

		public bool ForceDefaultClientId = false;

		public string ClientId = string.Empty;

		public string Path = "/mqtt";

		public bool AutoReconnect = true;

		public int ReconnectDelayMs = 5000;

		public int Timeout = 30;

		public string Username = string.Empty;

		public string Password = string.Empty;

		public int KeepAliveInterval = 60;

		public bool CleanSession = true;

		public bool UseSSL = false;

		public bool AcceptInvalidServerCertificate = true;

		public string InvocationContext = System.Guid.NewGuid().ToString();

		public string[] Hosts;

		public int[] Ports;

		public MQTTVersionEnum ProtocolVersion = MQTTVersionEnum.MQTT_3_1_1;

		public TopicBehaviour LwtTopic = null;

		public ITopic ILwtTopic {
			get {
				return LwtTopic==null
						? null
						: LwtTopic as ITopic;
			}
		}
			
		public string ClientIdRuntime {
			get 
			{
				if(RandomClientId) {
					if(string.IsNullOrEmpty(_clientId))
						_clientId = System.Guid.NewGuid().ToString();
				}
				else {
					_clientId = this.ForceDefaultClientId ? "UNITY3D-MQTT-CLIENT" : this.ClientId;
				}

				return _clientId;
			}

			set 
			{
				_clientId = value;
			}
		}

		private string _clientId = string.Empty;
	}
}