using UnityEngine;
using System;
using System.Collections;

namespace HG
{
	public class Logger
	{
		private static SEVERITY _verbosity = SEVERITY.INFO;

		public static void SetVerbosity(SEVERITY verbosity)
		{
			_verbosity = verbosity;
		}

		public static void Log(string source, string message, SEVERITY severity = SEVERITY.DEBUG)
		{
			if(severity > _verbosity)
				return;

			var format = "({0}) [{1}] {2}";
			var log = string.Format(format,DateTime.UtcNow.ToString(),source,message);

			switch(severity)
			{
				case SEVERITY.INFO:
					Debug.Log(log);
					break;
				case SEVERITY.WARN:
					Debug.LogWarning(log);
					break;
				case SEVERITY.ERROR:
					Debug.LogError(log);
					break;
			}
		}
	}

	public enum SEVERITY
	{
		VERBOSE 	= 9,
		DEBUG 		= 8,
		INFO 		= 7,
		WARN 		= 5,
		ERROR 		= 3,
		CRITICAL 	= 0
	}
}