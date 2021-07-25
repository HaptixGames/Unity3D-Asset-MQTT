using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace HG.iot.mqtt
{
	public static class MessageExtensions
	{
		public static Message Send(
			this Message message, 
			IEnumerable<ITopic> topics, 
			bool isRetained = false, 
			QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
		{
			if(topics==null) {
				Debug.LogWarningFormat("Unable to find valid topics for '{0}' message.",message.GetType());
				return null;
			}

			foreach(var t in topics)
			{
				t.Send(message, isRetained, qualityOfService);
			}

			return message;
		}

		public static Message SendByUserDefined(
			this Message message, 
			string userDefinedTopicValue, 
			bool isRetained = false, 
			QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
		{
			return Send(
				message,
				BrokerConnection.Instance.GetTopicsByUserDefined(userDefinedTopicValue),
				isRetained,
				qualityOfService);
		}

		public static Message SendByFilter(
			this Message message, 
			string topicFilter, 
			bool isRetained = false, 
			QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
		{
			return Send(
				message,
				BrokerConnection.Instance.GetTopicsByFilter(topicFilter),
				isRetained,
				qualityOfService);
		}

		public static Message SendByMessageType(
			this Message message, 
			Type messageType, 
			bool isRetained = false, 
			QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
		{
			return Send(
				message,
				BrokerConnection.Instance.GetTopicsByMessageType(messageType),
				isRetained,
				qualityOfService);
		}

		public static Message SendByMessageType(
			this Message message,
			bool isRetained = false, 
			QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
		{
			return Send(
				message,
				BrokerConnection.Instance.GetTopicsByMessageType(message.GetType()),
				isRetained,
				qualityOfService);
		}

		public static Message SendByTopicType<TTopic>(
			this Message message,
			bool isRetained = false, 
			QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
		{
			return Send(
				message,
				BrokerConnection.Instance.GetTopicsByTopicType(typeof(TTopic)),
				isRetained,
				qualityOfService);
		}

		public static string SendOnce<TTopic,TMessage>(this TMessage message, string filter, QualityOfServiceEnum qualityOfService = QualityOfServiceEnum.BestEffort)
			where TTopic : TopicBehaviour
			where TMessage : Message
		{
			return BrokerConnection.Instance.SendAndRemove<TTopic,TMessage>(message, filter, qualityOfService);
		}

		public static string ToJson(this Message message)
		{
			return JsonUtility.ToJson(message);
		}
	}
}