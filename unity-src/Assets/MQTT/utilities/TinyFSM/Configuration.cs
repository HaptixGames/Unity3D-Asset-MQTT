using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HG.TinyFSM
{
	public enum LogSeverity
	{
		ERROR,
		WARNING,
		INFO,
		VERBOSE
	}

	public static partial class Configuration
	{
		private static Dictionary<string, object> settings = new Dictionary<string, object>()
		{
			{ "log-WARNING"					, 		true },					// log warnings
			{ "log-ERROR"					, 		true },					// log errors
			{ "log-INFO"					, 		true },					// log informational
			{ "log-VERBOSE"					, 		false },				// log verbose
			{ "log-internal"				, 		true },					// log internal events
			{ "log-callback"				,		new Action<string, LogSeverity>((message, severity) => { }) },
			{ "persistent-game-object-name"	,		"haptixgames.com" },
			{ "persistent-game-object-flags",		HideFlags.None },
			{ "tiny-fsm-game-object-flags",			HideFlags.None }
		};

		public static bool HasSetting(string name)
		{
			return settings.ContainsKey(name);	
		}
		
		public static T GetSetting<T>(string name)
		{
			if(settings.ContainsKey(name))
				return (T)settings[name];
			else
			{
				LogInternal("Configuration does NOT contain requested key : " + name, LogSeverity.WARNING);
				return default(T);
			}
		}

		public static T GetSetting<T>(string name, T defaultValue)
		{
			if(settings.ContainsKey(name))
				return (T)settings[name];
			else
			{
				LogInternal("Configuration does NOT contain requested key : " + name, LogSeverity.WARNING);
				return defaultValue;
			}
		}

		public static object GetSetting(string name)
		{
			return GetSetting<object>(name);
		}

		public static void SetSetting(string name, object @value)
		{
			if(name=="log-VERBOSE" && (bool)@value == true)
				Debug.LogWarning("WARNING! (Web API Kit) Verbose logging has been turned on.  Verbose logging adversly affects performance!");


			if(settings.ContainsKey(name))
			{
				LogInternal("Configuration value set : " + name + " = " + @value, LogSeverity.VERBOSE);
				settings[name] = @value;
			}
			else
			{
				LogInternal("Configuration value added : " + name + " = " + @value, LogSeverity.VERBOSE);
				settings.Add (name, @value);
			}
		}
		
		public static bool RemoveSetting(string name)
		{
			if(settings.ContainsKey(name))
				return settings.Remove(name);
			
			return false;
		}

		public static void Log(string message, LogSeverity severity = LogSeverity.INFO)
		{
			if(GetSetting<bool>("log-"+severity.ToString()))
			{
				switch(severity)
				{
					case LogSeverity.ERROR:
						Debug.LogError("<color=red>{ERR}</color> " + message);
						break;
					case LogSeverity.WARNING:
						Debug.LogWarning("<color=orange>{WARN}</color> " + message);
						break;
					case LogSeverity.INFO:
						Debug.Log("<color=white>{INFO}</color> " + message);
						break;
					case LogSeverity.VERBOSE:
						Debug.Log("<color=grey>{VERB}</color> " + message);
						break;
				}
				
				Action<string, LogSeverity> callback = GetSetting<Action<string, LogSeverity>>("log-callback");
				if(callback!=null)
					callback(message, severity);
			}
		}
		
		public static void LogInternal(string transactionId, string message, LogSeverity severity = LogSeverity.INFO)
		{
			if(GetSetting<bool>("log-internal"))
				Log ("<color=white>[" + transactionId + "]</color> " + message, severity);
		}

		public static void LogInternal(string message, LogSeverity severity = LogSeverity.INFO)
		{
			if(GetSetting<bool>("log-internal"))
				Log (message, severity);
		}

		public static GameObject Bootstrap()
		{
			GameObject go = GameObject.Find("/" + Configuration.GetSetting<string>("persistent-game-object-name"));

			if(go==null) 
			{  
				go = new GameObject(Configuration.GetSetting<string>("persistent-game-object-name"));
				go.hideFlags = Configuration.GetSetting<HideFlags>("persistent-game-object-flags");
				
				#if !UNITY_EDITOR
				UnityEngine.Object.DontDestroyOnLoad(go);
				#endif
			}
			
			return go;
		}
	}
}