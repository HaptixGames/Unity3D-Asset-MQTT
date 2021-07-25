using HG.iot.mqtt;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace hg.iot.mqtt.example.pingpong
{
    public class PingPongBootstrap : MonoBehaviour
    {
        void onMqttReady(IBrokerConnection connectionManager)
        {
            Debug.Log("bootstrap: ready");
        }

        // this gameobject is dropped on the BrokerConnection.Bootstrap field
        // this method is invoked when the BrokerConnection script starts up
        // before this method is invoked, each topic's onMqttReady(ITopic) method is invoked
        void onMqttBoot(IBrokerConnection connectionManager)
        {
            // let's connect to the broker
            //  but before we do lets set up some global things
            //  we can access this script from anywhere through Bootstrap.Instance

            // set LTW
            // MyLwtMessage lwtMessage = new MyLwtMessage { };
            // connectionManager.ILwtTopic.DefaultMessage = lwtMessage;

            // we can change our topic filters if we need to
            // connectionManager.ChangeTopicFilter<PingTopic>("pong", "p0ng");

            Debug.Log("bootstrap: booting");
            Debug.LogFormat("CONNECTING {0}:{1} ...",
                                    connectionManager.DefaultConnectionOptions.Host,
                                    connectionManager.DefaultConnectionOptions.Port);
            var connectResult = connectionManager.Connect();
            Debug.LogFormat("CONNECTED ? {0}", connectResult.ToString());
        }

        void onMqttConnectSuccess(ConnectionResult result)
        {
            Debug.LogWarning("bootstrap: connection-success");

        }

        void onMqttConnectFailure(ConnectionResult result)
        {
            Debug.LogWarning("bootstrap: connection-failure");
        }

        void onMqttConnectLost(ConnectionResult result)
        {
            Debug.LogWarning("bootstrap: connection-lost");
        }

        void onMqttReconnect(ConnectionResult result)
        {
            Debug.LogWarning("bootstrap: connection-reconnect");
        }

#if UNITY_WEBGL
        private void onGeolocatorLocation(
            HG.webgl.UnityJavascriptInterop.GeolocatorResponse location)
        {
            // if location.error.errorCode != empty then you have a problem
            // in some instances it looks like a double request comes through,
            //  especially when user first ignores the 'share location' popup
            //  and then requests again.  in this case the first location will arrive correctly
            //  and the second location does not have an error but is all zeros
            //  so once you have a location, hold onto it!

            Debug.LogFormat("Location information: {0}", JsonUtility.ToJson(location));
        }
#endif
    }
}
