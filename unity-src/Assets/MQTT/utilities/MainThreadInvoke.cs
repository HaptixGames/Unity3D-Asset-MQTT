using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using HG.thirdparty;

namespace HG
{
    public class MainThreadInvoke : Singleton<MainThreadInvoke>
    {
        public void ProvideStack(bool state)
        {
            PROVIDE_STACK = state;
            Debug.Log("MTI STACK Enabled: " + state.ToString());
        }

        private bool PROVIDE_STACK = false;

		protected MainThreadInvoke () {} 

        private struct InvokeAction
        {
            public Action action;
            public string name;
        }

        private Queue<InvokeAction> _actions = new Queue<InvokeAction>();

		public void Add(Action action, string name = "unset")
		{
			lock(_actions)
			{
                _actions.Enqueue(new InvokeAction{ action = action, name = name });
                if (PROVIDE_STACK) Debug.Log("MTI queued: " + name);
			}
		}

		void Update()
		{
			lock(_actions)
			{
				while(_actions.Count>0)
				{
					InvokeAction ia = _actions.Dequeue();
                    if (PROVIDE_STACK) Debug.Log("MTI dequeued: " + ia.name);
					ia.action();
				}
			}
		}
	}
}