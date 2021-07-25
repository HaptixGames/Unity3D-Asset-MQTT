using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Linq;

namespace HG.iot.mqtt
{
	public abstract class TopicBehaviour : MonoBehaviour
	{

	}
		
	public abstract class Topic<TMessage> : TopicBehaviour, ITopic, ITopicNotificaitons, ITopicRuntime
		where TMessage : Message
	{
        public override string ToString()
        {
            return string.Format("filter: {0}", this.FilterAtRuntime);
        }

        protected ITopic topic {
			get {
				return this;
			}
		}

        public void Configure(string filter, string alias, IEnumerable<GameObject> receivers, QualityOfServiceEnum qualityOfService, bool allowEmptyMessage, int subscriptionTimeout, bool subscribeOnConnect)
        {
            this.SetFilter(filter, false);
            this.userDefined = alias;
            if(receivers != null)  this.Receivers = receivers.ToList();
            this.requestedQualityOfService = qualityOfService;
            this.allowEmptyMessage = allowEmptyMessage;
            this.subscriptionTimeout = subscriptionTimeout;
            this.subscribeOnConnect = subscribeOnConnect;
        }

        void OnValidate()
		{
			if(this.GetType().IsDefined(typeof(DescriptionAttribute), true))
			{
				userDescription = 
					((DescriptionAttribute) 
						this.GetType().GetCustomAttributes(typeof(DescriptionAttribute),true)[0])
						.Description;
			}
		}

		void Awake()
		{
			OnValidate();

			Debug.LogFormat("topic:awake::{0} waking up with filter '{1}'", this.TopicType.Name, this.Filter);

			_connection = BrokerConnection.Instance;

			if(_connection==null)
			{
				Debug.LogErrorFormat("topic:awake::{0} unable to find component implementing IMqttConnection interface!", this.TopicType.Name);
				return;
			}
			else
			{
				Debug.LogFormat("topic:awake::{0} found component implementing IMqttConnection interface.", this.TopicType.Name);
				_connection.AddTopic(this);
			}

			if(!this.FilterAtRuntime.IsVaidMqttPublishingTopic())
			{
				Debug.LogWarningFormat("topic:validate::{0} '{1}' is NOT a valid publishing filter.", this.TopicType.Name, this.Filter);
			}

			if(!this.FilterAtRuntime.IsValidMqttSubscriptionTopic())
			{
				Debug.LogWarningFormat("topic:validate::{0} '{1}' is NOT a valid subscription filter.", this.TopicType.Name, this.Filter);
			}
		}

		private IBrokerConnection _connection;

		protected Type topicType { 
			get {
				return this.GetType();
			}
		}

		private Type messageType {
			get {
				return typeof(TMessage);
			}
		}

		[SerializeField]
		protected List<GameObject> Receivers = new List<GameObject>();
        
        protected virtual void notifyReceivers(Guid id, ReceiverEvent @event,  object payload)
        {
            string methodName = string.Empty;

            switch (@event)
            {
                case ReceiverEvent.READY:
                    methodName = "onMqttReady";
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
                case ReceiverEvent.MESSAGE_ARRIVED:
                    methodName = "onMqttMessageArrived";
                    break;
                case ReceiverEvent.MESSAGE_DELIVERED:
                    methodName = "onMqttMessageDelivered";
                    break;
                case ReceiverEvent.MESSAGE_NOT_DELIVERED:
                    methodName = "onMqttMessageNotDelivered";
                    break;
                case ReceiverEvent.SUBSCRIPTION_FAILURE:
                    methodName = "onMqttSubscriptionFailure";
                    break;
                case ReceiverEvent.SUBSCRIPTION_SUCCESS:
                    methodName = "onMqttSubscriptionSuccess";
                    break;
                case ReceiverEvent.UNSUBSCRIPTION_FAILURE:
                    methodName = "onMqttUnsubscriptionFailure";
                    break;
                case ReceiverEvent.UNSUBSCRIPTION_SUCCESS:
                    methodName = "onMqttUnsubscriptionSuccess";
                    break;
            }

            var evt = new MqttEvent { ID = id, EVENT = @event, DATA = payload };

            foreach (var go in Receivers)
            {
                MainThreadInvoke.Instance.Add(() =>
                {
                    go.SendMessage("onMqttEvent", evt, SendMessageOptions.DontRequireReceiver);
                    //if (evt.Handled) return;
                    go.SendMessage(methodName, payload, SendMessageOptions.DontRequireReceiver);
                    go.SendMessage(methodName+"_"+topicType.Name, payload, SendMessageOptions.DontRequireReceiver);
                }, "(TR.Topic.notifyReceivers." + topicType.Name + ")" + methodName + "/" + (topic == null ? "NONE" : topicType.Name));
            }
            
            foreach (var go in this.ConnectionManager.Receivers)
            {
                // skip any receivers that might exist in topic and in conn mgr
                if (this.Receivers.Select(r => r.GetHashCode()).Contains(go.GetHashCode()))
                    continue;

                MainThreadInvoke.Instance.Add(() =>
                {
                    go.SendMessage("onMqttEvent", evt, SendMessageOptions.DontRequireReceiver);
                    //if (evt.Handled) return;
                    go.SendMessage(methodName, payload, SendMessageOptions.DontRequireReceiver);
                    go.SendMessage(methodName + "_" + topicType.Name, payload, SendMessageOptions.DontRequireReceiver);
                }, "(CR.Topic.notifyReceivers." + topicType.Name + ")" + methodName + "/" + (topic == null ? "NONE" : topicType.Name));
            }
        }
        
        protected struct QueuedMessage
		{
			public Message Message;
			public bool IsRetained;
			public QualityOfServiceEnum QualityOfService;
		}

		protected Queue<QueuedMessage> outboundQueue = new Queue<QueuedMessage>();

		protected virtual void processOutboundQueue()
		{
			if(outboundQueue.Count>0)
				Debug.LogFormat("topic:queue::{0} processing {1} oubound message(s) on '{2}'", this.TopicType.Name, outboundQueue.Count, this.FilterAtRuntime);

			while(outboundQueue.Count>0)
			{
				var qm = outboundQueue.Dequeue();
				_connection.Send(this, JsonUtility.ToJson(qm.Message), qm.IsRetained, qm.QualityOfService);
			}
		}

		#region ITopic

		public void RemoveSelf ()
		{
			MainThreadInvoke.Instance.Add(() => {
				this.ConnectionManager.RemoveTopic(this);
				Destroy(this);
			});
		}

		[SerializeField]
		protected TMessage defaultMessage = Activator.CreateInstance<TMessage>();

		public Message DefaultMessage {
			get {
				return defaultMessage;
			}

			set {
				defaultMessage = (TMessage) value;
			}
		}

		[SerializeField]
		protected TopicStats statistics = new TopicStats();

		public TopicStats Statistics {
			get {
				return statistics;
			}
		}

		public IBrokerConnection ConnectionManager {
			get {
				return _connection;
			}
		}

		public ITopicNotificaitons Notify {
			get {
				return (ITopicNotificaitons)this;
			}
		}

		public MonoBehaviour Script {
			get {
				return this;
			}
		}

		public Type TopicType { 
			get {
				return this.GetType();
			}
		}

		public Type MessageType { 
			get {
				return typeof(TMessage);
			}
		}

		[SerializeField]
		protected string userDescription = string.Empty;

		public string UserDescription {
			get {
				return userDescription;
			}
		}

		[SerializeField]
		protected int subscriptionTimeout = 15;

		public int SubscriptionTimeout {
			get {
				return subscriptionTimeout;
			}
		}

		[SerializeField]
		protected string userDefined = string.Empty;

		public string UserDefined {
			get {
				return userDefined;
			}
		}
			
		protected bool revertFilterAfterSend = false;

		protected string previousFilter = string.Empty;

		[SerializeField]
		protected string filter = string.Empty;

		public string Filter {
			get {
				return filter;
			}
		}

		public virtual string FilterAtRuntime {
			get {
				return filter.Detokenize(_connection.DefaultConnectionOptions);
			}
		}

		public void SetFilter (string newFilter, bool revertAfterSend = false)
		{
			Debug.LogFormat("topic:set-filter::{0} '{1}' => '{2}'", this.TopicType.Name, filter, newFilter);
			previousFilter = filter;
			filter = newFilter;
			revertFilterAfterSend = revertAfterSend;
		}

		public virtual void AppendFilter(string appendage, bool revertAfterSend = false)
		{
			Debug.LogFormat("topic:append-filter::{0} '{1}' => '{2}{3}'", this.TopicType.Name, this.FilterAtRuntime, this.FilterAtRuntime, appendage);
			previousFilter = filter;
			filter = filter + appendage;
			revertFilterAfterSend = revertAfterSend;
		}

		public virtual void RevertFilter()
		{
			if(revertFilterAfterSend==false)
			{
				Debug.LogWarningFormat("topic:revert-filter::{0} '{1}' => 'empty' is not set", this.TopicType.Name, this.FilterAtRuntime);
				return;
			}

			revertFilterAfterSend = false;

			if(previousFilter==string.Empty)
			{
				Debug.LogWarningFormat("topic:revert-filter::{0} '{1}' => 'empty' is not allowed", this.TopicType.Name, this.FilterAtRuntime);
				return;
			}

			Debug.LogFormat("topic:revert-filter::{0} '{1}' => '{2}'", this.TopicType.Name, this.FilterAtRuntime, previousFilter);
			filter = previousFilter;
			previousFilter = string.Empty;
		}

		[SerializeField]
		protected bool subscribeOnConnect = true;

		public bool SubscribeOnConnect {
			get {
				return subscribeOnConnect;
			}
		}

		[SerializeField]
		protected bool allowEmptyMessage = false;

		public bool AllowEmptyMessage {
			get {
				return allowEmptyMessage;
			}
		}

		protected bool isSubscribed = false;

		public virtual bool IsSubscribed {
			get {
				return isSubscribed;
			}
			private set {
				isSubscribed = value;
			}
		}
			
		[SerializeField]
		protected QualityOfServiceEnum requestedQualityOfService = QualityOfServiceEnum.BestEffort;

		public QualityOfServiceEnum RequestedQualityOfService {
			get {
				return requestedQualityOfService;
			}
		}

        //TODO: make drawer so this field is visible and readonly in editor
		private QualityOfServiceEnum _grantedQualityOfService = QualityOfServiceEnum.BestEffort;

		public QualityOfServiceEnum GrantedQualityOfService {
			get {
				return _grantedQualityOfService;
			}
		}

		public virtual void Subscribe(QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.Undefined) 
		{
			if(!this.FilterAtRuntime.IsValidMqttSubscriptionTopic())
			{
				Debug.LogErrorFormat("topic:subscribe::{0} '{1}' is NOT a valid subscription filter. Cancelling subscription!",this.TopicType.Name, this.FilterAtRuntime);
				return;
			}

			if(!_connection.IsConnected)
				throw new OperationCanceledException("Connection object is not in a valid state.");

			if(qualityOfService != QualityOfServiceEnum.Undefined)
				requestedQualityOfService = qualityOfService;

			if(requestedQualityOfService == QualityOfServiceEnum.Undefined)
				requestedQualityOfService = QualityOfServiceEnum.BestEffort;

			_connection.Subscribe(this);
		}

		/*public virtual string Send(TMessage Message, bool isRetained = false, QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
		{
			return topic.Send(Message as Message, isRetained, qualityOfService);
		}*/

		public string SendAndRemove (Message message, QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
		{
			string msgId = topic.Send(message, false, qualityOfService);
			this.RemoveSelf();
			return msgId;
		}

		public virtual string Send(Message message, bool isRetained = false, QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
		{
			message.Topic = topic;

			if(!this.FilterAtRuntime.IsVaidMqttPublishingTopic())
			{
				Debug.LogErrorFormat("topic:send::{0} '{1}' is NOT a valid publishing filter. Dropping message!",this.TopicType.Name, this.FilterAtRuntime);
				statistics.MessagesNotSent += 1;
				return string.Empty;
			}

			statistics.MessagesQueued += 1;

            if (!_connection.IsConnected)
            {
                //TODO: unfortunately when publishing to non-editor topics (send once), the queue gets lost and will not be processed on connect
                if (_connection.QueueMessagesWhenDisconnected)
                {
                    outboundQueue.Enqueue(new QueuedMessage { Message = message, QualityOfService = qualityOfService });
                    Debug.LogWarningFormat("topic:send::{0} connection to broker is not available. Your message has been queued for later delivery.", this.TopicType.Name);
                }
                else
                    statistics.MessagesQueued -= 1;

                return string.Empty;
			}

			string payload = onMqttMessageSerialize(message);

			if(string.IsNullOrEmpty(payload) && !allowEmptyMessage)
			{
				Debug.LogErrorFormat("topic:send::{0} '{1}' does not allow empty messages!", this.TopicType.Name, this.FilterAtRuntime);
				return string.Empty;
			}

			string result = _connection.Send(this, payload, isRetained, qualityOfService);

			if(revertFilterAfterSend) 
				RevertFilter();

			return result;
		}

		public virtual void Unsubscribe() 
		{
			if(!_connection.IsConnected)
				throw new OperationCanceledException("Connection object is not in a valid state.");

			_connection.Unsubscribe(this);
		}

		#endregion

		#region IITopic

		public virtual void onMqttReady(Guid id) 
		{
            notifyReceivers(id, ReceiverEvent.READY, this.topic);
		}

		public virtual void onMqttMessageDelivered(Guid id, DeliveryResponse response) 
		{
			Debug.LogFormat("topic:delivered::{0} msg-id:'{1}' => '{2}'", this.TopicType.Name, response.Id, topic.FilterAtRuntime);

			statistics.MessagesSent += 1;

            response.Topic = this;
            notifyReceivers(id, ReceiverEvent.MESSAGE_DELIVERED, response);
        }

		public virtual void onMqttMessageNotDelivered(Guid id, DeliveryResponse response) 
		{
			Debug.LogFormat("topic:not-delivered::{0} msg-id:'{1}' => '{2}'", this.TopicType.Name, response.Id, topic.FilterAtRuntime);

			statistics.MessagesNotSent += 1;

            response.Topic = this;
            notifyReceivers(id, ReceiverEvent.MESSAGE_NOT_DELIVERED, response);
        }

        public virtual void onMqttMessageArrived(Guid id, string arrivalTopic, string message)
		{
			if(!isSubscribed)
			{
				statistics.MessagesDropped += 1;
				Debug.LogWarningFormat("topic:arrived::{0} message arrived => '{1}' but was dropped because the topic is not subscribed.", topicType.Name, this.FilterAtRuntime);
				return;
			}

			statistics.MessagesReceived += 1;

			Message msg = onMqttMessageDeserialize(arrivalTopic, message);

			msg.Topic = topic;
			msg.ArrivedTopic = arrivalTopic;
			msg.OriginalMessage = message;
			statistics.AddTransitTime(msg.SetArrivalTimestamp());

            notifyReceivers(id, ReceiverEvent.MESSAGE_ARRIVED, new MessageArrival { Topic = this.topic, Payload = msg });
        }

        public virtual void onMqttSubscriptionSuccess(Guid id, SubscriptionResponse response) 
		{
            _grantedQualityOfService = response.GrantedQualityOfService;

			isSubscribed = true;

            response.Topic = this;
            notifyReceivers(id, ReceiverEvent.SUBSCRIPTION_SUCCESS, response);;
        }

        public virtual void onMqttSubscriptionFailure(Guid id, SubscriptionResponse response) 
		{
            response.Topic = this;
            notifyReceivers(id, ReceiverEvent.SUBSCRIPTION_FAILURE, response);
        }

        public virtual void onMqttUnsubscriptionSuccess(Guid id, SubscriptionResponse response) 
		{
			isSubscribed = false;

            response.Topic = this;
            notifyReceivers(id, ReceiverEvent.UNSUBSCRIPTION_SUCCESS, response);
        }

        public virtual void onMqttUnsubscriptionFailure(Guid id, SubscriptionResponse response) 
		{
            response.Topic = this;
            notifyReceivers(id, ReceiverEvent.UNSUBSCRIPTION_FAILURE, response);
        }

        public virtual void onMqttConnectSuccess(Guid id, ConnectionResult result)
		{
			if(subscribeOnConnect)
				topic.Subscribe();

			processOutboundQueue();

            notifyReceivers(id, ReceiverEvent.CONNECT_SUCCESS, result);
        }

        public virtual void onMqttConnectFailure(Guid id, ConnectionResult result)
		{
            notifyReceivers(id, ReceiverEvent.CONNECT_FAILURE, result);
        }

        public virtual void onMqttConnectLost(Guid id, ConnectionResult result)
		{
            //TODO: connection clean session should determine subscription and subscibe on connect
            //HACK: force subscription flag because when we disconnect, we cannot guarantee Unsubscribe will return
            this.isSubscribed = false;
            notifyReceivers(id, ReceiverEvent.CONNECT_LOST, result);
        }

        public virtual void onMqttReconnect(Guid id, ConnectionResult result)
		{
            notifyReceivers(id, ReceiverEvent.RECONNECT, result);
        }

        public virtual Message onMqttMessageDeserialize(string arrivalTopic, string message)
		{
			TMessage msg = null;

			try
			{
				//msg = hg.LitJson.JsonMapper.ToObject<TMessage>(message);
				msg = JsonUtility.FromJson<TMessage>(message);

				if(msg==null)
				{
					msg = Activator.CreateInstance<TMessage>();
					msg.SerializationFailed = true;
					msg.ArrivedEmpty = true;
				}
			}
			catch(ArgumentException aex)
			{
				if(!allowEmptyMessage)
					Debug.LogErrorFormat("topic:arrived::{0} '{1}' message failed deserialization from string '{2}'", topicType.Name, this.MessageType.Name, message);

				msg = Activator.CreateInstance<TMessage>();
				msg.SerializationFailed = true;
			}
			catch(Exception ex)
			{
				msg = Activator.CreateInstance<TMessage>();
				msg.SerializationFailed = true;

				Debug.LogError("TOPIC DESERIALIZER FAILED!");
				Debug.LogError(ex.Message);
			}

			return msg;
		}

		public virtual string onMqttMessageSerialize(Message message)
		{
			string payload = string.Empty;

			try
			{
				payload = JsonUtility.ToJson(message);
			}
			catch(ArgumentException aex)
			{
				Debug.LogErrorFormat("topic:send::{0} '{1}' message failed serialization from object '{2}'", this.TopicType.Name, this.MessageType.Name, message);
			
				message.SerializationFailed = true;
			}

			return payload;
		}

        #endregion
    }
}