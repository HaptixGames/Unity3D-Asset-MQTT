using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace HG.webgl
{
	public class HGUNITY3DJS
	{
		#if UNITY_WEBGL && !UNITY_EDITOR

		[DllImport("__Internal")]
		public static extern bool Ready(string gameObjectName);

		#else

		public static bool Ready(string gameObjectName) 
		{
			throw new PlatformNotSupportedException();
		}

		#endif
	}
}