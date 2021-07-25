using UnityEngine;
using System.Collections;
using System;

namespace HG.webgl
{
	public class UnityJavascriptInterop : MonoBehaviour 
	{
		[SerializeField]
		private string _geolocatorGoogleKey = "AIzaSyB1FMzOxifHhBB5GxroamCbye4j8EW5zYg";

		[SerializeField]
		private GameObject _locationReceiver = null;

		[SerializeField]
		private float _locationReceiptDelay = 3f;

		[SerializeField]
		private string _locationReceiverMethod = "onGeolocatorLocation";

		private string _locMethod {
			get {
				return string.IsNullOrEmpty(_locationReceiverMethod)
						? "onGeolocatorLocation"	
						: _locationReceiverMethod;
			}
		}

		IEnumerator Start () 
		{
			// let javascript know that Unity is ready
			// this script should be the last script in execution order

			#if UNITY_WEBGL && !UNITY_EDITOR

			//HGUNITY3DJS.Ready(this.gameObject.name, _geolocatorGoogleKey);
			HGUNITY3DJS.Ready(this.gameObject.name);

			#else

			yield return new WaitForSeconds(_locationReceiptDelay);

			if(_locationReceiver!=null)
				_locationReceiver.SendMessage(_locMethod, new GeolocatorResponse {
					error = null,
					location = new GeolocatorResponse.Location {
						coords = new GeolocatorResponse.Location.Coordinates {
							accuracy = 20,
							latitude = 42f,
							longitude = -88f
						}
					}
				});
			
			#endif

			yield return null;
		}

		void HGGENERICJS_OnToggleConsole(
			int value)
		{
			Console.Instance.Toggle();
		}

		void HGGEOLOCATORJS_OnLocation(
			string data)
		{
			GeolocatorResponse response = JsonUtility.FromJson<GeolocatorResponse>(data);

			MainThreadInvoke.Instance.Add(() => { 
				if(_locationReceiver!=null)
					_locationReceiver.SendMessage(_locMethod, response);
			});
		}

		[Serializable]
		public class GeolocatorResponse 
		{
			[Serializable]
			public class Error
			{
				public string code = string.Empty;
				public string message = string.Empty;
			}

			[Serializable]
			public class Location
			{
				[Serializable]
				public class Coordinates
				{
					public float accuracy = 0f;			//Specifies the accuracy of the latitude and longitude estimates in meters.
					public float altitude = 0f;			//Specifies the altitude estimate in meters above the WGS 84 ellipsoid.
					public float altitudeAccuracy = 0f;	//Specifies the accuracy of the altitude estimate in meters.
					public float heading = 0f;			//Specifies the device's current direction of movement in degrees counting clockwise relative to true north.
					public float latitude = 0f;			//Specifies the latitude estimate in decimal degrees. The value range is [-90.00, +90.00].
					public float longitude = 0f;		//Specifies the longitude estimate in decimal degrees. The value range is [-180.00, +180.00].
					public float speed = 0f;			//Specifies the device's current ground speed in meters per second.
				}

				public Coordinates coords = null;		//Specifies the geographic location of the device. The location is expressed as a set of geographic coordinates together with information about heading and speed. See geolocator~Coordinates type for details.
				public string flag = string.Empty;		//URL of the country flag image, in SVG format. This property exists only if address information is available.

				//public Address address = null;			//Specifies the address of the fetched location. The address is expressed as a set of political and locality components. This property might be undefined if addressLookup option is not enabled for the corresponding method. See geolocator~Address type for details.
				public string formattedAddress = string.Empty;	//The human-readable address of this location. Often this address is equivalent to the "postal address," which sometimes differs from country to country.
				//public MapData map = null;				//Provides references to the components of a created Google Maps Map and the containing DOM element. See geolocator~MapData type for details.
				public string placeId = string.Empty;	//A unique identifier that can be used with other Google APIs.
				public bool targetReached = false;		//Specifies whether the defined target coordinates is reached. This property is only available for geolocator.watch() method when target option is defined.
				public float timestamp = 0f;			//Specifies the time when the location information was retrieved and the Location object created.
				//public TimeZone timezone = null;		//Specifies time offset data for the fetched location on the surface of the earth. See geolocator~TimeZone type for details.
				public string type = string.Empty;		//Type of the location. See geolcoator.LocationType enumeration for details.
			}

			public Error error = null;
			public Location location = null;
		}
	}
}
