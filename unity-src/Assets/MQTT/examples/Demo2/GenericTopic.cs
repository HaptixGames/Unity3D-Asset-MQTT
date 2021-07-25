using HG.iot.mqtt;
using System;
using UnityEngine;

namespace hg.iot.mqtt.example.demo2
{
    public class GenericTopic : Topic<GenericMessage>
    {
        // handle incoming messages
        public override Message onMqttMessageDeserialize(string arrivalTopic, string message)
        {
            GenericMessage msg = Activator.CreateInstance<GenericMessage>();
            msg.SerializationFailed = false;
            msg.ArrivedEmpty = string.IsNullOrEmpty(message);
            msg.text = message;
            return msg;
        }

        // handle outgoing messages
        public override string onMqttMessageSerialize(Message message)
        {
            return (message as GenericMessage).text;
        }
    }
}