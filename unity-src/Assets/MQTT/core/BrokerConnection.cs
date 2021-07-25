using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using HG.thirdparty;

namespace HG.iot.mqtt
{
    public class BrokerConnection : Singleton<BrokerConnection>, IBrokerConnection, IBrokerConnectionNotifications
    {
        protected BrokerConnection() { }

        void Awake()
        {
            /*
			//https://github.com/LitJSON/litjson/issues/30
			hg.LitJson.JsonMapper.RegisterImporter<int, long>((int value) =>
            { 
                return (long)value;
            });
            */

            MainThreadInvoke.Instance.ProvideStack(this._provideDebugStack);

            _client = gameObject.GetComponent(typeof(IMqttClient)) as IMqttClient;

            if (_client == null)
            {
                Debug.LogError("Unable to find component implementing IMqttClient interface!");
                return;
            }
            else
            {
                Debug.Log("Found component implementing IMqttClient interface.");

                try
                {
                    _client.Init(this);
                }
                catch (PlatformNotSupportedException pnsex)
                {
                    Debug.LogError("Component implementing IMqttClient interface does not support the current platform.");
                }
            }
        }
        
        void Start()
		{
            notifyBootstrap(ReceiverEvent.READY, this);

            var id = Guid.NewGuid();
			foreach(var t in _topics)
				t.Notify.onMqttReady(id);

            notifyBootstrap(ReceiverEvent.BOOT, this);
		}

		[SerializeField]
		private GameObject _bootstrap = null;

		public IBrokerConnectionNotifications Notify {
			get {
				return (IBrokerConnectionNotifications)this;
			}
		}

        [SerializeField]
        private bool _queueMessagesWhenDisconnected = true;

        public bool QueueMessagesWhenDisconnected
        {
            get {
                return _queueMessagesWhenDisconnected;
            }
        }

        [SerializeField]
        private bool _provideDebugStack = false;

        [SerializeField]
        private List<GameObject> _receivers = new List<GameObject>();

        public IEnumerable<GameObject> Receivers
        {
            get
            {
                return _receivers;
            }
        }

        private void notifyBootstrap(ReceiverEvent @event, object payload)
        {
            if (_bootstrap == null)
                return;

            string methodName = string.Empty;

            switch (@event)
            {
                case ReceiverEvent.READY:
                    methodName = "onMqttReady";
                    break;
                case ReceiverEvent.BOOT:
                    methodName = "onMqttBoot";
                    break;
                case ReceiverEvent.CONNECT_SUCCESS:
                    methodName = "onMqttConnectSuccess";
                    break;
                case ReceiverEvent.CONNECT_FAILURE:
                    methodName = "onMqttConnectFailure";
                    break;
                case ReceiverEvent.CONNECT_LOST:
                    methodName = "onMqttConnectLost";
                    break;
                case ReceiverEvent.RECONNECT:
                    methodName = "onMqttReconnect";
                    break;
            }

            if (!string.IsNullOrEmpty(methodName))
                _bootstrap.SendMessage(methodName, payload, SendMessageOptions.DontRequireReceiver);
        }

        #region known topics

        private ConcurrentList<ITopic> _topics = new ConcurrentList<ITopic>();

		//TODO: detokenize filter
		public int ChangeTopicFilter<TTopic>(string oldValue, string newValue)
		{
			IEnumerable<ITopic> pms = GetTopicsByTopicType(typeof(TTopic));

			if(pms!=null)
			{
				foreach(ITopic t in pms)
					t.SetFilter(pms.First().Filter.Replace(oldValue,newValue));

				return pms.Count();
			}

			return 0;
		}

		public IEnumerable<ITopic> GetTopicsByTopicType(Type filter)
		{
			return _topics
				.Where(t => t.TopicType == filter);
		}

		public IEnumerable<ITopic> GetTopicsByMessageType(Type filter)
		{
			return _topics
				.Where(t => t.MessageType == filter);
		}

		//TODO: topic detokenize ?
		public IEnumerable<ITopic> GetTopicsByFilter(string filter)
		{
			return _topics
				.Where(t => t.Filter == filter);
		}

		public IEnumerable<ITopic> GetTopicsByUserDefined(string value)
		{
			return _topics
				.Where(t => t.UserDefined == value);
		}

        public ITopic AddTopic(GameObject parent, Type type, string filter, string alias,
            IEnumerable<GameObject> receivers = null,
            QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort,
            bool allowEmptyMessage = false,
            int subscriptionTimeout = 15,
            bool subscribeNow = false, 
            bool subscribeOnConnect = false)
        {
            ITopic topic = parent.AddComponent(type) as ITopic;
            (topic as ITopicRuntime).Configure(filter,alias,receivers,qualityOfService,allowEmptyMessage,subscriptionTimeout,subscribeOnConnect);
            if (subscribeNow) topic.Subscribe();
            return topic;
        }

        public void AddTopic(ITopic topic)
		{
			_topics.Add(topic);
		}

		public void RemoveTopic(ITopic topic)
		{
            if (topic.IsSubscribed)
                topic.Unsubscribe();
			_topics.Remove(topic);
		}

		#endregion

		#region client interaction

		private IMqttClient _client = null;

		private string _clientId = string.Empty;

		public string ClientId {
			get {
				return _clientId;
			}
		}

		[SerializeField]
		private ConnectionOptions _defaultConnectionOptions = new ConnectionOptions();

		public ConnectionOptions DefaultConnectionOptions {
			get {
				return _defaultConnectionOptions;
			}
		}

		public bool Connect (ConnectionOptions options = null)
		{
			if(options!=null) 
				_defaultConnectionOptions = options;

            // process client Id options and set client Id
            _defaultConnectionOptions.ClientId = _defaultConnectionOptions.ClientIdRuntime;

			return _client.Connect(_defaultConnectionOptions);
		}

		public bool Disconnect ()
		{
			if(!this.IsConnected)
				throw new OperationCanceledException("Connection object is not in a valid state.");

            //TODO: these are not going to unsubscribe in time
            /*foreach(ITopic topic in this._topics)
                if (topic.IsSubscribed)
                    topic.Unsubscribe();*/

            return _client.Disconnect();
		}

		public bool Reconnect ()
		{
			return _client.Reconnect();
		}
			
		public string SendAndRemove<TTopic,TMessage>(TMessage message, string filter, QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
			where TTopic : TopicBehaviour
			where TMessage : Message
		{
			ITopic dyn = this.gameObject.AddComponent<TTopic>() as ITopic;
			dyn.SetFilter(filter.Detokenize(_defaultConnectionOptions));
			return dyn.SendAndRemove(message, qualityOfService);
		}

		public string Send(ITopic topic, string message, bool isRetained = false, QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
		{
			Debug.LogFormat("connection:sending:: '{0}' => '{1}'", message,topic.FilterAtRuntime);

			if(!this.IsConnected)
				throw new OperationCanceledException("Connection object is not in a valid state.");

            var id = Guid.NewGuid();
            var wrappedMessage = new OutboundMessage { 
				Topic = topic,
				Message = message,
				IsRetained = isRetained,
				QualityOfService = qualityOfService,
				OnSuccess = (m) => {
                    topic.Notify.onMqttMessageDelivered(
                        id,
                        new DeliveryResponse {
                            Id = m.Id,
                            WasDelivered = m.WasDelivered
                        });
                },
				OnFailure = (m) => {
                    topic.Notify.onMqttMessageNotDelivered(
                        id,
                        new DeliveryResponse {
                            Id = m.Id,
                            WasDelivered = m.WasDelivered
                        });
                }
			};

			OutboundMessage wm = _client.Send(wrappedMessage);

			return wm.Id;
		}

		private bool _isConnected = false;

		public bool IsConnected {
			get {
				return _isConnected;
			}
		}
			
		public string Subscribe(ITopic topic)
		{
			if(!this.IsConnected)
				throw new OperationCanceledException("Connection object is not in a valid state.");

            var id = Guid.NewGuid();
            var wrappedSub = new WrappedSubscription {
				Topic = topic,
				OnSuccess = (s) => { 
					topic.Notify.onMqttSubscriptionSuccess( 
                        id,
						new SubscriptionResponse {
							Id = s.Id,
							GrantedQualityOfService = s.GrantedQualityOfService
						}); 
				},
				OnFailure = (s) => { 
					topic.Notify.onMqttSubscriptionFailure( 
                        id,
						new SubscriptionResponse {
							Id = s.Id,
							ErrorCode = s.ErrorCode,
							ErrorMessage = s.ErrorMessage
						}); 
				}
			};
            
            WrappedSubscription ws = _client.Subscribe(wrappedSub);

			return ws.Id;
		}

		public string Unsubscribe(ITopic topic)
		{
			if(!this.IsConnected)
				throw new OperationCanceledException("Connection object is not in a valid state.");

            var id = Guid.NewGuid();
			var wrappedSub = new WrappedSubscription {
				Topic = topic,
				OnSuccess = (s) => { 
					topic.Notify.onMqttUnsubscriptionSuccess(
                        id,
						new SubscriptionResponse {
							Id = s.Id
						}); 
				},
				OnFailure = (s) => { 
					topic.Notify.onMqttUnsubscriptionFailure(
                        id, 
						new SubscriptionResponse {
							Id = s.Id,
							ErrorCode = s.ErrorCode,
							ErrorMessage = s.ErrorMessage
						}); 
				}
			};

			WrappedSubscription ws = _client.Unsubscribe(wrappedSub);

			return ws.Id;
		}

		// dispatch inbound message to correct topics
		public void onMessageArrived(InboundMessage message)
		{
			Debug.LogFormat("connection:receiving:: '{0}' => '{1}'", message.Message, message.Topic);

            var id = Guid.NewGuid();
            foreach (var topic in _topics)
			{
				if(topic.FilterAtRuntime.DoesFilterMatchTopic(message.Topic))
					topic.Notify.onMqttMessageArrived(id, message.Topic,message.Message);
			}
		}
			
		#endregion

		#region pass connection related notifications from client to all topics

		public void onConnectSuccess(ConnectionResult result)
		{
			_clientId = result.ClientId;
			_isConnected = true;
            
			MainThreadInvoke.Instance.Add(() => {
                notifyBootstrap(ReceiverEvent.CONNECT_SUCCESS, result);
            },"(BrokerConn.onConnectSuccess)");

            var id = Guid.NewGuid();
            foreach (var t in _topics)
				t.Notify.onMqttConnectSuccess(id, result);
		}

		public void onConnectFailure(ConnectionResult result)
		{
			_isConnected = false;

			MainThreadInvoke.Instance.Add(() => {
                notifyBootstrap(ReceiverEvent.CONNECT_FAILURE, result);
            }, "(BrokerConn.onConnectFailure)");

            var id = Guid.NewGuid();
            foreach (var t in _topics)
				t.Notify.onMqttConnectFailure(id, result);
		}

		public void onConnectLost(ConnectionResult result)
		{
			_isConnected = false;
            
			MainThreadInvoke.Instance.Add(() => {
                notifyBootstrap(ReceiverEvent.CONNECT_LOST, result);
            }, "(BrokerConn.onConnectLost)");

            var id = Guid.NewGuid();
            foreach (var t in _topics)
				t.Notify.onMqttConnectLost(id, result);
		}

		public void onReconnect(ConnectionResult result)
		{
			_isConnected = true;
            
			MainThreadInvoke.Instance.Add(() => {
                notifyBootstrap(ReceiverEvent.RECONNECT, result);
            }, "(BrokerConn.onReconnect)");

            var id = Guid.NewGuid();
            foreach (var t in _topics)
				t.Notify.onMqttReconnect(id, result);
		}
        
        #endregion
    }
}