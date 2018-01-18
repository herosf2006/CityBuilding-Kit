using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ShopTabs : MonoBehaviour {//controls the building/units tabs
	
	private const int noTabs = 4;

	public int curSelection = 0, prevSelection = 0;

	public GameObject[] 
		LabelsSelected = new GameObject[noTabs],
		LabelsIdle = new GameObject[noTabs],
		Tabs = new GameObject[noTabs];

	private Component relay;
				
	public void OnBuildingsTab(){
		curSelection=0;
		SelectTab();
	}
	
	public void OnCreaturesTab(){
		curSelection=1;	
		SelectTab();
	}
	public void OnWallsTab(){
		curSelection=2;	
		SelectTab();
	}
	public void OnDefenseTab(){
		curSelection=3;
		SelectTab();
	}

	void Start () {
		relay = GameObject.Find("Relay").GetComponent<Relay> ();	
	}
			
	private void SelectTab()
	{
		if(curSelection != prevSelection)
		{	
			//((Relay)relay).currentTab = curSelection;
			
			LabelsIdle[prevSelection].SetActive(true);
			LabelsSelected[prevSelection].SetActive(false);
			
			LabelsSelected[curSelection].SetActive(true);
			LabelsIdle[curSelection].SetActive(false);
						
			SwitchTab();
			
			prevSelection = curSelection;			
		}
	}

	private void SwitchTab()
	{
		if(prevSelection == 1)
		{
			((Relay)relay).ActivateUnitProc();
		}
		else if(curSelection==1)
		{
			((Relay)relay).ActivateUnitMenu();
		}
		
		Tabs[curSelection].SetActive(true);
		Tabs[prevSelection].SetActive(false);
	}
}
