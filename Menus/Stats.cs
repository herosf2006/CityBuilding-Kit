using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Stats : MonoBehaviour { //the resource bars on top of the screen

	private int hours, minutes, seconds;				//for cloak remaining label

	private const int 
		noOfCreators = 4,
		
		noOfBuildings = 12,				//correlate with BuildingCreator.cs; verify that the Inspector has 11 elements
		noOfWeapons = 6,
		noOfAmbients = 16,
		noOfUnits = 12,					//correlate with MenuUnitBase.cs/ verify that the Inspector has 12 elements
		noOfWalls = 13;

	public BaseCreator[] creators = new BaseCreator[noOfCreators]; //the store must update other creators/interfaces after purchases

	public GameObject GhostHelper;

	public bool 
		gameWasLoaded = false,
		tutorialCitySeen = false, 
		tutorialBattleSeen = false,
		removablesCreated = false,
		resourcesAdded = false,
		resourcesSubstracted = false;

	public int 	//when user hard buys resources, storage capacity permanently increases 
		
		structureIndex = -1,			//unique index that all structures have - buildings, weapons, walls, etc
		townHallLevel = 0,				
		level = 1,						//player level
		experience = 1,
		maxExperience = 100, 			//save this too, used for progress bar to next level
		
		dobbits = 1, 
		occupiedDobbits = 0,

		remainingCloakTime,
		purchasedCloakTime,

		occupiedHousing = 0, 		//based on size, not number of units
		maxHousing = 0,				//housing refers ONLY to soldiers, not npc/dobbits
		
		gold = 5000, 
		maxGold = 10000, 

		mana = 500,		
		maxMana = 1000, 

		crystals = 5,
		maxCrystals = 5, 

		
		deltaGoldPlus, deltaGoldMinus,					//when a resource is added/spent, it starts a counter; must be separate because the operations might be simultaneous  
		deltaManaPlus, deltaManaMinus,
		deltaCrystalsPlus, deltaCrystalsMinus;

	//public float[] productionRates;

	public int[] 						
		//productionBuildings,//0 gold 1 mana	delete this
		maxBuildingsAllowed = new int[noOfBuildings],//existingBuildings,
		maxWallsAllowed = new int[noOfWalls],	
		maxWeaponsAllowed = new int[noOfWeapons],
		maxAmbientsAllowed = new int[noOfWeapons],
		sizePerUnit = new int[noOfUnits],			//based on size, a soldier can occupy more than 1 space
		existingUnits = new int[noOfUnits];			//existing units
		

	public UIProgressBar xpBar, dobbitsBar, cloakBar, soldiersBar, goldBar,  manaBar, crystalsBar;

	public UILabel 
		nameLb, levelLb, xpLb, 
		dobbitLb,  
		cloakLb, 
		housingLb, unitsNoLb, goldLb, manaLb, crystalsLb,
		maxHousingLb, maxGoldLb, maxManaLb;

	public TextAsset EvoStructuresXML;	//variables for loading building characteristics from XML
	protected List<Dictionary<string,string>> levels = new List<Dictionary<string,string>>();
	protected Dictionary<string,string> dictionary;

	private Component transData, statusMsg;

	//Interface connections

	public Store store;//this component is disabled, GameObject.Find doesn't work

	void Start () {
		
		transData = GameObject.Find ("TransData").GetComponent<TransData> ();
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();
		StartCoroutine ("ReturnFromBattle");	
		StartCoroutine ("LaunchTutorial");
		//productionBuildings = new int[2];
		//productionRates = new float[2];


		GetEvoStructuresXML ();

		UpdateBuildingXmlData ();
		UpdateWallXmlData ();
		UpdateWeaponXmlData ();
		UpdateAmbientXmlData ();

		UpdateUnitsNo ();
		StartCoroutine ("LateUpdateUI");
		//UpdateBuildingXmlData ();
	}

	private IEnumerator LateUpdateUI()
	{
		yield return new WaitForSeconds (3);
		UpdateUI ();
	}

	private void GetEvoStructuresXML()//reads structures XML
	{
		XmlDocument xmlDoc = new XmlDocument(); 
		xmlDoc.LoadXml(EvoStructuresXML.text); 
		XmlNodeList levelList = xmlDoc.GetElementsByTagName("Level");
		
		foreach (XmlNode levelInfo in levelList)
		{
			XmlNodeList levelContent = levelInfo.ChildNodes;	
			dictionary = new Dictionary<string, string>();
			
			foreach (XmlNode levelItems in levelContent) // levels itens nodes.
			{
				/*
				<Level>	<!-- 01 -->
				<MaxBuildings>2,2,2,1,1,1,0,0,0,0,0,0</MaxBuildings>	
				<MaxWeapons>2,2,2,1,1,1</MaxWeapons>							
				<MaxWalls>50,50</MaxWalls>	
				<MaxAmbients>2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2</MaxAmbients>				
				</Level>	
				*/
				
				if(levelItems.Name == "MaxBuildings")
				{
					dictionary.Add("MaxBuildings",levelItems.InnerText); // put this in the dictionary.
				}				
				if(levelItems.Name == "MaxWeapons")
				{
					dictionary.Add("MaxWeapons",levelItems.InnerText); // put this in the dictionary.
				}
				if(levelItems.Name == "MaxWalls")
				{
					dictionary.Add("MaxWalls",levelItems.InnerText); // put this in the dictionary.
				}
				if(levelItems.Name == "MaxAmbients")
				{
					dictionary.Add("MaxAmbients",levelItems.InnerText); // put this in the dictionary.
				}
			}
			
			levels.Add(dictionary);
		}
	}

	private void UpdateBuildingXmlData()
	{
		string[] maxBuildingsString = (levels [townHallLevel] ["MaxBuildings"]).Split("," [0]);
		for (int i = 0; i < noOfBuildings; i++) 
		{
			maxBuildingsAllowed[i] = int.Parse(maxBuildingsString[i]);		
		}

	}
	private void UpdateWallXmlData()
	{		
		//EvolutionBuildings.xml:
		//<MaxWalls>25,25</MaxWalls>	25 stone walls, 25 wooden fences of any kind

		//{"StoneTower","StoneWallNE","StoneWallNW",
		//"WoodCornerN","WoodCornerS","WoodCornerE","WoodCornerW", "WoodFenceNE",
		//"WoodFenceNW","WoodEndNE","WoodEndNW","WoodEndSE","WoodEndSW"};

		string[] maxWallsString = (levels [townHallLevel] ["MaxWalls"]).Split("," [0]);

		for (int i = 0; i < noOfWalls; i++) //13 types 
		{
			if (i<3)
			maxWallsAllowed[i]=int.Parse(maxWallsString[0]);
			else
			maxWallsAllowed[i]=int.Parse(maxWallsString[1]);
		}
	}

	private void UpdateWeaponXmlData()
	{	
		string[] maxWeaponString = (levels [townHallLevel] ["MaxWeapons"]).Split("," [0]);

		for (int i = 0; i < noOfWeapons; i++) 
		{
			maxWeaponsAllowed[i] = int.Parse(maxWeaponString[i]);
		}
	}
	private void UpdateAmbientXmlData()
	{	
		string[] maxAmbientString = (levels [townHallLevel] ["MaxAmbients"]).Split("," [0]);

		for (int i = 0; i < noOfAmbients; i++) 
		{			
			maxAmbientsAllowed[i] = int.Parse(maxAmbientString[i]);
		}
	}

	/*public bool CheckResources(int dGold, int dMana, int dCrystals, int whichOne)//0,1,2 gold/mana/crystals
	{
		bool enoughResource;

		switch (whichOne) 
		{
		case 0:
			enoughResource = dGold > gold + deltaGoldPlus - deltaGoldMinus;
			break;
		case 1:
			enoughResource = dMana > mana + deltaManaPlus - deltaManaMinus;
			break;
		case 2:
			enoughResource = dCrystals > crystals + deltaCrystalsPlus - deltaCrystalsMinus;
			break;
		}
		return enoughResource;
	}*/

	public bool EnoughGold(int goldPrice)
	{
		return (goldPrice <= gold + deltaGoldPlus - deltaGoldMinus);
	}

	public bool EnoughMana(int manaPrice)
	{
		return (manaPrice <= mana + deltaManaPlus - deltaManaMinus);
	}

	public bool EnoughCrystals(int crystalPrice)
	{
		return ( crystalPrice <= crystals + deltaCrystalsPlus - deltaCrystalsMinus);
	}

	public void AddResources(int dGold, int dMana, int dCrystals)
	{
		deltaGoldPlus += dGold;
		deltaManaPlus += dMana;
		deltaCrystalsPlus += dCrystals;
			
		if (!resourcesAdded) {			
			resourcesAdded = true;
			InvokeRepeating ("GradualAddResources", 0.1f, 0.1f);
		}

		UpdateCreatorMenus ();
		//updates in sequence all buttons from all panels; 
	}

	private void GradualAddResources()
	{
		if (resourcesAdded) //verify if needed
		{			
			if(deltaGoldPlus>10)
			{
				int substract = deltaGoldPlus/10;
	

				deltaGoldPlus-= substract;
				gold += substract;
			}
			else if(deltaGoldPlus>0)
			{
				deltaGoldPlus--;
				gold ++;
			}

			if(deltaManaPlus>10)
			{
				int substract = deltaManaPlus/10;

				deltaManaPlus-= substract;
				mana += substract;
			}
			else if(deltaManaPlus>0)
			{
				deltaManaPlus--;
				mana ++;
			}

			if(deltaCrystalsPlus>10)
			{
				int substract = deltaCrystalsPlus/10;
				deltaCrystalsPlus -= substract;
				crystals += substract;
			}
			else if(deltaCrystalsPlus>0)
			{
				deltaCrystalsPlus--;
				crystals ++;
			}

			ApplyMaxCaps();
			UpdateUI();

			if (deltaGoldPlus == 0 && deltaManaPlus == 0 && deltaCrystalsPlus == 0) 
			{						
				CancelInvoke ("GradualAddResources");
				resourcesAdded = false;
			}
		}
	}

	public void SubstractResources(int dGold, int dMana, int dCrystals)
	{		
		deltaGoldMinus += dGold; deltaManaMinus += dMana; deltaCrystalsMinus += dCrystals;

		if (!resourcesSubstracted) 
		{			
			resourcesSubstracted = true;
			InvokeRepeating ("GradualSubstractResources", 0.1f, 0.1f);
		}

		UpdateCreatorMenus();
		//updates in sequence all buttons from all panels; 
	}


	private void GradualSubstractResources()
	{
		if (resourcesSubstracted) //verify if needed
		{			
			if(deltaGoldMinus>10)
			{
				int substract = deltaGoldMinus/10;

				deltaGoldMinus-= substract;//substract is a negative value here
				gold -= substract;
			}
			else if(deltaGoldMinus>0)
			{
				deltaGoldMinus--;
				gold --;
			}

			if(deltaManaMinus>10)
			{
				int substract = deltaManaMinus/10;

				deltaManaMinus-= substract;
				mana -= substract;
			}
			else if(deltaManaMinus>0)
			{
				deltaManaMinus--;
				mana --;
			}
		
			if(deltaCrystalsMinus>10)
			{
				int substract = deltaCrystalsMinus/10;
				deltaCrystalsMinus -= substract;
				crystals -= substract;
			}
			else if(deltaCrystalsMinus>0)
			{
				deltaCrystalsMinus--;
				crystals --;
			}

			ApplyMaxCaps();
			UpdateUI();

			if (deltaGoldMinus == 0 && deltaManaMinus == 0 && deltaCrystalsMinus==0) 
			{					
				CancelInvoke ("GradualSubstractResources");
				resourcesSubstracted = false;
			}
		}
	}

	private IEnumerator ReturnFromBattle()
	{
		yield return new WaitForSeconds (1.5f);

		if (((TransData)transData).battleOver) 
		{
			((TransData)transData).ReturnFromBattle();
			tutorialCitySeen = true;//since we have already been to battle, no tutorial 
		}	
	}

	private IEnumerator LaunchTutorial()
	{
		yield return new WaitForSeconds (10.0f);
		//if (!tutorialCitySeen)
			//GhostHelper.SetActive (true);//since this is a delayed function, we will activate the first time tutorial here 
	}

	public void ApplyMaxCaps()//cannot exceed storage+bought capacity
	{
		if (gold > maxGold) { gold = maxGold; }
		if (mana > maxMana) { mana = maxMana; }
		//if (experience > maxExperience) { experience = maxExperience; }
	}

	public void VerifyMaxReached()
	{
		if (gold == maxGold) 
		{ 
			((Messenger)statusMsg).DisplayMessage("Increase Gold storage capacity.");	
		}
		if (mana == maxMana) 
		{ 
			((Messenger)statusMsg).DisplayMessage("Increase Mana storage capacity.");
		}
	}
	public void UpdateCreatorMenus()
	{
		for (int i = 0; i < creators.Length ; i++) {
			creators [i].UpdateButtons ();
		}
	}

	public void UpdateUI()//updates numbers and progress bars
	{
		((UISlider)xpBar.GetComponent ("UISlider")).value = (float)experience/(float)maxExperience;
		((UISlider)dobbitsBar.GetComponent ("UISlider")).value = 1-((float)occupiedDobbits/(float)dobbits);
		((UISlider)goldBar.GetComponent ("UISlider")).value = (float)gold/(float)maxGold;
		((UISlider)manaBar.GetComponent ("UISlider")).value = (float)mana/(float)maxMana;
		((UISlider)crystalsBar.GetComponent ("UISlider")).value = (float)crystals/(float)maxCrystals;
		((UISlider)soldiersBar.GetComponent ("UISlider")).value = (float)occupiedHousing/(float)maxHousing;

		nameLb.text = "Player Name";
		levelLb.text = level.ToString ();

		xpLb.text = experience.ToString () + " / " + maxExperience.ToString();
		dobbitLb.text = (dobbits-occupiedDobbits).ToString () + " / " + dobbits.ToString ();
		cloakLb.text = remainingCloakTime.ToString () + " / " + purchasedCloakTime.ToString ();

		housingLb.text = occupiedHousing.ToString (); // + " / " + maxHousing.ToString ()
		maxHousingLb.text = maxHousing.ToString ();

		goldLb.text = gold.ToString ();
		maxGoldLb.text = maxGold.ToString ();

		manaLb.text = ((int)mana).ToString ();
		maxManaLb.text = maxMana.ToString ();

		crystalsLb.text = crystals.ToString ();
	}

	public void UpdateUnitsNo()
	{	
		int allUnits = 0;
		for (int i = 0; i <  existingUnits.Length; i++) 
		{
			allUnits += existingUnits [i];// + deployedUnits[i];
		}		
		unitsNoLb.text = allUnits.ToString ();// update total units
	}

	private void UpdateTimeCounter(int remainingTime)				//calculate remaining time
	{
		hours = (int)remainingTime/60; 
		minutes = (int)remainingTime%60;
		seconds = (int)(60 - (((UISlider)cloakBar.GetComponent("UISlider")).value*purchasedCloakTime*60)%60);	

		if (minutes == 60) minutes = 0;
		if (seconds == 60) seconds = 0;

		UpdateTimeLabel ();
	}

	private void UpdateTimeLabel()									//update the time labels on top
	{
		if(hours>0 && minutes >0 && seconds>=0 )
		{		
			((UILabel)cloakLb).text = 
				hours.ToString() +" h " +
				minutes.ToString() +" m " +
				seconds.ToString() +" s ";			
		}
		else if(minutes > 0 && seconds >= 0)
		{
			((UILabel)cloakLb).text = 
				minutes.ToString() +" m " +
				seconds.ToString() +" s ";
		}
		else if(seconds > 0 )
		{
			((UILabel)cloakLb).text = 
				seconds.ToString() +" s ";
		}
	}
}



/*

if (productionBuildings [0] > 0) 
		{
			if(gold<maxGold)
			{
				gold += (int) (productionBuildings [0] * productionRates [0]);
				isProducing=true;
			}
			else
			{
				((Messenger)statusMsg).DisplayMessage("Increase Gold storage capacity.");			
			}
		}
		if (productionBuildings [1] > 0) 
		{
			if(mana<maxMana)
			{
				mana += (int) (productionBuildings [1] * productionRates [1]);
				isProducing=true;
			}
			else
			{
				((Messenger)statusMsg).DisplayMessage("Increase Mana storage capacity.");
			}
		}




*/