using HG.iot.mqtt;
using System;

namespace hg.iot.mqtt.example.demo2
{
    [Serializable]
    public class GenericMessage : Message
    {
        public string text;
    }
}