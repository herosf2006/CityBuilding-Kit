using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuMain : MonoBehaviour {											//manages major interface elements and panels

	private bool showHide;
	
	private const int 
		noAnchors = 4,//includes 3 fading "Plus buttons" + the day/night "clock"
		noPanels = 4,
		noResPB = 6; //all resources + dobbit/cloak

	public GameObject[] 
		Anchors = new GameObject[noAnchors],						//small interface elements to be deactivated when a panel is opened
		Panels = new GameObject[noPanels],							//shop options competition invite purchase upgrade units damage
		ResourcePB = new GameObject[noResPB];										

	//GameObject.Find only retrieves active objects; it will not find a disabled panel
	public GameObject 
		AnchorCenter,
		MenuArmyObj, 
		DeleteBuildingYN, 		//building
		DeleteDefenseYN, 		//defense
		DeleteWeaponYN;			//weapon	

	public bool constructionGreenlit = true, waitForPlacement = false;
	private bool waitForTween = false;

	private Component relay, menuArmy, buildingCreator, wallCreator, weaponCreator;
	public ShopPanelManager shopPanelManager;

	void Start () {

		relay = GameObject.Find ("Relay").GetComponent<Relay> ();
		buildingCreator = GameObject.Find ("BuildingCreator").GetComponent<StructureCreator>();
		wallCreator = GameObject.Find ("WallCreator").GetComponent<StructureCreator>();
		weaponCreator = GameObject.Find ("WeaponCreator").GetComponent<StructureCreator>();
		menuArmy = MenuArmyObj.GetComponent<MenuArmy> ();

	}

	private void Delay()//brief delay to prevent selecting the buildint underneath menus if button positions overlap - the "double command" 
	{
		((Relay)relay).DelayInput();
	}
	private void PauseMovement(bool b)
	{
		((Relay)relay).pauseMovement = b;
	}

	public void OnShop(){OnActivatePanel (0);}// ((Relay)relay).UpdateCurrentTab (((Relay)relay).curSelection);  
	public void OnCloseShop(){shopPanelManager.StopLateCoroutines(); OnDeactivatePanel (0);}
	public void OnCloseShopToBuild()
	{
		if (constructionGreenlit) 
		{
			OnDeactivatePanel (0);
			waitForPlacement = true;
			//Delay();
			//Panels [0].SetActive (false);
			//ActivateInterface ();
		}
	}

	public void OnSettings(){OnActivatePanel (1);}
	public void OnCloseSettings(){OnDeactivatePanel (1);}

	public void OnCompetition(){OnActivatePanel (2);}
	public void OnCloseCompetition(){OnDeactivatePanel (2);}

	public void OnUnits()
	{
		if(!((Relay)relay).pauseInput) 
		{
			OnActivatePanel (2);
			((MenuArmy)menuArmy).UpdateStats();
		}
		
	}
	public void OnCloseUnits(){OnDeactivatePanel (2);}

	public void OnDamage(){OnActivatePanel (3);}
	public void OnCloseDamage(){OnDeactivatePanel (3);}


	//Building destroy
	public void OnConfirmationBuilding()								//the destroy building confirmation
	{ 
		Delay ();
		PauseMovement (true);
		DeleteBuildingYN.SetActive (true); 
		//StatsPadObj.SetActive (false);
	}
	public void OnCloseConfirmationBuilding() 
	{ 
		PauseMovement (false);
		DeleteBuildingYN.SetActive (false); 
		Delay();
	}

	public void OnDestroyBuilding()
	{
		((StructureCreator)buildingCreator).Cancel();
		DeleteBuildingYN.SetActive (false);//Delay();		//delay deferred to buildingCreator
	}
	public void OnCancelDestroyBuilding()
	{
		((StructureCreator)buildingCreator).OK();
		DeleteBuildingYN.SetActive (false);//Delay(); 
	}

	//Wall destroy

	public void OnConfirmationWall()								//the destroy fortification confirmation
	{ 
		Delay ();
		PauseMovement (true);
		DeleteDefenseYN.SetActive (true); 
		//StatsPadObj.SetActive (false);
	}
	public void OnCloseConfirmationWall() 
	{ 
		PauseMovement (false);
		DeleteDefenseYN.SetActive (false); 
		Delay();
	}
	
	public void OnDestroyWall()
	{
		((StructureCreator)wallCreator).Cancel();
		DeleteDefenseYN.SetActive (false);//Delay();		//delay deferred to buildingCreator
	}
	public void OnCancelDestroyWall()
	{
		((StructureCreator)wallCreator).OK();
		DeleteDefenseYN.SetActive (false);//Delay(); 
	}

	public void OnConfirmationWeapon()								//the destroy fortification confirmation
	{ 
		Delay ();
		PauseMovement (true);
		DeleteWeaponYN.SetActive (true); 
	}
	public void OnCloseConfirmationWeapon() 
	{ 
		PauseMovement (false);
		DeleteWeaponYN.SetActive (false); 
		Delay();
	}

	public void OnDestroyWeapon()
	{
		((StructureCreator)weaponCreator).Cancel();
		DeleteWeaponYN.SetActive (false);//Delay();		//delay deferred to buildingCreator
	}

	public void OnCancelDestroyWeapon()
	{
		((StructureCreator)weaponCreator).OK();
		DeleteWeaponYN.SetActive (false);//Delay(); 
	}



	void OnActivatePanel(int panelno)
	{		
		if (waitForTween) 
		{				
			return;
		} 

		bool pauseInput = ((Relay)relay).pauseInput;
		
		if (!pauseInput) 
		{		
			waitForTween = true;	
			Panels [panelno].SetActive (true);

			TweenAnchorCenter ();
			((Relay)relay).pauseInput = true;
			TweenAnchors ();
			StartCoroutine (ReadyTween ());
		} 
	}

	void OnDeactivatePanel(int panelno)
	{	
		if (waitForTween) 
		{				
			return;
		} 

		waitForTween = true;	

		//((Relay)relay).pauseInput = false;	//move this to building placed
		Delay ();

		TweenAnchorCenter ();
		StartCoroutine (LateDeactivatePanel (panelno));
		ActivateInterface ();
		StartCoroutine (ReadyTween ());

	}
	private void TweenAnchorCenter()
	{		
		AnchorCenter.GetComponent<TweenAlpha> ().Toggle ();
		AnchorCenter.GetComponent<TweenScale> ().Toggle ();
	}
	private IEnumerator LateDeactivatePanel(int panelno)
	{
		yield return new WaitForSeconds (0.6f);
		Panels[panelno].SetActive(false);
		if(!waitForPlacement)
			((Relay)relay).pauseInput = false;
		//AnchorCenter.SetActive(false);
	}
	private void ActivateInterface()
	{
		TweenAnchors ();
	}

	private void TweenAnchors()
	{
		for (int i = 0; i < Anchors.Length; i++) //skip the upgrade button
		{
			if (i != 1) //skip the top right anchor for children tween position
			{
				Anchors [i].GetComponent<TweenAlpha> ().Toggle ();

				showHide = !showHide;
				if (showHide)
				{
					StartCoroutine(Fade.Instance.FadeTo(0.0f, 0.5f));
				}
				else
				{
					StartCoroutine(Fade.Instance.FadeTo(1.0f, 0.5f));
				}
 				

			}
		}
		TweenResourcePB ();
	}
	private IEnumerator ReadyTween()
	{
		yield return new WaitForSeconds(0.6f);//make sure tweens are no longer than 0.5f
		waitForTween = false;

	}
	private void TweenResourcePB()
	{
		for (int i = 0; i < ResourcePB.Length; i++) 
		{			
			ResourcePB [i].GetComponent<TweenPosition> ().Toggle ();
		}
	}

	public void ExitGame()
	{
		Application.Quit();
	}	
}
