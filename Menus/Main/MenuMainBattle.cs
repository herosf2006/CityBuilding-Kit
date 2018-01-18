using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuMainBattle : MonoBehaviour {	//manages major interface elements and panels on the battle map
	
	private const int 
		noIFElements = 6,
		noPanels = 2;

	public GameObject[] 
		InterfaceElements = new GameObject[noIFElements],
		Panels = new GameObject[noPanels];//unitsbattle options

	private Component relay;

	void Start () {

		relay = GameObject.Find ("Relay").GetComponent<Relay> ();
	}
	private void Delay()//to prevent selecting the buildint underneath various buttons
	{
		((Relay)relay).DelayInput();
	}

	public void OnUnits(){OnActivateButton (0); MoveHeliosUI (true);}

	private void MoveHeliosUI(bool on)
	{
		if (on) 
		{
			InterfaceElements [3].SetActive (true);//reactivate this, they are all deactivated below in ActivateButton
			InterfaceElements [3].transform.localPosition = new Vector3 (100, 230, 0);
		} 
		else 
		{
			InterfaceElements [3].transform.localPosition = new Vector3 (100, 60, 0);
		}
	}
	public void OnCloseUnits(){OnDeactivateButton (0); MoveHeliosUI (false); Delay ();}
	
	public void OnOptions(){OnActivateButton (1);InterfaceElements [0].SetActive (false);}
	public void OnCloseOptions(){OnDeactivateButton (1);InterfaceElements [0].SetActive (true);Delay ();}

	void OnActivateButton(int panelno)
	{
		bool pauseInput = false;
		
		pauseInput = ((Relay)relay).pauseInput;
		
		if (!pauseInput) 
		{
			Panels [panelno].SetActive (true);
			((Relay)relay).pauseInput = true;
			for (int i = 1; i < InterfaceElements.Length; i++) //InterfaceElements 0 - leaves the navigation buttons active
			{
				InterfaceElements [i].SetActive (false);
			}
		} 
	}

	void OnDeactivateButton(int panelno)
	{	
		((Relay)relay).pauseInput = false;		
		Panels[panelno].SetActive(false);
		ActivateInterface();
	}

	private void ActivateInterface()
	{
		for (int i = 1; i < InterfaceElements.Length; i++) //skip InterfaceElements 0 - the navigation buttons are already active
		{
			//if(i!= 3) //the hard resource button is not wired
			//if(i!=9)//to disable navigation buttons
			InterfaceElements[i].SetActive(true);
		}
	}

	public void ExitGame()
	{
		Application.Quit();
	}	
}
