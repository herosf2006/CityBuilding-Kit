using UnityEngine;
using System.Collections;

public class StatsBattle : MonoBehaviour {

	private const int noOfUnits = 10;//correlate with MenuUnitBase.cs/ verify that the Inspector has 12 elements

	public int[] 
		availableUnits = new int[noOfUnits],
		deployedUnits = new int[noOfUnits];

	public bool tutorialBattleSeen = false; 

	public UIProgressBar goldBar, manaBar, crystalsBar;
	public UILabel goldLb, manaLb, crystalsLb, remainingUnitsNo;
	public UIToggle soundToggle,ambientToggle,musicToggle;

	public float 
		gold = 0, 
		mana = 0;//increasing as map is attacked

	public int 
		crystals = 0, 
		unitsLost = 0, 
		buildingsDestroyed = 0,
		maxStorageGold = 0, //maximum loot existing on the map - necessary for progress bars
		maxStorageMana = 0, 
		maxCrystals = 0; 

	public GameObject GhostHelperBattle;
	private Component transData, soundFX;

	void Start () {

		transData = GameObject.Find("TransData").GetComponent<TransData>();
		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();

		LoadTransData ();
		UpdateUI ();
	}

	private void LoadTransData()
	{
		tutorialBattleSeen = ((TransData)transData).tutorialBattleSeen;
		if (!tutorialBattleSeen) GhostHelperBattle.SetActive (true);

		availableUnits = ((TransData)transData).goingToBattleUnits;

		if (!((TransData)transData).soundOn) 
		{			
			((SoundFX)soundFX).ChangeSound (((TransData)transData).soundOn);
			soundToggle.Start ();
			soundToggle.Set (false, false);
		}

		if (!((TransData)transData).ambientOn) 
		{
			((SoundFX)soundFX).ToggleAmbient ();
			ambientToggle.Start ();
			ambientToggle.Set (false, false);
		}

		if (!((TransData)transData).musicOn) 
		{
			((SoundFX)soundFX).ChangeMusic (((TransData)transData).musicOn);
			musicToggle.Start ();
			musicToggle.Set (false, false);
		}

	}

	public void ApplyMaxCaps()//cannot exceed storage+bought capacity
	{
		if (gold > maxStorageGold) { gold = maxStorageGold; }
		if (mana > maxStorageMana) { mana = maxStorageMana; }
	}

	public void UpdateUI()//updates numbers and progress bars
	{
		((UISlider)goldBar.GetComponent ("UISlider")).value = (float)gold/(float)maxStorageGold;
		((UISlider)manaBar.GetComponent ("UISlider")).value = (float)mana/(float)maxStorageMana;
		((UISlider)crystalsBar.GetComponent ("UISlider")).value = (float)crystals/(float)maxCrystals;
			
		goldLb.text = ((int)gold).ToString ();
		manaLb.text = ((int)mana).ToString ();
		crystalsLb.text = crystals.ToString ();
	}

	public void UpdateUnitsNo()
	{		
		int remainingUnits = 0;
		for (int i = 0; i <  availableUnits.Length; i++) 
		{
			remainingUnits += availableUnits[i];
		}		
		remainingUnitsNo.text = remainingUnits.ToString ();// update remaining units
	}

	public void ReturnHome()
	{
		Application.LoadLevel ("Game");
	}
}
