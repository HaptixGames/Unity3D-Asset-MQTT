using UnityEngine;
using System.Collections;
using System;

namespace HG.webgl
{
	public class UnityJavascriptInterop2 : MonoBehaviour 
	{
		[SerializeField]
		private GameObject _locationReceiver = null;

		[SerializeField]
		private string _locationReceiverMethod = "onReceiveLocation";

		private string _locMethod {
			get {
				return string.IsNullOrEmpty(_locationReceiverMethod)
						? "onReceiveLocation"	
						: _locationReceiverMethod;
			}
		}

		IEnumerator Start () 
		{
            // let javascript know that Unity is ready
            // this script should be the last script in execution order

            Debug.Log("UnityJavascriptInterop2.Start()");

#if UNITY_WEBGL && !UNITY_EDITOR

            Debug.Log("Notify browser that Unity player is ready");
			HGUNITY3DJS.Ready(this.gameObject.name);

#else

            yield return new WaitForSeconds(1.0f);

			if(_locationReceiver!=null)
				_locationReceiver.SendMessage(_locMethod, new LocationResponse {
					location = new LocationResponse.Location { 
						country_code = "US",
						country_name = "United States",
						city = "Chicago",
						state = "Illinois",
						postal = "60000",
						latitude = 42,
						longitude = -88,
						IPv4 = "127.0.0.1"
					},
					error = new LocationResponse.Error {
						status = 200,
						code = "0",
						message = "None"
					}
				});
			
			#endif

			yield return null;
		}

		void HGGENERICJS_OnToggleConsole(int value)
		{
			Console.Instance.Toggle();
		}

		void HGGEOLOCATORJS_OnLocation(string data)
		{
			LocationResponse response = JsonUtility.FromJson<LocationResponse>(data);

			MainThreadInvoke.Instance.Add(() => { 
				if(_locationReceiver!=null)
					_locationReceiver.SendMessage(_locMethod, response);
			});
		}

		[Serializable]
		public class LocationResponse 
		{
			[Serializable]
			public class Error
			{
				public int status = 0;
				public string code = string.Empty;
				public string message = string.Empty;
			}

			[Serializable]
			public class Location
			{
				public string country_code = string.Empty;
				public string country_name = string.Empty;
				public string city = string.Empty;
				public string postal = string.Empty;
			    public double latitude = 0;
			    public double longitude = 0;
				public string IPv4 = string.Empty;
				public string state = string.Empty;
			}

			public Error error = null;
			public Location location = null;
		}
	}
}
