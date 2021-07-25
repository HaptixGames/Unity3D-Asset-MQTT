using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HG.TinyFSM
{
	[Serializable]
	public class TinyStateMachine
	{
		// behavior responsible for hosting coros
		private MonoBehaviour _behaviorHost = null;
		
		private IEnumerator _previousState = null;
		private IEnumerator _currentState = null;
		
		// queue of states
		private Queue<IEnumerator> _nextStatesQueue = null;
		
		private bool _nextStatesQueueBlocked = false;
	
		public bool NextStateRequested { get; private set; }
		private bool _nextStateAllowed = false;
		
		private System.Action<TinyStateMachine,string,string> _onStateChangeNotification;
		
		private System.Action _currentGuiRoutine = () => {};
		//private List<Tuple<int,string,System.Action>> _guiRoutines;
		
		private bool _canUpdate = false;
		
		#if UNITY_EDITOR	
		private HG.TinyFSM.EditorProgressionBehavior _behaviorEditorProgress = null;	
		private UnityEditor.EditorWindow _window = null;
		public TinyStateMachine(UnityEditor.EditorWindow window, System.Action<TinyStateMachine,string,string> onStateChange): this(onStateChange)
		{
			_window = window;
			_behaviorEditorProgress = (HG.TinyFSM.EditorProgressionBehavior)Configuration.Bootstrap().AddComponent(typeof(HG.TinyFSM.EditorProgressionBehavior));
			_behaviorEditorProgress.Subscribe();
		}
		#endif	
		
		public TinyStateMachine(System.Action<TinyStateMachine,string,string> onStateChange)
		{
			Configuration.Log("[TinyFsm] Initializing", LogSeverity.VERBOSE);
		
			//_guiRoutines = new List<Tuple<int, string, Action>>();
		
			_nextStatesQueue = new Queue<IEnumerator>();
			_onStateChangeNotification = onStateChange;
			generateBehaviourHost();
		}

		private void generateBehaviourHost()
		{
			if(_behaviorHost!=null)
				return;

			GameObject go = new GameObject();
			go.name = Configuration.GetSetting<string>("persistent-game-object-name") + ":tiny-fsm:" + go.GetInstanceID().ToString();
			//TODO: hide fsm object
			go.hideFlags = Configuration.GetSetting<HideFlags>("tiny-fsm-game-object-flags");
			_behaviorHost = go.AddComponent<TinyFsmBehaviorHost>();
		}


		public void Update()
		{
			generateBehaviourHost();

			if(!_canUpdate)
			{
				Configuration.Log("[TinyFsm] Not yet updateable.",LogSeverity.VERBOSE);
				return;
			}
			
			// if there are no states to dequeue or the queue is blocked then don't do anyting
			if(_nextStatesQueue.Count==0 || _nextStatesQueueBlocked)
				return;
			
			// block the state queue before we transition until next state becomes current
			_nextStatesQueueBlocked = true;	
			IEnumerator nextState = _nextStatesQueue.Dequeue();
			_behaviorHost.StartCoroutine(spinUpNextState(nextState));
		}


		#region Transition
		public void Stop(bool destroyFsm = false)
		{
			//TODO: what happens to below coro when a recompile is performed?
			//TODO: need to destroy FSM GOs without coro
			
			#if UNITY_EDITOR
			//HACK: this destroys fsm and related objects when DLL is rebuilt while editor window with an FSM is visible in editor
			if(destroyFsm)
			{
				NextStateRequested = true;
				_behaviorEditorProgress.Unsubscribe();
				GameObject.DestroyImmediate(_behaviorHost.gameObject);
				GameObject.DestroyImmediate(_behaviorEditorProgress);
			}
			#else
			//TODO : TEST
			_behaviorHost.StartCoroutine(spinUpNextState(null));
			#endif
		}
		
		public bool CanTransition
		{
			get
			{
				return !_nextStatesQueueBlocked;
			}
		}
		
		public bool TryGoto(IEnumerator nextState)
		{
			_canUpdate = true;
		
			if(_nextStatesQueueBlocked)
			{
				Configuration.Log("[TinyFsm] Denied Requested State Change  => " + stateName(nextState) ,LogSeverity.VERBOSE);
				return false;
			}
			else
			{
				Configuration.Log("[TinyFsm] Accepted Requested State Change  => " + stateName(nextState) ,LogSeverity.VERBOSE);
				_nextStatesQueue.Enqueue(nextState);
				return true;
			}
		}
		
		public bool Goto(IEnumerator nextState)
		{
			_canUpdate = true;
			
			Configuration.Log("[TinyFsm] Accepted Requested State Change  => " + stateName(nextState) ,LogSeverity.VERBOSE);
			_nextStatesQueue.Enqueue(nextState);
			return true;
		}
		
		// called from current state before breaking out of its enumerator
		public void CurrentStateCompleted()
		{
			_nextStateAllowed = true;
		}
		
		private IEnumerator spinUpNextState(IEnumerator state)
		{
			Configuration.Log("[TinyFsm] Spinning Up State : " + stateName(state),LogSeverity.VERBOSE);
			
			//allows current state to break out of its update loop and exit
			NextStateRequested = true;
			
			if(_currentState!=null)
			{
				Configuration.Log("[TinyFsm] " + stateName(state) + " is waiting for " + stateName(_currentState) + " to finish.",LogSeverity.VERBOSE);
				
				//we wait until the current state calls fsm.CurrentStateCompleted() and sets _nextStateAllowed=true
				while(!_nextStateAllowed)
				{
					yield return null;
				}
				
				Configuration.Log("[TinyFsm] " + stateName(_currentState) + " has finished.",LogSeverity.VERBOSE);
				
				//couple more frames
				//yield return null;
				//yield return null;
			}
			
			//reset flags
			_nextStateAllowed = false;
			NextStateRequested = false;

			//TODO: when stopping fsm, this coroutine needs to continue to execute
			// requested state is null so we stop the fsm
			if(state==null)
			{
				Configuration.Log("[TinyFsm] Stopping...",LogSeverity.VERBOSE);
				#if UNITY_EDITOR
				_behaviorEditorProgress.Unsubscribe();
				//TODO: should stop coros before destroying?
				GameObject.DestroyImmediate(_behaviorHost.gameObject);
				GameObject.DestroyImmediate(_behaviorEditorProgress);
				#else
				//TODO: this coro will not run until the end if we destroy the monobehavior running it?
				GameObject.Destroy(_behaviorHost.gameObject);
				#endif
				Configuration.Log("[TinyFsm] Stopped!",LogSeverity.VERBOSE);
			}
			else
			{
				_behaviorHost.StartCoroutine(state);
				
				//couple more frames
				//yield return null;
				//yield return null;
				
				_previousState = _currentState;
				_currentState = state;
				
				if(_onStateChangeNotification!=null)
					_onStateChangeNotification(this,stateName(_previousState),stateName(_currentState));
				
				#if UNITY_EDITOR
				if(_window!=null)
					_window.Repaint();
				#endif
				
				Configuration.Log("[TinyFsm] " + stateName(_currentState) + " is now the current state.",LogSeverity.VERBOSE);
			}
			
			_nextStatesQueueBlocked = false;
			
			yield return null;
		}
		#endregion
		
		#region GUI Hooks
		public void OnGUI()
		{
			_currentGuiRoutine();
		}
		
		public void SetGui(System.Action guiRoutine)
		{
			_currentGuiRoutine = guiRoutine;
		}

		/* TODO : implement multiple gui routine support
		public void AddGui(string name, System.Action guiRoutine, int drawOrder = 0)
		{

		}
		
		public void RemoveGui(string name)
		{
			var routine = _guiRoutines.Find(t => t.Item2 == name);
			//TODO don't remove here, remove prior to gui iterations	
			if(routine!=null)
				_guiRoutines.Remove(routine);
		}
		*/
		
		#endregion
		
		#region State Infos
		public bool IsNextState(IEnumerator state)
		{
			//Configuration.Log("[TinyFsm] Next State Check",LogSeverity.VERBOSE);
			
			try
			{
				return stateName(_nextStatesQueue.Peek())==stateName(state);
			}
			catch
			{
				return false;
			}
		}

		public bool WasPreviousState(IEnumerator state)
		{
			Configuration.Log("[TinyFsm] Previous State Check",LogSeverity.VERBOSE);
			return stateName(_previousState)==stateName(state);
		}
		
		public bool IsInState(IEnumerator state)
		{
			Configuration.Log("[TinyFsm] Current State Check",LogSeverity.VERBOSE);
			return CurrentStateName==stateName(state);
		}
		
		public string CurrentStateName
		{
			get
			{
				//TODO: cache this
				return stateName(_currentState);
			}
		}
		
		private string stateName(IEnumerator state)
		{
			if(state==null)
				return "(null-state)";
			else
			{
				string mangled = state.GetType().Name;
				return mangled.Remove(mangled.IndexOf(">")+1,mangled.Length-mangled.IndexOf(">")-1);
			}
		}
		#endregion
	}
}