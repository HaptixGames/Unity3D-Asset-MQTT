# Unity3D-Asset-MQTT

[WebGL Demo](http://unity3dassets.com/r/wak/demos/mqtt/2/)  

[Code Flow](http://unity3dassets.com/wp-content/uploads/2018/08/wak_mqtt_flow_1.png)  

[Event Callbacks](http://unity3dassets.com/wp-content/uploads/2018/08/wak_mqtt_flow_2.png)  


## Setting up your scene

1. Create a GameObject.

2. Add the following scripts: ConnectionBroker, DesktopClient, and WebGLClient.

3. Configure your connection options on the ConnectionBroker’s DefaultConnectionOptions field.

4. Add the GlobalTopic script to one of your game objects.  Set its filter (topic path) and other options.

5. Add the SomeReceiver script to another one of your game objects.

6. Drag and drop the game object that contains SomeReceiver script to the GlobalTopic’s Receivers field.

## Topics and Messages

You will be creating classes to represent topics and messages.  Topics must inherit from Topic<TMessage> and messages must inherit from Message.  Unity’s JsonUtility is used to serialize and deserialize messages across the wire.

```
public sealed class GlobalTopic : Topic<GlobalMessage>
{

}

[Serializable]
public sealed class GlobalMessage: Message
{
   public string text;
   public int number;
   public bool truth;
}
```
  
## Receivers

Receiver scripts are able to communicate with the broker through topics.  Here is an example of one with all the methods required to interface.

```
public class SomeReceiver : MonoBehaviour
{
   ITopic _cacheGlobalTopic = null;

   // Topic.SimpleNotifications=TRUE
   void onMqttReady(ITopic topic)
   {
      _cacheGlobalTopic = topic;

      Debug.Log("onMqttReady invoked");
      Debug.Log(string.Format("'{0}' topic's SimpleNotifications are set to TRUE",topic.Filter));

      tests(topic);
   }

   // Topic.SimpleNotifications=FALSE
   void onMqttReady_GlobalTopic(ITopic topic)
   {
      _cacheGlobalTopic = topic;

      Debug.Log("onMqttReady_GlobalTopic invoked");
      Debug.Log(string.Format("'{0}' topic's SimpleNotifications are set to FALSE",topic.Filter));
      Debug.Log("SimpleNotifications=FALSE give you the flexibility of receiving messages from various topics within the same receiver script");
      Debug.Log("Every notification will be in the format '[notification-method]_[topic-filter]'");

      tests(topic);
   }

   void tests(ITopic topic)
   {
      try
      {
         Debug.Log("Let's try subscribing to this topic without connecting to broker first....");
         topic.Subscribe();
      }
      catch(OperationCanceledException ocex)
      {
         Debug.LogError("Performing actions that require an active connection will throw an 'OperationCancelledException'");
      }

      topic.ConnectionManager.Connect();
   }

   void onMqttMessageDelivered_GlobalTopic(string messageId)
   {
      Debug.Log("message delivered to broker");
   }

   void onMqttMessageArrived_GlobalTopic(GlobalMessage message)
   {
      Debug.Log("Message arrived on GlobalTopic");
      Debug.Log("Note that the message parameter in the arrival notification is strong typed to that of the topic's message");

      if(!message.JSONConversionFailed)
         Debug.Log(JsonUtility.ToJson(message));
      else
         Debug.LogWarning("message arrived, but failed JSON conversion");
   }

   void onMqttSubscriptionSuccess_GlobalTopic(SubscriptionResponse response)
   {
      Debug.Log("subscription successful");

      Debug.Log("Let's send a message with a QOS of 'at least once' or 'exactly once'. " +
" 'Best effort' QOS does not get delivery verification from broker. " +
"'Best effort' is however the quickest and dirtiest way to send a message.");

      _cacheGlobalTopic.Send(
         new GlobalMessage { text = "this is text", number = 666, truth = true },
         false,
         QualityOfServiceEnum.AtLeastOnce);
      }

   void onMqttSubscriptionFailure_GlobalTopic(SubscriptionResponse response)
   {
      Debug.Log("subscription failed");
   }

   void onMqttUnsubscriptionSuccess_GlobalTopic(SubscriptionResponse response)
   {
      Debug.Log("unsubscription successful");
   }

   void onMqttUnsubscriptionFailure_GlobalTopic(SubscriptionResponse response)
   {
      Debug.Log("unsubscription failed");
   }

   void onMqttConnectSuccess_GlobalTopic(ConnectionResult response)
   {
      Debug.Log("you are connected to broker");
   }

   void onMqttConnectFailure_GlobalTopic(ConnectionResult response)
   {
      Debug.Log("connection to broker failed");
   }

   void onMqttConnectLost_GlobalTopic(ConnectionResult response)
   {
      Debug.Log("connection to broker lost");
   }

   void onMqttReconnect_GlobalTopic(ConnectionResult response)
   {
      Debug.Log("broker has reconnected");
   }
}
```
