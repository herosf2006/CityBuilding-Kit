using UnityEngine;
using System.Collections;

public class Relay : MonoBehaviour {//system-wide coordination for input and elements that are alternately activated 
	
	public GameObject MenuUnit, UnitProc;//alternates between the unit menu and the unit proc
	
	//Relayed variables
	public int 
	curSelection = 0, //refers to tabs
	prevSelection = 0;

	public bool 
		pauseInput = false,	
		delay = false,
		pauseMovement = false,			//pause building movement while the destroy confirmation is on
		deploying = false;

	public AlphaTween currentAlphaTween;

	public void CheckForUnitMenu()
	{
		if (prevSelection == 5)
			ActivateUnitMenu ();
	}

	public void UpdateCurrentTab(int selection)
	{
		curSelection = selection;

		if (curSelection == 5) ActivateUnitMenu ();		
		if (prevSelection == 5) ActivateUnitProc ();
		
		prevSelection = curSelection;		
	}

	public void ActivateUnitMenu()
	{			
		((MenuUnit)MenuUnit.GetComponent("MenuUnit")).LoadValuesfromProc();	
		UnitProc.SetActive(false);
	}
	
	public void ActivateUnitProc()
	{
		UnitProc.SetActive(true);	
		((MenuUnit)MenuUnit.GetComponent("MenuUnit")).PassValuestoProc();	
	}

	public void DelayInput()
	{
		delay = true;
		StopCoroutine("ResetDelayInput");
		StartCoroutine("ResetDelayInput");
	}

	IEnumerator ResetDelayInput()
	{
		yield return new WaitForSeconds(0.2f);
		delay = false;
	}
}
