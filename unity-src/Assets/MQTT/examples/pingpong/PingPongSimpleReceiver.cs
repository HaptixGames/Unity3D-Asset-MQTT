using HG.iot.mqtt;
using System;
using UnityEngine;

namespace hg.iot.mqtt.example.pingpong
{
    public class PingPongSimpleReceiver : MonoBehaviour
    {
        void onMqttReady(ITopic topic)
        {
            Debug.Log("[t/simple] onMqttReady, topic: " + topic.GetType().FullName);
        }

        void onMqttConnectSuccess(ConnectionResult response)
        {
            Debug.Log("[t/simple] onMqttConnectSuccess");
        }

        void onMqttConnectFailure(ConnectionResult response)
        {
            Debug.Log("[t/simple] onMqttConnectFailure");
        }

        void onMqttConnectLost(ConnectionResult response)
        {
            Debug.Log("[t/simple] onMqttConnectLost");
        }

        void onMqttReconnect(ConnectionResult response)
        {
            Debug.Log("[t/simple] onMqttReconnect");
        }

        void onMqttMessageArrived(MessageArrival message)
        {
            Debug.Log("[t/simple] onMqttMessageArrived, topic: " + message.Topic.GetType().FullName + ", message: " + message.Payload.GetType().FullName);

            // subscribe your broker to 'pingpong/pong'
            // when you send a message to filter defined by PingTopic in editor, default 'pingpong/ping'
            //  below code will respond with multiple messages, read on
            if (message.Topic is PingTopic)
            {
                // let's send a message to a topic that has already been added in the editor
                PongMessage pong = new PongMessage();
                // notice that PongTopic has a message type of PongMessage
                //  here we find matching topics given the message type
                pong.SendByMessageType(qualityOfService: QualityOfServiceEnum.AtLeastOnce);
                // we can also find the editor topics by its filter
                pong.SendByFilter("pingpong/pong", qualityOfService: QualityOfServiceEnum.AtLeastOnce);
                // we can also find the editor topics by its type
                pong.SendByTopicType<PongTopic>(qualityOfService: QualityOfServiceEnum.AtLeastOnce);
                // we can also find the editor topics by its user defined field
                pong.SendByUserDefined("pong", qualityOfService: QualityOfServiceEnum.AtLeastOnce);
                // or we can create a new topic, send the message, and destroy the topic
                pong.SendOnce<PongTopic, PongMessage>("pingpong/pong", qualityOfService: QualityOfServiceEnum.AtLeastOnce);
                // by including all five ways of sending a message, 
                //  you will notice five responses to your ping message at the broker
                // if you were to duplicate the PongTopic on the receiver game object,
                //  you would notice nine messages sent, this is because 'SendOnce' method will only execute once
                //  all other ways of sending messages will pick up both instances of PongTopic inside the editor

                // next, take a look at PingPongComplexReceiver
            }
        }

        void onMqttMessageNotDelivered(DeliveryResponse response)
        {
            Debug.Log("[t/simple] onMqttMessageNotDelivered, topic: " + response.Topic.GetType().FullName + ", id: " + response.Id);
        }

        void onMqttMessageDelivered(DeliveryResponse response)
        {
            Debug.Log("[t/simple] onMqttMessageDelivered, topic: " + response.Topic.GetType().FullName + ", id: " + response.Id);
        }

        void onMqttSubscriptionSuccess(SubscriptionResponse response)
        {
            Debug.Log("[t/simple] onMqttSubscriptionSuccess, topic: " + response.Topic.GetType().FullName);
        }
        
        void onMqttSubscriptionFailure(SubscriptionResponse response)
        {
            Debug.Log("[t/simple] onMqttSubscriptionFailure, topic: " + response.Topic.GetType().FullName);
        }

        void onMqttUnsubscriptionSuccess(SubscriptionResponse response)
        {
            Debug.Log("[t/simple] onMqttSubscriptionSuccess, topic: " + response.Topic.GetType().FullName);
        }

        void onMqttUnsubscriptionFailure(SubscriptionResponse response)
        {
            Debug.Log("[t/simple] onMqttUnsubscriptionFailure, topic: " + response.Topic.GetType().FullName);
        }
    }
}