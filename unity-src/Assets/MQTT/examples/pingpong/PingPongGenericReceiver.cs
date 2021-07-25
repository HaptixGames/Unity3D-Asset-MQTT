using System.Collections;
using System.Collections.Generic;
using HG.iot.mqtt;
using UnityEngine;

namespace hg.iot.mqtt.example.pingpong
{
    public class PingPongGenericReceiver : MonoBehaviour
    {
        void onMqttEvent(MqttEvent @event)
        {
            Debug.Log("[t/event] id: " + @event.ID.ToString() + ", event: " + @event.EVENT + ", data: " + @event.DATA.GetType().FullName);

            // remove the complex receiver script from receivers gameobject
            //  and add this generic receiver script instead

            // subscribe your broker to 'pingpong/pong'
            // send a message to filter defined by PingTopic in editor, default 'pingpong/ping'

            // in a generic receivers, all mqtt related events arrive to the onMqttEvent method

            // let's look for message arrival events
            if (@event.EVENT == ReceiverEvent.MESSAGE_ARRIVED)
            {
                MessageArrival ma = (MessageArrival)@event.DATA;

                // if message arrived on PingTopic, let's reply
                if (ma.Topic is PingTopic)
                {
                    // let's send a message to a topic that has already been added in the editor
                    PongMessage p = new PongMessage();
                    p.SendByMessageType(qualityOfService: QualityOfServiceEnum.AtLeastOnce);
                }
            }

            // editor topics can also be maniuplated at runtime
            //  below will create an additional PingTopic to the editor upon broker connection
            //  now you will notice two responses when you send a ping message to the broker
            /*
            if (@event.EVENT == ReceiverEvent.CONNECT_SUCCESS)
            {
                var topicsParent = GameObject.Find("topics");
                var receivers = new List<GameObject> { GameObject.Find("receivers") };
                var pongtopic = BrokerConnection.Instance.AddTopic(topicsParent, typeof(PingTopic), "pingpong/ping", "ping", receivers, subscribeNow: true, subscribeOnConnect: true);
            }
            */

            // here is how the mqtt support in your scene is structured
            /*

             scene:
                mqtt GO:
                    #1 config GO:
                        - BrokerConnection: defines connection parameters to broker
                            - Bootstrap: drop the #2 bootstrap gameobject here
                            - Receivers: drop the #4 receivers gameobject here
                        - DesktopClient: support for desktop builds
                        - UWPClient: support for UWP builds
                        - WebGLClient: support for WebGL builds
                    #2 bootstrap GO:
                        - your bootstrap script
                    #4 topics GO:
                        - your editor defined mqtt topics
                            - Receivers: drop the #4 receivers gameobject here                       
                    #4 receivers GO:
                        - your mqtt events receiver scripts

             */

        }
    }
}