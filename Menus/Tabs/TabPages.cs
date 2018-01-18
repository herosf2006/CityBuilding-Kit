using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TabPages : MonoBehaviour {//controls the 2 pages and arrows for units, buildings, etc

	public int currentPanel = 0;	
	public GameObject[] Pages = new GameObject[2];	//since it's a "tab pages", presumably you have at least 2 pages
	public GameObject ArrowLeft, ArrowRight;
	private Component relay;

	void Start () {
		
		relay = GameObject.Find ("Relay").GetComponent<Relay> ();
	}

	private void Delay()//to prevent menu button commands from interfering with sensitive areas/buttons underneath
	{
		((Relay)relay).DelayInput();
	}

	public void OnArrowLeft()
	{
		Delay ();

		currentPanel--;

		if(currentPanel<Pages.Length)
		{
			Pages[currentPanel+1].SetActive(false);
		}

		Pages[currentPanel].SetActive(true);

		if(currentPanel==0)
		{
			ArrowLeft.SetActive(false);
		}
		if(!ArrowRight.activeSelf)
		{
			ArrowRight.SetActive(true);
		}

	}
	
	public void OnArrowRight()
	{	
		Delay ();

		currentPanel++;

		if(currentPanel>0)
		{
			Pages[currentPanel-1].SetActive(false);
		}

		Pages[currentPanel].SetActive(true);

		if(currentPanel==Pages.Length-1)
		{
			ArrowRight.SetActive(false);
		}
		if(!ArrowLeft.activeSelf)
		{		
			ArrowLeft.SetActive(true);
		}


	}	
	
}

