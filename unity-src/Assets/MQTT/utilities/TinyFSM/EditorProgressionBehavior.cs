using UnityEngine;
using System.Collections;

namespace HG.TinyFSM
{
	/// <summary>
	/// Allows some aspects of coroutines to run to completion in editor mode.
	/// Use this class as a host for coroutines inside the editor.
	/// </summary>
	[ExecuteInEditMode]
	public class EditorProgressionBehavior : MonoBehaviour 
	{
		#if UNITY_EDITOR	
		bool _ready = false;

		void Start () 
		{
			
		}
		
		void Update () 
		{
			if(!_ready)
				return;
		
			try
			{
				UnityEditor.EditorUtility.SetDirty(_dirtyUp);
			}
			catch(System.Exception ex) 
			{
				Configuration.Log("EditorProgressionBehavior encountered an error: " + ex, LogSeverity.WARNING);
				findWakPersistent();
			}
		}
		
		GameObject _dirtyUp;
		
		void findWakPersistent()
		{
			_dirtyUp = Configuration.Bootstrap();
		}

		void OnDisable()
		{

		}

		public void Subscribe()
		{
			findWakPersistent();
			_ready = true;
			UnityEditor.EditorApplication.update += this.Update;
			Configuration.Log("EditorProgressionBehavior Editor Update Connected",LogSeverity.VERBOSE);
		}
		
		public void Unsubscribe()
		{
			UnityEditor.EditorApplication.update -= this.Update;
			_ready = false;
			Configuration.Log("EditorProgressionBehavior Editor Update Disconnected",LogSeverity.VERBOSE);
		}	
		#endif
	}
}
