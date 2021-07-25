using System.Collections;
using System.Collections.Generic;
using HG.iot.mqtt;
using UnityEngine;

namespace hg.iot.mqtt.example.pingpong
{
    public class PingPongComplexReceiver : MonoBehaviour
    {
        void onMqttReady_PingTopic(ITopic topic)
        {
            Debug.Log("[t/complex] onMqttReady, topic: " + topic.GetType().FullName);
        }

        void onMqttConnectSuccess_PingTopic(ConnectionResult response)
        {
            Debug.Log("[t/complex/PingTopic] onMqttConnectSuccess");
        }

        void onMqttConnectFailure_PingTopic(ConnectionResult response)
        {
            Debug.Log("[t/complex/PingTopic] onMqttConnectFailure");
        }

        void onMqttConnectLost_PingTopic(ConnectionResult response)
        {
            Debug.Log("[t/complex/PingTopic] onMqttConnectLost");
        }

        void onMqttReconnect_PingTopic(ConnectionResult response)
        {
            Debug.Log("[t/complex/PingTopic] onMqttReconnect");
        }

        void onMqttMessageArrived_PingTopic(MessageArrival message)
        {
            Debug.Log("[t/complex] onMqttMessageArrived, topic: " + message.Topic.GetType().FullName + ", message: " + message.Payload.GetType().FullName);

            // remove the simple receiver script from receivers gameobject
            //  and add this complex receiver script instead

            // subscribe your broker to 'pingpong/pong'
            // send a message to filter defined by PingTopic in editor, default 'pingpong/ping'

            // the way a complex receiver is structured is each method is the mqtt event followed by the topic type name
            //  here we intercept the 'onMqttMessageArrived' method invocation for 'PintTopic'

            // let's send a message to a topic that has already been added in the editor
            PongMessage p = new PongMessage();
            p.SendByMessageType(qualityOfService: QualityOfServiceEnum.AtLeastOnce);

            // next, take a look at PingPongGenericReceiver
        }

        // since we are sending messages to the pong topic, we will want to be notified if the message sent was delivered
        void onMqttMessageNotDelivered_PongTopic(DeliveryResponse response)
        {
            Debug.Log("[t/complex] onMqttMessageNotDelivered, topic: " + response.Topic.GetType().FullName + ", id: " + response.Id);
        }

        void onMqttMessageDelivered_PongTopic(DeliveryResponse response)
        {
            Debug.Log("[t/complex] onMqttMessageDelivered, topic: " + response.Topic.GetType().FullName + ", id: " + response.Id);
        }

        void onMqttMessageNotDelivered_PingTopic(DeliveryResponse response)
        {
            Debug.Log("[t/complex] onMqttMessageNotDelivered, topic: " + response.Topic.GetType().FullName + ", id: " + response.Id);
        }

        void onMqttMessageDelivered_PingTopic(DeliveryResponse response)
        {
            Debug.Log("[t/complex] onMqttMessageDelivered, topic: " + response.Topic.GetType().FullName + ", id: " + response.Id);
        }

        void onMqttSubscriptionSuccess_PingTopic(SubscriptionResponse response)
        {
            Debug.Log("[t/complex] onMqttSubscriptionSuccess, topic: " + response.Topic.GetType().FullName);
        }

        void onMqttSubscriptionFailure_PingTopic(SubscriptionResponse response)
        {
            Debug.Log("[t/complex] onMqttSubscriptionFailure, topic: " + response.Topic.GetType().FullName);
        }

        void onMqttUnsubscriptionSuccess_PingTopic(SubscriptionResponse response)
        {
            Debug.Log("[t/complex] onMqttSubscriptionSuccess, topic: " + response.Topic.GetType().FullName);
        }

        void onMqttUnsubscriptionFailure_PingTopic(SubscriptionResponse response)
        {
            Debug.Log("[t/complex] onMqttUnsubscriptionFailure, topic: " + response.Topic.GetType().FullName);
        }
    }
}