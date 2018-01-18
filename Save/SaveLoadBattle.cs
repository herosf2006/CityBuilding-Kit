using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.IO;
using System.Text;


public class SaveLoadBattle : SaveLoadBase {

	//private int 
		//constructionZ = 0, 		//normally = 1, same as , but creates a zdepth problem for the construction dobbits/soldiers
		//cannonZ = -5,
		//damageZ = -7,
		//buildingIndexOverride = -1, //many lists of elements must be mirrored
		//grassIndexOvveride = -1;		
	//protected string filePath;
	private string 
		//filePath,
		fileNameLocal = "LocalSave",
		myMapID = "MyMapID",				//local - saves the map id from server IlXcUovzS6
		battleSaveFile = "BattleMap",
		fileExt = ".txt",
		myUseridFile,						//gfdfghjke.txt
		battleMapID,
		attackExt = "_results",				//_attack
		nullMapIDCode = "0000000000";		//the server avoids loading own map; if there is no own map, an error is displayed in console

	public string 											//http://citybuildingkit.com/costin/admin.php
		serverAddress = "http://citybuildingkit.com/costin/public/",		
		filesAddress = "get_match.php",						//get_match.php?license=U125UNITYDEMO
		matchAddress = "finish_match.php",
		license = "U125UNITYDEMO",							//"costin/finish_match.php"
		myMapIDCode;

	public string[] campaignNo = new string[22];
	private WWW w2;

	//private List<GameObject> 
		//inRemovalList = new List<GameObject>();

	public GameObject 
		//BuildingCannon, 
		//DamageBar, 
		GoHomeBt;

	//private GameObject //objects used to parent buildings/cannons , once they are instantiated
		//GroupCannons, 
		//GroupDamageBars;
		
	//lists for these elements - unknown number of elements
	//private List<GameObject> //make private
		//LoadedCannons = new List<GameObject>(), //list is used to parent the cannons to the cannon group
		//LoadedDamageBars = new List<GameObject>();//list is used to parent the damagebars to the damagebar group

	private bool oneLoad = true;
	private Component statsBattle;//removableCreator, 
		
	// Use this for initialization
	void Start () 
	{
		isBattleMap = true;
		InitializeComponents ();

		statsBattle = GameObject.Find ("StatsBattle").GetComponent<StatsBattle> ();
		GroupDamageBars = GameObject.Find ("GroupDamageBars");
		filePath = Application.persistentDataPath +"/";//other platforms

		//filePath = Application.dataPath + "/";//windows - same folder as the project
		//filePath = Application.persistentDataPath +"/";//other platforms

		//LoadGame(); //normal, server random load
		LoadBattleGame();
	}

	private bool CheckServerSaveFile()//LOCAL recording of a previous save on server
	{
		bool serverSaveExists = false;//checks if the mapcode was saved locally, not if it is still available on server, to avoid attacking your own city

		#if !UNITY_WEBPLAYER	
		serverSaveExists = File.Exists(filePath + myMapID + fileExt);
		#endif

		#if UNITY_WEBPLAYER	
		serverSaveExists = PlayerPrefs.HasKey("mapid");
		#endif

		return(serverSaveExists);
	}

	public void ReadMyMapID()
	{
		#if !UNITY_WEBPLAYER	
		StreamReader sReader = new StreamReader(filePath + myMapID + fileExt);
		myMapIDCode = sReader.ReadLine();
		#endif
		
		#if UNITY_WEBPLAYER	
		myMapIDCode = PlayerPrefs.GetString("mapid");
		#endif
	}

	private void SaveBattleMap()//saves a copy of the server map locally
	{
		#if !UNITY_WEBPLAYER
		StreamWriter sWriter = new StreamWriter (filePath + battleSaveFile + fileExt);
		sWriter.Write(w2.text);
		sWriter.Flush ();
		sWriter.Close ();
		#endif

		#if UNITY_WEBPLAYER	
		PlayerPrefs.SetString("battlesave", w2.text);
		PlayerPrefs.Save ();
		#endif

		//LoadMap();
	}

	public void SaveAttack()
	{
		StartCoroutine("SendAttackToServer");

		//optional - write file locally
		/*
		StreamWriter sWriter = new StreamWriter (filePath + battleResultSaveFile + fileExt);

		sWriter.WriteLine ("###StartofFile###");
		//the battle results file must pass the losses to the attacked user id
		sWriter.WriteLine(
							((StatsBattle)StatsBattleSc).gold.ToString()+","+
		                  	((StatsBattle)StatsBattleSc).mana.ToString()
		                  );

		sWriter.WriteLine ("###EndofFile###");

		sWriter.Flush ();
		sWriter.Close ();
		StartCoroutine("SendResultsToServer");
		*/

	}

	IEnumerator SendAttackToServer()   
	{
		((Messenger)statusMsg).DisplayMessage("Uploading battle results.");
		//byte[] levelData = System.IO.File.ReadAllBytes(filePath + battleResultSaveFile + fileExt);//full local save file

		byte[] levelData;

		levelData = Encoding.ASCII.GetBytes("###StartofFile###\n" +// gold,mana,buildingsDestroyed,unitsLost
		                                           ((StatsBattle)statsBattle).gold.ToString () + "," +
		                                           ((StatsBattle)statsBattle).mana.ToString ()+  "," +
		                                           ((StatsBattle)statsBattle).buildingsDestroyed.ToString ()+  "," +
		                                           ((StatsBattle)statsBattle).unitsLost.ToString ()+
		                                           "\n###EndofFile###");

		WWWForm form = new WWWForm();

		form.AddField("savefile","file");
		
		form.AddBinaryData( "savefile", levelData, battleMapID + attackExt,"text/xml");//file

		//change the url to the url of the php file
		WWW w = new WWW(serverAddress + filesAddress +"?mapid=" + battleMapID + attackExt +"&license="+license, form);//myUseridFile 
			
		yield return w;
		if (w.error != null)
		{
			print("error");
			print ( w.error ); 
			((Messenger)statusMsg).DisplayMessage("Network error.");
		}
		else
		{
			bool ready = false;
			#if !UNITY_WEBPLAYER	
			ready = (w.uploadProgress == 1 && w.isDone);
			#endif

			#if UNITY_WEBPLAYER	
			ready = w.isDone;
			#endif

			//this part validates the upload, by waiting 5 seconds then trying to retrieve it from the web
			if(ready)//w.uploadProgress == 1 && w.isDone
			{
				//print ( "Sent File " + myMapIDCode + " Contents are: \n\n" + w.text);
				
				yield return new WaitForSeconds(5);
				//change the url to the url of the folder you want it the levels to be stored, the one you specified in the php file
				WWW w2 = new WWW(serverAddress + filesAddress +"?get_user_map=1&mapid=" + battleMapID + attackExt+"&license="+license);//returns a specific map
				
				yield return w2;
				if(w2.error != null)
				{
					print("error 2");
					print ( w2.error ); 
					((Messenger)statusMsg).DisplayMessage("Attack file check error.");
				}
				else
				{
					//then if the retrieval was successful, validate its content to ensure the level file integrity is intact
					if(w2.text != null && w2.text != "")
					{
						if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
						{
							//and finally announce that everything went well
							print ( "Received Verification File " + battleMapID + attackExt + " Contents are: \n\n" + w2.text);//file
							((Messenger)statusMsg).DisplayMessage("Uploaded attack results to "  + battleMapID + attackExt );
						}
						else
						{
							print ( "Level File " + battleMapID + attackExt + " is Invalid");//file
							print ( "Received Verification File " + battleMapID + attackExt + " Contents are: \n\n" + w2.text);//file although incorrect, prints the content of the retrieved file
							((Messenger)statusMsg).DisplayMessage("Uploaded attack failed.");
						}
					}
					else
					{
						print ( "Level File " + battleMapID + attackExt + " is Empty");//file
						((Messenger)statusMsg).DisplayMessage("Attack file is empty?");
					}
				}

			}     
		}
		GoHomeBt.SetActive(true);//display go home button regardles of upload success, so the user can go back to hometown/exit the game
	}
	/*
	public void LoadCampaignFromServer()
	{
		StartCoroutine("DownloadCampaignMap");//force the local level save before this

		((Messenger)statusMsg).DisplayMessage("Downloading campaign map.");
	}
	*/
	public void LoadMapFromServer()
	{
		StartCoroutine("DownloadBattleMap");//force the local level save before this

		((Messenger)statusMsg).DisplayMessage("Downloading random map.");
	}

	IEnumerator DownloadBattleMap()   
	{
		if(CheckServerSaveFile())		
			ReadMyMapID();		
		else 			
			myMapIDCode = nullMapIDCode;

		//loads a map other than the user map; if there is only one map on the server - your own, loads your own map

		int campaignLevel = ((TransData)transData).campaignLevel;

		if (campaignLevel == -1)
			w2 = new WWW (serverAddress + filesAddress + "?get_random_map=1&mapid=" + myMapIDCode + "&license=" + license); //mapid with the get_random_map to prevent the user's map from being downloaded by accident
		else 
		{
			battleMapID = "0000camp" + campaignNo [campaignLevel];
			w2 = new WWW (serverAddress + matchAddress + "?get_user_map=1&mapid=" + battleMapID + "&license=" + license);
		
		}
		//w2 = new WWW(serverAddress + matchAddress + "?get_user_map=1&mapid=" + myMapIDCode+"&license="+license);//download own map reference - no mapid, so it doesn't work here
		yield return w2;
		
		if(w2.error != null)
		{
			print("Server load error" + w2.error);
			((Messenger)statusMsg).DisplayMessage("Map download error.");
		}
		
		else
		{
			//then if the retrieval was successful, validate its content to ensure the level file integrity is intact
			if(w2.text != null && w2.text != "")
			{
				if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
				{

					string[] temp = w2.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
					if(campaignLevel==-1)
					{
					battleMapID = temp[1];//skip ###StartofFile### then read ID
					}

					print ( "Downloaded File "+ battleMapID + " Contents are: \n\n" + w2.text);

					SaveBattleMap();
					sReader = new StreamReader(filePath + battleSaveFile + fileExt);

					StartCoroutine ("LateLoadGame");
					((Messenger)statusMsg).DisplayMessage("Map downloaded.");
				}
				
				else
				{			
					print ( "Random Level File is Invalid. Contents are: \n\n" + w2.text);
					//although incorrect, prints the content of the retrieved file
					((Messenger)statusMsg).DisplayMessage("License code not added in Map01\nor downloaded file corrupted."); 

					((Messenger)statusMsg).DisplayMessage("If you have your own save file on server\n" +
						"delete local files or change your 10 character ID in:\n" +
					    "C:/Users/user/AppData/LocalLow/\n" +
					    "AStarterKits/StrategyKit/MyMapID.txt");
				}
			}
			else
			{
				print ( "Random Level File is Empty");
				((Messenger)statusMsg).DisplayMessage("Downloaded file empty?");
			}

		}
	}

	private IEnumerator LateLoadGame()
	{
		yield return new WaitForSeconds (0.5f);
		LoadGame ();
		BattleMapInstructions ();
	}

	private void BattleMapInstructions()
	{
		GameObject tempHelios = GameObject.Find ("Helios");

		tempHelios.GetComponent<Helios>().Buildings = LoadedBuildings;
		//tempHelios.GetComponent<Helios> ().Cannons = LoadedCannons;
		tempHelios.GetComponent<Helios> ().DamageBars = LoadedDamageBars;
		GameObject tempGridManager = GameObject.Find ("GridManager");
		tempGridManager.GetComponent<GridManager>().UpdateObstacles();
		tempHelios.GetComponent<Helios>().NetworkLoadReady();
		((SoundFX)soundFX).BattleMapSpecific ();

	}
	public void LoadBattleGame()
	{
		if(!oneLoad) {return;}//prevents loading twice, since there are no safeties and the procedure should be automated at startup, not button triggered
		oneLoad = false;

		//isBattleMap = true;
		//StreamReader sReader = new StreamReader(filePath + fileName + fileExt);//load local file instead of server, same format

		LoadMapFromServer();
			
	}
		
	private void LoadMap()
	{
		#if !UNITY_WEBPLAYER	
		isFileSave = true;
		//LoadGameMapPlayerPrefs();
		LoadGame();
		//LoadGameMapFile();
		#endif
		
		#if UNITY_WEBPLAYER	
		isFileSave = false;
		LoadGameMapPlayerPrefs();
		#endif
	}

	private bool CheckPlayerPrefsSaveFile()
	{
		// loads the battle map from the playerprefs save - could be loaded from w2.text directly	//	PlayerPrefs.SetString("battlesave", w2.text);
	
		((Messenger)statusMsg).DisplayMessage("Checking for PlayerPrefs battle map save file...");
		bool localSaveExists = PlayerPrefs.HasKey("battlesave");
		return(localSaveExists);
	}

	private void InstantiateObjects()
	{	
		InstantiateConstructions ();
		InstantiateRemovables ();
		InstantiateRemovableTimers ();
	}

	private void ProcessZ()
	{
		for (int i = 0; i < LoadedBuildings.Count; i++) 
		{
			Vector3 pivotPos = LoadedBuildings[i].transform.GetChild (1).position; //pivot
			Vector3 spritesPos = LoadedBuildings[i].transform.GetChild (2).position;//sprites
			//Vector3 pos = selectedBuilding.transform.position;
			
			float correctiony = 10 / (pivotPos.y + 3300);//ex: fg 10 = 0.1   bg 20 = 0.05  
			//all y values must be positive, so we add the grid origin y 3207 +100 to avoid divide by 0; 
			//otherwise depth glitches around y 0
			
			LoadedBuildings[i].transform.GetChild(2).position = new Vector3(spritesPos.x, spritesPos.y, zeroZ - correctiony);//	transform.GetChild(2).position   
		}
		
		for (int i = 0; i < LoadedConstructions.Count; i++) 
		{
			Vector3 pivotPos = LoadedConstructions[i].transform.GetChild (1).position; //pivot
			Vector3 pos = LoadedConstructions[i].transform.GetChild (3).position;//sprites
			//Vector3 pos = selectedBuilding.transform.position;
			
			float correctiony = 10 / (pivotPos.y + 3300);//ex: fg 10 = 0.1   bg 20 = 0.05  
			//all y values must be positive, so we add the grid origin y 3207 +100 to avoid divide by 0; 
			//otherwise depth glitches around y 0
			
			LoadedConstructions[i].transform.GetChild(3).position = new Vector3(pos.x, pos.y, zeroZ - correctiony);
		}
		
	}

}
//1428 lines before refactoring

