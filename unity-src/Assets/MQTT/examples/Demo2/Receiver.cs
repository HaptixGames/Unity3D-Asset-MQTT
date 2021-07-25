using HG.iot.mqtt;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace hg.iot.mqtt.example.demo2
{
    public class Receiver : MonoBehaviour
    {
        public GameObject scrollViewPrefab;
        public ScrollRect scrollRect;
        public GameObject scrollViewContent;
        public Image imgConnectStatus;
        public Text txtBrokerInfo;
        public Button btnConnect;
        public Text txtFilter;
        public Button btnSubscribe;
        public Text txtMessage;

        private IBrokerConnection _connection;

        private void Awake()
        {
            _connection = BrokerConnection.Instance;
        }

        public void clickConnect()
        {
            if (_connection.IsConnected)
                _connection.Disconnect();
            else
            {
                Uri uri = null;
                int port = 1883;

                if (Uri.TryCreate(txtBrokerInfo.text, UriKind.Absolute, out uri))
                {
#if UNITY_WEBGL
                    if (uri.Port < 0)
                        port = 61614;
#endif

                    if (uri.Port > 0)
                        port = uri.Port;

                    _connection.DefaultConnectionOptions.Host = uri.Host;
                    _connection.DefaultConnectionOptions.Port = port;
                    if (!string.IsNullOrEmpty(uri.LocalPath))
                        _connection.DefaultConnectionOptions.Path = uri.LocalPath;

                    _connection.Connect();
                }
                else
                {
                    addScrollviewMessage("ERROR", "Invalid broker URI");
                }
            }
        }

        public void filterEditing(string filter)
        {
            var topics = _connection.GetTopicsByFilter(filter);
            if(topics.Count()>0 && topics.First().IsSubscribed==true)
                btnSubscribe.GetComponentInChildren<Text>().text = "Unsubscribe";
            else
                btnSubscribe.GetComponentInChildren<Text>().text = "Subscribe";
        }

        public void clickSubscribe()
        {
            if (txtFilter.text.IsValidMqttSubscriptionTopic())
            {
                var topics = _connection.GetTopicsByFilter(txtFilter.text);

                if (topics.Count()==0)
                {
                    var topicsParent = GameObject.Find("topics");
                    var receivers = new List<GameObject> { GameObject.Find("receivers") };
                    try
                    {
                        var newTopic = _connection.AddTopic(topicsParent, typeof(GenericTopic), txtFilter.text, "none", receivers, subscribeNow: true, subscribeOnConnect: true);
                    } catch (OperationCanceledException ocex) {
                        addScrollviewMessage("WARNING", "Topic was added but not subscribed because broker is disconnected");
                    } catch (Exception ex) {
                        addScrollviewMessage("ERROR", ex.Message);
                    }
                }
                else if(topics.Count()>0 && topics.First().IsSubscribed==false)
                {
                    try
                    { 
                        topics.First().Subscribe();
                    }
                    catch (OperationCanceledException ocex)
                    {
                        addScrollviewMessage("WARNING", "Topic already exists but not subscribed because broker is disconnected");
                    }
                    catch (Exception ex)
                    {
                        addScrollviewMessage("ERROR", ex.Message);
                    }
                }
                else if (topics.Count() > 0 && topics.First().IsSubscribed == true)
                {
                    topics.First().Unsubscribe();
                }
            }
            else
                addScrollviewMessage("ERROR", "Invalid subscription filter: '" + txtFilter.text + "'");
        }
        
        public void clickPublish()
        {
            if (txtFilter.text.IsVaidMqttPublishingTopic())
            {
                GenericMessage m = new GenericMessage { text = txtMessage.text };
                m.SendOnce<GenericTopic, GenericMessage>(txtFilter.text, QualityOfServiceEnum.AtLeastOnce);
            }
            else
                addScrollviewMessage("ERROR", "Invalid publishing filter: '" + txtFilter.text + "'");
        }

        void addScrollviewMessage(string line1, string line2)
        {
            GameObject newObj;
            newObj = (GameObject)Instantiate(scrollViewPrefab, scrollViewContent.transform);
            Text[] children = newObj.GetComponentsInChildren<Text>();
            children[0].text = line1;
            children[1].text = line2;
            children[2].text = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + " UTC";
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0;
        }

        //TODO: meh
        List<Guid> _lastTenEvents = new List<Guid>();

        void onMqttEvent(MqttEvent @event)
        {
            if (_lastTenEvents.Count() > 10)
                _lastTenEvents.RemoveAt(0);

            if (_lastTenEvents.Contains(@event.ID))
                return;

            _lastTenEvents.Add(@event.ID);

            Debug.Log("[t/event] id: " + @event.ID.ToString() + ", event: " + @event.EVENT + ", data: " + @event.DATA.GetType().FullName);

            addScrollviewMessage("(MQTT) " + @event.EVENT.ToString(), string.Format("({0}) {1}", @event.DATA.GetType().FullName, @event.DATA.ToString()));
            
            switch(@event.EVENT)
            {
                case ReceiverEvent.CONNECT_SUCCESS:
                    txtBrokerInfo.GetComponentInParent<InputField>().readOnly = true;
                    btnConnect.GetComponentInChildren<Text>().text = "Disconnect";
                    imgConnectStatus.color = Color.green;
                    break;

                case ReceiverEvent.CONNECT_LOST:
                    txtBrokerInfo.GetComponentInParent<InputField>().readOnly = false;
                    btnConnect.GetComponentInChildren<Text>().text = "Connect";
                    imgConnectStatus.color = Color.red;
                    break;

                case ReceiverEvent.SUBSCRIPTION_SUCCESS:
                    var t1 = _connection.GetTopicsByFilter(txtFilter.text);
                    if (t1.Count()>0 && t1.First().IsSubscribed==true)
                        btnSubscribe.GetComponentInChildren<Text>().text = "Unsubscribe";
                    break;

                case ReceiverEvent.UNSUBSCRIPTION_SUCCESS:
                    var t2 = _connection.GetTopicsByFilter(txtFilter.text);
                    if (t2.Count() > 0 && t2.First().IsSubscribed == false)
                        btnSubscribe.GetComponentInChildren<Text>().text = "Subscribe";
                    break;

            }
        }
    }
}