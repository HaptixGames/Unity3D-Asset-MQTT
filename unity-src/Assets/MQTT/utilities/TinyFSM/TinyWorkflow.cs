using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

namespace HG.TinyFSM
{ 
	public abstract class WorkflowStateObject
	{
		public IWorkflow Workflow;
		private Dictionary<string,System.Object> _bucket;
		
		public System.Action<bool>[] StepResultCallbacks { get; private set; }
		
		public WorkflowStateObject(params System.Action<bool>[] stepResultCallbacks): this()
		{
			StepResultCallbacks = stepResultCallbacks;
		}
		
		public WorkflowStateObject()
		{
			_bucket = new Dictionary<string, System.Object>();
		}
		
		public object this[string key]
		{
			get
			{
				if(_bucket.ContainsKey(key))
					return _bucket[key];
				else
					return null;
			}
			
			set
			{
				if(_bucket.ContainsKey(key))
					_bucket[key] = value;
				else 
					_bucket.Add(key,value);
			}
		}
		
		public void SetResultCallbacks(params System.Action<bool>[] stepResultCallbacks)
		{
			StepResultCallbacks = stepResultCallbacks;
		}
		
		public virtual void OnWorkflowStart()
		{
		
		}
		
		public virtual void OnWorkflowStop()
		{
		
		}
		
		public virtual void OnWorkflowStepCompletion(int stepNumber, bool success, WorkflowStateObject state)
		{
			
		}
	}
	
	
	public interface IWorkflow
	{
		string CurrentWorkflowName { get; }
	
		void StartWorkflow(string name, object stateObject);
		void StepComplete(bool success = true);
		void RepeatStep();
		void NextStep(bool success = true, bool notifyCompletion = true);
		void Stop();
		Coroutine StartCoroutine(IEnumerator routine);
		
		#if UNITY_EDITOR
		void SubscribeEditorForUpdates();
		void UnsubscribeEditorFromUpdates();
		#endif
	}
	
	
	public class Workflow<T> : IWorkflow 
		where T : WorkflowStateObject
	{
		public Dictionary<string,List<System.Action<T>>> Workflows;
		
		private Action<T> _currentStep = null;
		private int _currentStepNumber = -1;
		private Queue<Action<T>> _steps;
		public T StateObject { get; private set; }
		
		private MonoBehaviour _behaviorHost;
		
		public string CurrentWorkflowName { get; private set; }
		public bool IsRunning { get; private set; }
		
		#if UNITY_EDITOR
		public void SubscribeEditorForUpdates()
		{
			(_behaviorHost as HG.TinyFSM.EditorProgressionBehavior).Subscribe();
		}
		
		public void UnsubscribeEditorFromUpdates()
		{
			(_behaviorHost as HG.TinyFSM.EditorProgressionBehavior).Unsubscribe();
		}
		#endif
		
		public Workflow()
		{
			Configuration.Log("[TinyWorkflow] Initializing",LogSeverity.VERBOSE);
		}
		
		public void StartWorkflow(string name, object stateObject)
		{		
			Configuration.Log("[TinyWorkflow] Starting '" + name + "' workflow.",LogSeverity.VERBOSE);
			
			if(IsRunning)
			{
				Configuration.Log("[TinyWorkflow] This workflow instance is currently executing the '" + CurrentWorkflowName + "' workflow.",LogSeverity.ERROR);
				return;
			}
			
			if(!Workflows.ContainsKey(name))
			{
				Configuration.Log("[TinyWorkflow] This workflow '" + name + "' could not be located.",LogSeverity.ERROR);
				return;
			}
			
			if(stateObject==null)
			{
				Configuration.Log("[TinyWorkflow] Cannot start workflow with a null state object.",LogSeverity.ERROR);
				return;
			}
		
			IsRunning = true;
		
			if(_behaviorHost==null)
			{
				#if UNITY_EDITOR
				_behaviorHost = (HG.TinyFSM.EditorProgressionBehavior)Configuration.Bootstrap().AddComponent(typeof(HG.TinyFSM.EditorProgressionBehavior));
				#else
				_behaviorHost = (MonoBehaviour)Configuration.Bootstrap().AddComponent(typeof(MonoBehaviour));
				#endif
			}
		
			CurrentWorkflowName = name;
			StateObject = (T)stateObject;
			StateObject.Workflow = this;
			
			_steps = new Queue<Action<T>>();
			_currentStepNumber = -1;
			_currentStep = null;
			
			foreach(var step in Workflows[name])
			{
				_steps.Enqueue(step);
			}
			
			Configuration.Log("[TinyWorkflow] Queued " + _steps.Count.ToString() + " steps.",LogSeverity.VERBOSE);
			
			StateObject.OnWorkflowStart();
			
			NextStep(false, false);
		}
		
		public void StepComplete(bool success = true)
		{
			Configuration.Log("[TinyWorkflow] Step Completed",LogSeverity.VERBOSE);
		
			try { StateObject.StepResultCallbacks[_currentStepNumber](success); }
			catch { }	
		
			if(_currentStep!=null)
				StateObject.OnWorkflowStepCompletion(_currentStepNumber,success,StateObject);
		}
		
		public void RepeatStep()
		{
			Configuration.Log("[TinyWorkflow] Repeating Step",LogSeverity.VERBOSE);
		
			if(_currentStep!=null)
				_currentStep(StateObject);	
		}
		
		public void NextStep(bool success = true, bool notifyCompletion = true)
		{
			if(notifyCompletion)
				StepComplete(success);
		
			Configuration.Log("[TinyWorkflow] Next Step",LogSeverity.VERBOSE);
			
			if(_steps.Count>0)
			{
				_currentStep = _steps.Dequeue();
				_currentStepNumber++;
				_currentStep(StateObject);
			}
		}
		
		public void Stop()
		{
			Configuration.Log("[TinyWorkflow] Stopping",LogSeverity.VERBOSE);
		
			if(!IsRunning)
			{
				Configuration.Log("[TinyWorkflow] Workflow is not running.",LogSeverity.WARNING);
				return;
			}
		
			if(StateObject!=null)
				StateObject.OnWorkflowStop();
				
			killBehaviorHost();
			
			IsRunning = false;
			
			Configuration.Log("[TinyWorkflow] Stopped",LogSeverity.VERBOSE);
		}
		
		public Coroutine StartCoroutine(IEnumerator routine)
		{
			Configuration.Log("[TinyWorkflow] Starting Coroutine",LogSeverity.VERBOSE);
		
			return _behaviorHost.StartCoroutine(routine);
		}
		
		void killBehaviorHost()
		{
			if(_behaviorHost!=null)
			{
				_behaviorHost.StopAllCoroutines();
				#if UNITY_EDITOR
				GameObject.DestroyImmediate(_behaviorHost);
				#else
				GameObject.Destroy(_behaviorHost);
				#endif
			}
		}
	}
}

