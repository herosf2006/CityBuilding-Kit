using UnityEngine;
using System.Collections;

public class TransData : MonoBehaviour {
	
	/*  
	  
	Important !!! - if you want to play directly the Map01 pvp level, activate the disabled TransData in the Hierarchy and play in editor; 

	This allows you to skip the hometown battle preparations.
	
	*/

	private const int 
		numberOfUnits = 10,								//coordinate with menu unit, etc
		numberOfBuildings = 12,
		numberOfWalls = 13,
		numberOfRemovables = 7;

	public int[] 
		housingPerUnit = new int[numberOfUnits],	
		goingToBattleUnits= new int[numberOfUnits],	
		returnedFromBattleUnits = new int[numberOfUnits],
		buildingValues = new int[numberOfBuildings],//value will be awarded to attackers
		wallGoldValues = new int[numberOfWalls],		
		removeTimes = new int[numberOfRemovables];  	

	public string[] buildingCurrency = new string[numberOfBuildings];//0 Gold 1 Mana 2 Crystals

	public int goldGained, manaGained, campaignLevel=-1;

	public bool battleOver = false, tutorialBattleSeen = true, soundOn = true, ambientOn = true, musicOn = true;
	private bool collectGained = false;

	private float addTime = 0.1f, addCounter = 0;
	private int messageCounter = 0;

	private Component stats, messenger;

	void Awake() {
		DontDestroyOnLoad(this);						// Do not destroy this game object:		
	} 

	public void ReturnFromBattle()
	{
		#if !UNITY_WEBPLAYER
		GameObject.Find ("SaveLoadMap").GetComponent<SaveLoadMap> ().LoadFromLocalFile ();
		#endif

		#if UNITY_WEBPLAYER
		GameObject.Find ("SaveLoadMap").GetComponent<SaveLoadMap> ().LoadFromPlayerPrefs ();
		#endif

		CleanDuplicate ();

		stats = GameObject.Find ("Stats").GetComponent<Stats> ();
		messenger = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();

		for (int i = 0; i < numberOfUnits; i++) 
		{	
			((Stats)stats).occupiedHousing += housingPerUnit[i]*returnedFromBattleUnits[i];
			((Stats)stats).existingUnits [i] = returnedFromBattleUnits [i];
		}
		((Stats)stats).tutorialBattleSeen = tutorialBattleSeen;
		collectGained = true;
		campaignLevel = -1;
	}

	private void CleanDuplicate()//since transdata is not destroyed at level load, now we have a duplicate
	{
		GameObject[] transDatas=GameObject.FindGameObjectsWithTag("TransData");

		if (transDatas.Length == 2) 
		{
			for (int i = 0; i < transDatas.Length; i++) 
			{
				if(!transDatas[i].GetComponent<TransData>().battleOver)
				{
					Destroy(transDatas[i]);			
					break;
				}
			}
		}
	}
		
	// Update is called once per frame
	void Update () {

		if (collectGained) //increases the available gold and mana with the loot
		{
			addCounter += Time.deltaTime;

			if(addCounter>=addTime)
			{

				addCounter = 0;

				if(goldGained>10)
				{
					int substract = goldGained/10;

					goldGained-= substract;
					((Stats)stats).gold += substract;
				}
				else if(goldGained>0)
				{
					goldGained--;
					((Stats)stats).gold ++;
				}
				if(manaGained>10)
				{
					int substract = manaGained/10;

					manaGained-= substract;
					((Stats)stats).mana += substract;
				}
				else if(manaGained>0)
				{
					manaGained--;
					((Stats)stats).mana ++;
				}

				((Stats)stats).ApplyMaxCaps();

				((Stats)stats).UpdateUI();

				if(goldGained==0 && manaGained==0)
					collectGained=false;

				messageCounter++;

				if(messageCounter>20)
				{
					((Messenger)messenger).DisplayMessage("Adding loot to our resources");
					((Stats)stats).VerifyMaxReached();
					messageCounter=0;
				}
			}
		}

	}
}
