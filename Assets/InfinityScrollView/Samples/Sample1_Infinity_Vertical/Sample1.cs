﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OneP.InfinityScrollView;
using OneP.Samples;
using System.Globalization;

public class Sample1 :SingletonMono<Sample1> {

	public InfinityScrollView verticleScroll;
	public Text txtInfo;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void Reload(){

		string text = GetComponentInChildren<InputField>().text;
		int valueData = 0;
		if (!int.TryParse(text, out valueData))
		{
			valueData = 100;
		}
		if (valueData < 0)
			valueData = 100;
		
		verticleScroll.Setup (valueData);
		if (valueData > 0) {
			verticleScroll.InternalReload ();
		}
	}
	public void OnClickItem(int index){

		txtInfo.text="Click On Item:"+index;
	}
	public void NextSample(){
		SampleGlobalValue.GoToNextSample ();
	}
}
