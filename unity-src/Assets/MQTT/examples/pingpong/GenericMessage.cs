using HG.iot.mqtt;
using System;

namespace hg.iot.mqtt.example.pingpong
{
    [Serializable]
    public class GenericMessage : Message
    {
        public string text;
    }
}