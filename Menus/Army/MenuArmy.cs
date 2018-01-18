using UnityEngine;
using System.Collections;

public class MenuArmy : MonoBehaviour {//the panel with all the units, also used for selecting the army before attack

	private const int unitsNo = 10, buildingsNo = 12;  //correlate with MenuUnitBase.cs

	public UILabel[] 
		existingUnitsLb = new UILabel[unitsNo];		

	public GameObject loadingLb;

	private Component removableCreator, transData, saveLoadMap, stats, statusMsg, soundFX;

	public StructureCreator buildingCreator;

	// Use this for initialization
	void Start () {

		removableCreator = GameObject.Find ("RemovableCreator").GetComponent<RemovableCreator> ();

		stats = GameObject.Find("Stats").GetComponent <Stats>();
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();
		transData = GameObject.Find("TransData").GetComponent<TransData>();
		saveLoadMap = GameObject.Find("SaveLoadMap").GetComponent<SaveLoadMap>();
		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();
	
	}

	public void LoadCampaign0(){LoadCampaign (0);}
	public void LoadCampaign1(){LoadCampaign (1);}
	public void LoadCampaign2(){LoadCampaign (2);}
	public void LoadCampaign3(){LoadCampaign (3);}
	public void LoadCampaign4(){LoadCampaign (4);}
	public void LoadCampaign5(){LoadCampaign (5);}
	public void LoadCampaign6(){LoadCampaign (6);}
	public void LoadCampaign7(){LoadCampaign (7);}
	public void LoadCampaign8(){LoadCampaign (8);}
	public void LoadCampaign9(){LoadCampaign (9);}
	public void LoadCampaign10(){LoadCampaign (10);}
	public void LoadCampaign11(){LoadCampaign (11);}
	public void LoadCampaign12(){LoadCampaign (12);}
	public void LoadCampaign13(){LoadCampaign (13);}
	public void LoadCampaign14(){LoadCampaign (14);}
	public void LoadCampaign15(){LoadCampaign (15);}
	public void LoadCampaign16(){LoadCampaign (16);}
	public void LoadCampaign17(){LoadCampaign (17);}
	public void LoadCampaign18(){LoadCampaign (18);}
	public void LoadCampaign19(){LoadCampaign (19);}
	public void LoadCampaign20(){LoadCampaign (20);}
	public void LoadCampaign21(){LoadCampaign (21);}


	private void LoadCampaign(int campaignLevel)
	{
		((TransData)transData).campaignLevel = campaignLevel;
		LoadMultiplayer0 ();
	}

	public void LoadMultiplayer0()
	{	
		bool unitsExist = false;

		for (int i = 0; i < ((Stats)stats).existingUnits.Length; i++) 
		{
			if(((Stats)stats).existingUnits[i]>0)
			{
				unitsExist = true;
				break;
			}
		}
		if (unitsExist && ((TransData)transData).campaignLevel != -1) {
			StartCoroutine (LoadMultiplayerMap (0)); 		
		}
		else if(!unitsExist)
		{
			((Messenger)statusMsg).DisplayMessage("Train units for battle.");
		}
		else if(unitsExist && 
			((((Stats)stats).gold >= 250&&((TransData)transData).campaignLevel==-1)))		
		{
			StartCoroutine(LoadMultiplayerMap(0)); 
		}
		else if(!unitsExist)
		{
			((Messenger)statusMsg).DisplayMessage("Train units for battle.");
		}
		else
		{
			((Messenger)statusMsg).DisplayMessage("You need more gold.");
		}
	
	}

	private IEnumerator LoadMultiplayerMap(int levelToLoad)				//building loot values = half the price
	{	
		if(((TransData)transData).campaignLevel==-1)
		((Stats)stats).gold -= 250;										//this is where the price for the battle is payed, before saving the game

		int[] existingUnits = new int[unitsNo];//((Stats)stats).existingUnits;

		for (int i = 0; i < existingUnits.Length; i++) 
		{
			((Stats)stats).occupiedHousing -= ((Stats)stats).sizePerUnit[i]*((Stats)stats).existingUnits[i];//existingUnits[i]
			existingUnits[i] = ((Stats)stats).existingUnits [i];
			((Stats)stats).existingUnits [i] = 0;
		}

		((Stats)stats).UpdateUI ();// - optional- no element of the UI is visible at this time
		((Stats)stats).UpdateUnitsNo();

		#if !UNITY_WEBPLAYER
		((SaveLoadMap)saveLoadMap).SaveGameLocalFile ();							//local autosave at battle load
		#endif

		#if UNITY_WEBPLAYER
		((SaveLoadMap)saveLoadMap).SaveGamePlayerPrefs ();
		#endif

		loadingLb.SetActive (true);
		((TransData)transData).removeTimes = ((RemovableCreator)removableCreator).removeTimes;
		((TransData)transData).housingPerUnit = ((Stats)stats).sizePerUnit;
		((TransData)transData).goingToBattleUnits = existingUnits;
		((TransData)transData).tutorialBattleSeen = ((Stats)stats).tutorialBattleSeen;

		((TransData)transData).soundOn = ((SoundFX)soundFX).soundOn;
		((TransData)transData).ambientOn = ((SoundFX)soundFX).ambientOn;
		((TransData)transData).musicOn = ((SoundFX)soundFX).musicOn;

		for (int i = 0; i < buildingsNo; i++) 
		{
			((TransData)transData).buildingValues [i] = int.Parse(buildingCreator.structures [i] ["Price"]);
			((TransData)transData).buildingCurrency [i] = buildingCreator.structures [i] ["Currency"];
		}

		yield return new WaitForSeconds (0.2f);
		switch (levelToLoad) 
		{
		case 0:	 
			Application.LoadLevel("Map01");
			break;		
		}
	}

	public void UpdateStats()
	{
		StartCoroutine ("LateUpdateStats");
	}

	private IEnumerator LateUpdateStats()
	{
		yield return new WaitForSeconds (0.2f);
		for (int i = 0; i < existingUnitsLb.Length; i++) {
			existingUnitsLb [i].text = ((Stats)stats).existingUnits [i].ToString ();
		}
	}

	}
