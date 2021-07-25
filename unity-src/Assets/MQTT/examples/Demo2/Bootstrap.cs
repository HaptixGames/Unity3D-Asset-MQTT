using HG.iot.mqtt;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace hg.iot.mqtt.example.demo2
{
    public class Bootstrap : MonoBehaviour
    {
        void onMqttReady(IBrokerConnection connectionManager)
        {
            Debug.Log("bootstrap: ready");
        }

        void onMqttBoot(IBrokerConnection connectionManager)
        {
            Debug.Log("bootstrap: booting");
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
    }
}
