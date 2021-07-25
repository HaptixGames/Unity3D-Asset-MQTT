using UnityEngine;
using System.Collections;
using System.Linq;

namespace HG.iot.mqtt
{
	//http://git.eclipse.org/c/mosquitto/org.eclipse.mosquitto.git/tree/lib/util_mosq.c

	public static class StringExtensions
	{
		public static string Detokenize(this string topic, ConnectionOptions connectionOptions)
		{
			// TODO: regex
			return topic
				.Replace("{device_id}", connectionOptions.ClientId)
				.Replace("{client_id}", connectionOptions.ClientId)
				.Replace("{username}", connectionOptions.Username)
				.Replace("{host}", connectionOptions.Host);
		}

		public static bool IsValidMqttSubscriptionTopic(this string topic)
		{
            if (string.IsNullOrEmpty(topic))
                return false;

			var chars = topic.ToCharArray();
			char last_char = char.MinValue;
			char this_char = char.MinValue;
			char next_char = char.MinValue;

			for(int i=0; i<chars.Length; i++)
			{
				this_char = chars[i];

				if(i < chars.Length-1)
					next_char = chars[i+1];
				else
					next_char = char.MinValue;

				if(this_char == '+')
				{
					if((last_char != char.MinValue && last_char != '/') || (next_char != char.MinValue && next_char != '/'))
						return false;
				}
				else if(this_char == '#')
				{
					if((last_char != char.MinValue && last_char != '/') || next_char != char.MinValue)
						return false;
				}

				last_char = chars[i];
			}

			if(topic.Length > 65535) return false;

			return true;
		}

		public static bool IsVaidMqttPublishingTopic(this string topic)
		{
			int len = 0;

			foreach(var c in topic)
			{
				if(c=='+' || c=='#')
					return false;
			}

			if(topic.Length > 65535) return false;

			return true;
		}

		public static bool DoesFilterMatchTopic(this string sub, string topic)
		{
			int slen, tlen;
			int spos, tpos;
			bool multilevel_wildcard = false;

			if(string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(topic))
				return false;

			slen = sub.Length;
			tlen = topic.Length;

			if((sub[0] == '$' && topic[0] != '$') || (topic[0] == '$' && sub[0] != '$'))
				return false;

			spos = 0;
			tpos = 0;

			while(spos < slen && tpos < tlen) 
			{
				if(sub[spos] == topic[tpos])
				{
					if(tpos == tlen-1)
					{
						/* Check for e.g. foo matching foo/# */
						if(spos == slen-3 && sub[spos+1] == '/' && sub[spos+2] == '#')
						{
							multilevel_wildcard = true;
							return true;
						}
					}

					spos++;
					tpos++;

					if(spos == slen && tpos == tlen)
					{
						return true;
					}
					else if(tpos == tlen && spos == slen-1 && sub[spos] == '+')
					{
						spos++;
						return true;
					}
				}
				else
				{
					if(sub[spos] == '+')
					{
						spos++;
						while(tpos < tlen && topic[tpos] != '/')
						{
							tpos++;
						}

						if(tpos == tlen && spos == slen)
						{
							return true;
						}
					}
					else if(sub[spos] == '#')
					{
						multilevel_wildcard = true;

						if(spos+1 != slen)
						{
							return false;
						}
						else
						{
							return true;
						}
					}
					else
					{
						return false;
					}
				}
			}
				
			if(multilevel_wildcard == false && (tpos < tlen || spos < slen))
				return false;

			return true;
		}
	}
}