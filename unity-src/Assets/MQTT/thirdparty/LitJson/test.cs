using System;
using UnityEngine;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;


namespace hg.LitJson
{
	
	public class test : MonoBehaviour
	{
		public void Start()
		{
			/*hg.LitJson.JsonMapper.RegisterImporter<Int32,String>(
				new hg.LitJson.ImporterFunc<Int32,String> ((intValue) => { return intValue.ToString(); } )
			);*/
		
			hg.LitJson.JsonMapper.UnregisterImporters();
		
			TextAsset ta = Resources.Load("sampleJson") as TextAsset;
			string json = ta.text;
			
			//JsonData jd = JsonMapper.ToObject(json);
			testModel m = JsonMapper.ToObject<testModel>(json);
			
		}
	}
	
	public class testModel
	{
		public string[][] aaData;
		//public List<string[]> aaData;
		public testModelResult[] result;
	}
	
	public class testModelResult
	{
		public string short_url;
	}
}
