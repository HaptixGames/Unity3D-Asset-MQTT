using UnityEngine;
using System.Collections;
using System;

namespace HG.iot.mqtt
{
	public interface ITopic
	{
		string UserDescription { get; }
		IBrokerConnection ConnectionManager { get; }
		Message DefaultMessage { get; set; }
		Type TopicType { get; }
		Type MessageType { get; }
		ITopicNotificaitons Notify { get; }
		MonoBehaviour Script { get; }
		int SubscriptionTimeout { get; }
		string UserDefined { get; }
		string Filter { get; }
		string FilterAtRuntime { get; }
		bool SubscribeOnConnect { get; }
		bool IsSubscribed { get; }
		bool AllowEmptyMessage { get; }
		QualityOfServiceEnum RequestedQualityOfService { get; }
		QualityOfServiceEnum GrantedQualityOfService { get; }
		TopicStats Statistics { get; }
		void SetFilter(string filter, bool revertAfterSend = false);
		void AppendFilter(string filter, bool revertAfterSend = false);
		void RevertFilter();
		void Subscribe(QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.Undefined);
		//string Send(TMessage message, bool isRetained = false, QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort);
		string Send(Message message, bool isRetained = false, QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort);
		string SendAndRemove(Message message, QualityOfServiceEnum qualityOfService);
		void Unsubscribe();
		void RemoveSelf();
	}
}