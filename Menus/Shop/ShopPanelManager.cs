using UnityEngine;
using System.Collections;

public class ShopPanelManager : MonoBehaviour {

	public GameObject HomePage, MenuUnit, MenuMain, BKCommon;

	public GameObject[] 
		sideTabs = new GameObject[2],				//SideTabsLeft, SideTabsRight,
		sideTabsUnit = new GameObject[2],
		sideButtons = new GameObject[8],
		panels = new GameObject[8];					//FirstPage,Store,Production,Military,Fort,Defense,Troops, Cloak,Ambient;
		
	private int 
		currentSelection = -1, 
		previousSelection = -1;

	public Relay relay;

	void Start () 
	{
		
	}

	public void ShowHomePage ()	{ SwitchPanel (-1);	}
	public void Show0(){ SwitchPanel (0);}
	public void Show1(){ SwitchPanel (1);}
	public void Show2(){ SwitchPanel (2);}
	public void Show3(){ SwitchPanel (3);}
	public void Show4(){ SwitchPanel (4);}
	public void Show5(){ SwitchPanel (5);}//units
	public void Show6(){ SwitchPanel (6);}
	public void Show7(){ SwitchPanel (7);}

	public void LateShow0(){StopLateCoroutines ();StartCoroutine ("LateShowStore");}
	public void LateShow1(){StopLateCoroutines ();StartCoroutine ("LateShowDobbit");}
	public void LateShow5(){StopLateCoroutines ();StartCoroutine ("LateShowUnits");}
	public void LateShow6(){StopLateCoroutines ();StartCoroutine ("LateShowCloak");}
	private IEnumerator LateShowStore()
	{
		yield return new WaitForSeconds (0.5f);
		Show0 ();
	}

	private IEnumerator LateShowDobbit()
	{
		yield return new WaitForSeconds (0.5f);
		Show1 ();
	}
	private IEnumerator LateShowCloak()
	{
		yield return new WaitForSeconds (0.5f);
		Show6 ();
	}
	private IEnumerator LateShowUnits()
	{
		yield return new WaitForSeconds (0.5f);
		Show5 ();
	}
	public void StopLateCoroutines()//I don't want to disable all coroutines
	{			
		StopCoroutine ("LateShowStore");
		StopCoroutine ("LateShowDobbit");
		StopCoroutine ("LateShowCloak");
	}


	//tween position toggle tween color toggle

	private void SwitchPanel(int newPage)		//the commented sections are for the units side tabs extension for multiple barracks - in a future update
	{
		if (newPage == currentSelection)	return;

		if (currentSelection == -1) //home page switching off
		{
			HomePage.GetComponent<TweenAlpha> ().Toggle ();
			StartCoroutine ("DisableHomePage");
		}

		if (newPage == -1) //returning to home page
		{
			HomePage.SetActive (true);
			HomePage.GetComponent<TweenAlpha> ().Toggle ();

			//if (currentSelection != 5 || newPage == 5)//don't activate if last menu was units
			SideTabsTween ();			
		} 	

		else if (newPage == 5 && previousSelection!=-1) //launching the unit menu
		{
			//SideTabsTween ();
			StartCoroutine (ToggleBKCommon(false,0.3f));
		}
		else if (previousSelection == 5 ) //&& newPage!=-1  //previous was unit menu, new page is not home page
		{
			//SideTabsTween ();	
			StartCoroutine (ToggleBKCommon(true,0.01f));
		}

		/*back to HomePage page*/ /*exit HomePage page*/ /*launch unit menu*/
		else if(newPage!=-1 && previousSelection==-1 )//&& newPage!=5  from home page to another
		{			
			SideTabsTween ();
			if(newPage==5)
				StartCoroutine (ToggleBKCommon(false, 0.3f));
		}

		if (newPage == 5 || previousSelection == 5) 
		{
			//SideTabsUnitTween ();
			if (newPage == 5) 
			{
				StartCoroutine (ToggleBKCommon(false, 0.3f));
			}
			if(previousSelection==5)
				StartCoroutine (ToggleBKCommon(true, 0.3f));
		}

		currentSelection = newPage;
		
		if (previousSelection != -1)
		{			
			panels [previousSelection].GetComponent<TweenAlpha> ().Toggle ();
			StartCoroutine (DisablePanel (previousSelection));
			SideButtonsTween (previousSelection);
		} 

		if (currentSelection != -1) 
		{
			panels [currentSelection].SetActive (true);
			panels [currentSelection].GetComponent<TweenAlpha> ().Toggle ();
			SideButtonsTween (currentSelection);
		} 

		relay.UpdateCurrentTab(newPage);

		previousSelection = currentSelection; 
	}

	private IEnumerator DisableHomePage()
	{
		yield return new WaitForSeconds (0.6f);
		HomePage.SetActive (false);
	}
	private IEnumerator ToggleBKCommon(bool b, float delay)
	{
		yield return new WaitForSeconds (delay);//0.3f
		BKCommon.SetActive(b);
	}
	private IEnumerator DisablePanel(int index)
	{
		yield return new WaitForSeconds (0.6f);
		panels [index].SetActive (false);
	}

	private void SideButtonsTween(int i)
	{
		sideButtons [i].GetComponent<TweenPosition> ().Toggle ();
		sideButtons [i].GetComponent<TweenColor> ().Toggle ();
	}

	private void SideTabsTween()
	{
		for (int i = 0; i < sideTabs.Length; i++) 
		{
			sideTabs[i].GetComponent<TweenAlpha>().Toggle();
			sideTabs[i].GetComponent<TweenPosition>().Toggle();
		}
	}
	private void SideTabsUnitTween()
	{
		for (int i = 0; i < sideTabsUnit.Length; i++) 
		{
			sideTabsUnit[i].GetComponent<TweenAlpha>().Toggle();
			sideTabsUnit[i].GetComponent<TweenPosition>().Toggle();
		}
	}
}
