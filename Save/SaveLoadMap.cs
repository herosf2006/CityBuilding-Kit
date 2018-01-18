using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.IO;
using System.Text;
//																	## This is for local saving / loading ##  
/*
public struct ConStruct
{
	public string buildingTag;
	public int 
	structureIndex,
	buildingTime,
	remainingTime,
	storageIncrease;
}
*/
public class SaveLoadMap : SaveLoadBase {

	//private List<ConStruct[]> conStruct = new List<ConStruct[]>();


	//protected string filePath;
	private string
		fileNameLocal = "LocalSave",
		fileNameServer = "ServerSave",
		fileNameAttack = "ServerAttack",
		fileExt = ".txt";

	//PlayerPrefs save variables
	private List<string> savefileStrings = new List<string>();

	/* 	
	    //save file data structure
	    Grass: grassType, grassIndex, position.x, position.y
		Buildings: buildingType, structureIndex, position.x, position.y
		Construction: buildingType, constructionIndex, buildingTime, remainingTime, storageIncrease, position.x, position.y
		Removables: removableType, removableIndex, removeTime, inRemoval, goldPrice, manaPrice, goldGain, manaGain,  position.x,position.Y
		Units: currentSlidVal, currentTrainingTime
		Units: trainingTimes, existingUnits, battleUnits
		Units: queList = qIndex, objIndex, trainingIndex
		Stats: experience,dobbits,occupiedDobits,currentPopulation,maxPopulation,gold,mana,crystals,maxStorageGold,maxStorageMana,maxCrystals,productionRateGold,productionRateMana,tutorialCitySeen,tutorialBattleSeen,removablesCreated,soundOn,musicOn
	*/

	//public GameObject 
		//DamagePanel;

	private GameObject[] RemovableTimers;//, Buildings
	//private List<GameObject> Buildings = new List<GameObject>();

	private List<GameObject> 
	positionalStructures = new List<GameObject>(),//buildings and weapons can have different base sizes
	gridStructures = new List<GameObject>();//grid

	public UILabel goldLostLb, manaLostLb, buildingsLostLb, unitsLostLb;

	private Component relay;

	// Use this for initialization
	void Start () 
	{
		InitializeComponents ();	                                    

		relay = GameObject.Find("Relay").GetComponent<Relay>();

		filePath = Application.persistentDataPath +"/";//other platforms'=

		//LoadFromLocal ();	//automatic local load at startup - for local testing; for multiplayer, 
							//the user must both upload/download from server to receive the attack results
	}

	void OnApplicationQuit() //autosave
	{

		//if(!((Relay)relay).pauseInput)//check if the user is doing something, like moving buildings


		//SaveGameLocalFile();
	}


	public void SaveGamePlayerPrefs()
	{
		isFileSave = false;
		SaveGame ();
	}

	public void SaveGameLocalFile()
	{
		isFileSave = true;
		SaveGame ();
	}

	private void WriteNextLine(string newLine)
	{
		if (isFileSave) 
		{
			sWriter.WriteLine (newLine);
		}
		else
		{
			savefileStrings.Add(newLine);
		}
	}


	private void SaveGame()
	{
		((Stats)stats).gameWasLoaded = true;//saving the game also prevents the user from loading on top

		if (isFileSave) 
		{
			print ("Save Game Path on your system: " + filePath.ToString ());
			sWriter = new StreamWriter (filePath + fileNameLocal + fileExt);
		}
			else
		{
			print (	"Saving to PlayerPrefs.\n"+
			       	"PlayerPrefs are stored in HKEY_CURRENT_USER/SOFTWARE/StartupKits/StrategyKit (Run regedit)");
			savefileStrings.Clear ();

		}

		ReadStructures();

		WriteNextLine ("###StartofFile###");

		WriteNextLine ("###PosStruct###");
		
		for (int i = 0; i < positionalStructures.Count; i++) 
		{
			Component sSel = positionalStructures[i].GetComponent("StructureSelector");
				
			WriteNextLine	(((StructureSelector)sSel).structureClass+","+ 	
							((StructureSelector)sSel).structureType+","+
			              	((StructureSelector)sSel).structureIndex.ToString()+","+
						  	((StructureSelector)sSel).grassType.ToString()+","+
							positionalStructures[i].transform.position.x+","+
							positionalStructures[i].transform.position.y
			              );

		}

		WriteNextLine ("###GridStruct###");

		for (int i = 0; i < gridStructures.Count; i++) 
		{					
			Component sSel = gridStructures[i].GetComponent("StructureSelector");

			WriteNextLine	(((StructureSelector)sSel).structureClass+","+  
							((StructureSelector)sSel).structureType+","+
							((StructureSelector)sSel).structureIndex.ToString()+","+
							((StructureSelector)sSel).iRow.ToString()+","+
							((StructureSelector)sSel).jCol.ToString()
			);
		}

		WriteNextLine ("###Construction###");
		for (int i = 0; i < Construction.Length; i++) 
		{
			Component cSel = Construction[i].GetComponent("ConstructionSelector");
			WriteNextLine   (((ConstructionSelector)cSel).structureClass+","+
							((ConstructionSelector)cSel).structureType+","+
				            ((ConstructionSelector)cSel).constructionIndex.ToString()+","+
							((ConstructionSelector)cSel).grassType.ToString()+","+
				            ((ConstructionSelector)cSel).buildingTime+","+
				            ((ConstructionSelector)cSel).remainingTime+","+
				            ((ConstructionSelector)cSel).storageAdd+","+									            
							((ConstructionSelector)cSel).iRow+","+
							((ConstructionSelector)cSel).jCol+","+							
							
							Construction[i].transform.position.x+","+
							Construction[i].transform.position.y	
			              );
		}

		WriteNextLine ("###Removables###");

		//NO NEED TO SAVE THIS SINCE IT WILL BE RECOVERED FROM RemovableCreator AT LOAD
		//int[] removeTimeArray = ((RemovableCreator)removableCreator).removeTimeArray;
		//WriteNextLine (string.Join(",", Array.ConvertAll(removeTimeArray, x => x.ToString())));  // array conversion to csv

		for (int i = 0; i < Removables.Length; i++) 
		{
			Component rSel = Removables[i].GetComponent("RemovableSelector");
				
			WriteNextLine (((RemovableSelector)rSel).removableType+","+
			              ((RemovableSelector)rSel).removableIndex.ToString()+","+
			              ((RemovableSelector)rSel).inRemoval.ToString()+","+
			              ((RemovableSelector)rSel).iColumn.ToString()+","+
			              ((RemovableSelector)rSel).jRow.ToString()
			              );
		}

		WriteNextLine ("###RemovableTimers###"); 

		for (int i = 0; i < RemovableTimers.Length; i++) 
		{
			Component rTimer = RemovableTimers[i].GetComponent("RemovableTimerSelector");
				
			WriteNextLine (((RemovableTimerSelector)rTimer).removableIndex.ToString()+","+
			              ((RemovableTimerSelector)rTimer).remainingTime.ToString());
		}

		WriteNextLine ("###Numerics###");//the unique id for buildings/grass patches
		
		WriteNextLine (((StructureCreator)buildingCreator).structureIndex.ToString()+","+
			((StructureCreator)wallCreator).structureIndex.ToString()+","+
			((StructureCreator)weaponCreator).structureIndex.ToString());		                     
		
		WriteNextLine (((UnitProc)unitProc).currentSlidVal.ToString("0.00")+","+
		                   (((UnitProc)unitProc).currentTrainingTime)		                            
		                   );
		
		const int numberOfUnits = 10;
		int[] trainingTimes = new int[numberOfUnits];//an array that holds training times from all units - 
		//at first load, the XML will not have been read 
		int[] sizePerUnit = new int[numberOfUnits];
		int[] existingUnits = new int[numberOfUnits];//these units form the current population

		
		trainingTimes = ((UnitProc)unitProc).trainingTimes;//replace our empty array with the xml values, already in unitproc
		sizePerUnit = ((UnitProc)unitProc).sizePerUnit;
		existingUnits = ((Stats)stats).existingUnits;
				
		WriteNextLine (String.Join(",", new List<int>(trainingTimes).ConvertAll(i => i.ToString()).ToArray()));
		WriteNextLine (String.Join(",", new List<int>(sizePerUnit).ConvertAll(i => i.ToString()).ToArray()));
		WriteNextLine (String.Join(",", new List<int>(existingUnits).ConvertAll(i => i.ToString()).ToArray()));
			
		List<Vector3> queList = new List<Vector3>();
		queList=((UnitProc)unitProc).queList;
		
		for (int i = 0; i < queList.Count; i++) 
		{
			WriteNextLine(queList[i].ToString().Trim(new Char[] { ')', '(' }));
		}


		WriteNextLine ("###Economy###");

		existingEconomyBuildings.Clear ();
		existingEconomyBuildings = ((ResourceGenerator)resourceGenerator).existingEconomyBuildings;

		for (int i = 0; i < existingEconomyBuildings.Count; i++) 
		{			

			WriteNextLine (existingEconomyBuildings [i].structureIndex + "," +
				(int)existingEconomyBuildings [i].storedGold + "," +
				(int)existingEconomyBuildings [i].storedMana);
		}


		WriteNextLine ("###Stats###");
		
		WriteNextLine (
					  ((Stats)stats).level+","+
			          ((Stats)stats).townHallLevel+","+
			          ((Stats)stats).structureIndex+","+
					  ((Stats)stats).experience+","+
					  ((Stats)stats).maxExperience+","+
		              ((Stats)stats).dobbits+","+
		              ((Stats)stats).occupiedDobbits+","+
		              ((Stats)stats).occupiedHousing+","+
		              ((Stats)stats).maxHousing+","+
		              ((Stats)stats).gold+","+
		              ((Stats)stats).mana+","+
		              ((Stats)stats).crystals+","+
		              ((Stats)stats).maxGold+","+
		              ((Stats)stats).maxMana+","+
		              ((Stats)stats).maxCrystals+","+		           
		              ((Stats)stats).tutorialCitySeen+","+
		              ((Stats)stats).tutorialBattleSeen+","+
		              ((Stats)stats).removablesCreated+","+
		              ((SoundFX)soundFX).soundOn+","+
					  ((SoundFX)soundFX).ambientOn+","+
					  ((SoundFX)soundFX).musicOn
		              );


		WriteNextLine (System.DateTime.Now.ToString());
		
		WriteNextLine ("###EndofFile###");

		if (isFileSave) 
		{
			sWriter.Flush ();
			sWriter.Close ();
		}
		else
		{
			savefile = "";

			for (int i = 0; i < savefileStrings.Count; i++) 
			{
				savefile += savefileStrings[i]+"\n";
			}

			PlayerPrefs.SetString("savefile", savefile);
			PlayerPrefs.Save ();
		}
		existingBuildings = new int[buildingTypesNo];//reset for next save - remove if automatic
		((Messenger)statusMsg).DisplayMessage("Game saved.");
	}

	private void ReadStructures()//reads all buildings/grass/under construction
	{ 
		Construction = GameObject.FindGameObjectsWithTag ("Construction");//find all buildings under construction
		Removables = GameObject.FindGameObjectsWithTag ("Removable");
		RemovableTimers = GameObject.FindGameObjectsWithTag ("RemovableTimer");
		//Grass = GameObject.FindGameObjectsWithTag ("Grass");//finds all patches of grass from underneath the buildings

		GameObject[] structures = GameObject.FindGameObjectsWithTag ("Structure");
		ClearStructureLists ();

		foreach (GameObject structure in structures) 
		{
			string structureClass = structure.GetComponent<StructureSelector> ().structureClass;

			switch (structureClass) 
			{
			case "Building":				
				positionalStructures.Add(structure);
				break;
			case "Weapon":
				positionalStructures.Add(structure);
				break;

			case "StoneWall":
				gridStructures.Add (structure);
				break;
			case "WoodFence":
				gridStructures.Add (structure);
				break;			
			case "Ambient":
				gridStructures.Add (structure);
				break;
			}
		}
	}

	private void ClearStructureLists()
	{
		positionalStructures.Clear ();
		gridStructures.Clear ();	
	}

	private bool CheckLocalSaveFile()
	{
		((Messenger)statusMsg).DisplayMessage("Checking for local save file...");
		bool localSaveExists = File.Exists(filePath + fileNameLocal + fileExt);
		return(localSaveExists);
	}

	private bool CheckPlayerPrefsSaveFile()
	{
		((Messenger)statusMsg).DisplayMessage("Checking for PlayerPrefs save file...");
		bool localSaveExists = PlayerPrefs.HasKey("savefile");
		return(localSaveExists);
	}

	private bool CheckLoadOnce()
	{
		bool gameWasLoaded = ((Stats)stats).gameWasLoaded;

		if(gameWasLoaded) //prevents loading twice, since there are no safeties and the procedure should be automated at startup, not button triggered
		{
			((Messenger)statusMsg).DisplayMessage("Only one load per session is allowed. Canceling...");
			((Messenger)statusMsg).DisplayMessage("Unless you just generated the trees - this prevents you from loading houses on top of trees.");
			((Messenger)statusMsg).DisplayMessage("Next time, generate the trees before you do anything else - then save/load the game.");
		}

		return gameWasLoaded;
	}

	public void LoadFromLocalFile()
	{
		if(CheckLoadOnce ()) return;

		if(CheckLocalSaveFile())
		{
			((Messenger)statusMsg).DisplayMessage("Local save file found.");
			//StreamReader 
			sReader = new StreamReader(filePath + fileNameLocal + fileExt);

			isFileSave = true;//this is a local file save, not player prefs registry record
			LoadGame ();
		}
		else
		{
			((Messenger)statusMsg).DisplayMessage("No local save file found.");
			((SoundFX)soundFX).ChangeMusic (true);//first run, start music
		}
	}

	public void LoadFromPlayerPrefs()
	{
		if(CheckLoadOnce ()) return;
		
		if(CheckPlayerPrefsSaveFile())
		{
			((Messenger)statusMsg).DisplayMessage("PlayerPrefs save file found.");
			savefile = PlayerPrefs.GetString("savefile");

			isFileSave = false;//this is a player prefs registry record load
			LoadGame ();
		}
		else
		{
			((Messenger)statusMsg).DisplayMessage("No local save file found.");
			((SoundFX)soundFX).ChangeMusic (true);//first run, start music
		}

	}

	public void LoadFromServer()
	{
		if(CheckLoadOnce ()) return;

		//StreamReader 
		sReader = new StreamReader(filePath + fileNameServer + fileExt);

		isFileSave = true;
		LoadGame ();
	}

	public void LoadPlayerPrefsAttackFromServer()
	{	
		string text= PlayerPrefs.GetString(fileNameAttack);
		AttackPlayerPrefsDamage (text);
	}

	public void LoadAttackFromServer()
	{
		//	StreamReader 
		sReader = new StreamReader(filePath + fileNameAttack + fileExt);
		AttackDamage (sReader);
	}

	private void AttackPlayerPrefsDamage(string text)
	{
		string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
		currentLine = lines[1];//skip header
				
		string[] losses = currentLine.Split(","[0]);
		
		int goldLost = int.Parse(losses[0]);
		int manaLost = int.Parse(losses[1]);
		int buildingsLost = int.Parse(losses[2]);
		int unitsLost = int.Parse(losses[3]);
		
		if (goldLost == 0 && manaLost == 0) 
		{
			return;//the file exists, but has been loaded and reset
		}
		
		goldLostLb.text = goldLost.ToString ();
		manaLostLb.text = manaLost.ToString ();
		buildingsLostLb.text = buildingsLost.ToString ();
		unitsLostLb.text = unitsLost.ToString ();
		
		((Stats)stats).gold -= goldLost;
		((Stats)stats).mana -= manaLost;
		((Stats)stats).UpdateUI();
		//menuMain.OnCloseSettings();
		StartCoroutine("ActivateDamagePanel");
	}

	private void AttackDamage(StreamReader sReader)
	{

		currentLine = "";

		currentLine = sReader.ReadLine();//skip header

		currentLine = sReader.ReadLine();//read gold/mana

		string[] losses = currentLine.Split(","[0]);

		int goldLost = int.Parse(losses[0]);
		int manaLost = int.Parse(losses[1]);
		int buildingsLost = int.Parse(losses[2]);
		int unitsLost = int.Parse(losses[3]);

		if (goldLost == 0 && manaLost == 0) 
		{
			return;//the file exists, but has been loaded and reset
		}

		goldLostLb.text = goldLost.ToString ();
		manaLostLb.text = manaLost.ToString ();
		buildingsLostLb.text = buildingsLost.ToString ();
		unitsLostLb.text = unitsLost.ToString ();

		((Stats)stats).gold -= goldLost;
		((Stats)stats).mana -= manaLost;
		((Stats)stats).UpdateUI();
		//menuMain.OnCloseSettings();
		StartCoroutine("ActivateDamagePanel");
	}

	private IEnumerator ActivateDamagePanel()//keeps trying to launch the damage panel, waiting fot the user to finish other tasks, if any
	{										 //otherwise the panel is superimposed on other panels	
		yield return new WaitForSeconds (2);
		if(!((Relay)relay).pauseInput)
		{
			//((Relay)relay).pauseInput = true;
			if(GhostHelper.activeSelf){ ((GhostHelper)ghostHelper).ResetHelper();}

			menuMain.OnDamage();

		}
		else
			StartCoroutine("ActivateDamagePanel");
	}
	/*
	void OnGUI()
	{
		if (GUI.Button (new Rect (245, Screen.height - 140, 45, 25), "menu")) {
			//I will leave this on manual control; When save/load are automated, include this as well
			menuMain.OnDamage ();
		}
		if (GUI.Button (new Rect (445, Screen.height - 140, 45, 25), "close")) {
			//I will leave this on manual control; When save/load are automated, include this as well
			menuMain.OnCloseDamage ();
		}

	}
	*/
}

//2498 lines before refactoring
