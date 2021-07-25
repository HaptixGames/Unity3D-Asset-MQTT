using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace HG.iot.mqtt.interop
{
	public class HGIOTMQTTJS
	{
		#if UNITY_WEBGL //&& !UNITY_EDITOR
		//http://www.eclipse.org/paho/files/jsdoc/index.html

		[DllImport("__Internal")]
		public static extern int Connect(
			string gameObjectName,
			string host, 
			int port, 
			string clientId, 
			string path, 
			string invocationContext,
			int keepAliveInterval,
			bool cleanSession,
			string username,
			string password,
			bool useSSL,
			bool autoReconnect,
			int reconnectDelayMs,
			int lwtQos,
			string lwtTopic,
			string lwtMessage);

		[DllImport("__Internal")]
		public static extern bool Reconnect(int instanceId);

		[DllImport("__Internal")]
		public static extern bool Disconnect(int instanceId);

		[DllImport("__Internal")]
		public static extern bool Send(int instanceId, string topic, string data, int qos, bool retained);

		[DllImport("__Internal")]
		public static extern bool Subscribe(int instanceId, string filter, int qos, string invocationContext, int timeout);

		[DllImport("__Internal")]
		public static extern bool Unsubscribe(int instanceId, string filter, string invocationContext, int timeout);

		[DllImport("__Internal")]
		private static extern void StartTrace(int instanceId);

		[DllImport("__Internal")]
		private static extern void StopTrace(int instanceId);

		[DllImport("__Internal")]
		private static extern object[] GetTraceLog(int instanceId);

		/*
		[DllImport("__Internal")]
		extern static string emscripten_run_script_string(string script);
		...

		string username = emscripten_run_script_string("data.username");
		string message = emscripten_run_script_string("data.message");
		*/
		#else

		public static int Connect(
			string host, 
			int port, 
			string clientId, 
			string path, 
			string invocationContext,
			int keepAliveInterval,
			bool cleanSession,
			string username,
			string password,
			bool useSSL,
			bool autoReconnect,
			int reconnectDelayMs,
			int lwtQos,
			string lwtTopic,
			string lwtMessage) { throw new System.PlatformNotSupportedException(); }

		public static bool Reconnect(int instanceId) { throw new System.PlatformNotSupportedException(); }
		public static bool Disconnect(int instanceId) { throw new System.PlatformNotSupportedException(); }
		public static bool Send(int instanceId, string topic, string data, int qos, bool retained) { throw new System.PlatformNotSupportedException(); }
		public static bool Subscribe(int instanceId, string filter, int qos, string invocationContext, int timeout) { throw new System.PlatformNotSupportedException(); }
		public static bool Unsubscribe(int instanceId, string filter, string invocationContext, int timeout) { throw new System.PlatformNotSupportedException(); }
		private static void StartTrace(int instanceId) { throw new System.PlatformNotSupportedException(); }
		private static void StopTrace(int instanceId) { throw new System.PlatformNotSupportedException(); }
		private static object[] GetTraceLog(int instanceId) { throw new System.PlatformNotSupportedException(); }

		#endif
	}
}
