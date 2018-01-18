using UnityEngine;
using System.Xml;
using System.IO;
using System.Collections;
using System.Text;

public class SaveLoadWWW : MonoBehaviour
{
	private string 
		filePath,
		fileExt = ".txt",
		attackExt = "_results",				//_attack

		fileNameLocal = "LocalSave",		//local - the full game save
		fileNameServer = "ServerSave",		//local - the full game save downloaded from server
		fileNameAttack = "ServerAttack",
		fileNameMapID = "MyMapID";			//local - saves the name of the map from server

	public string myMapIDCode, 
	initialMapIDCode = "000InitMap";						//example: gfdfghjke

	public string 
		serverAddress = "http://citybuildingkit.com/",
		filesAddress = "get_match.php",
		matchAddress = "finish_match.php",
		license = "U125UNITYDEMO";

	//PlayerPrefs save variables
	string currentLine;
	//private int lineIndex = 0;


	private Component saveLoadMap, statusMsg, stats; 
	private bool saving = false;

	void Start () 
	{		
		//filePath = Application.dataPath + "/";//windows - same folder as the project
		filePath = Application.persistentDataPath +"/";//other platforms

		//#if !UNITY_WEBPLAYER	
		print ("File path on your system is: " + filePath.ToString ());
		//#endif

		//#if UNITY_WEBPLAYER	
		print ("PlayerPrefs are stored in HKEY_CURRENT_USER/SOFTWARE/StartupKits/StrategyKit (Run regedit)");
		//#endif

		saveLoadMap = GameObject.Find ("SaveLoadMap").GetComponent<SaveLoadMap> ();
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();
		stats = GameObject.Find("Stats").GetComponent<Stats>();

		//StartCoroutine ("ServerAutoLoad");	//automatic server load at startup - prevents user from building and then loading on top

		//if (!File.Exists (filePath + fileNameLocal + fileExt)) //load initial map if local save not found
			//StartCoroutine ("InitialMapAutoLoad");
	//	else
			//StartCoroutine ("LateDisplayLoadMessage");
	}
	private IEnumerator LateDisplayLoadMessage()
	{
		yield return new WaitForSeconds (2);
		((Messenger)statusMsg).DisplayMessage("You already saved locally. Load manually or auto local/server.");
	}
	private IEnumerator ServerAutoLoad()
	{
		yield return new WaitForSeconds (2);
		LoadFromServer ();
	}
	private IEnumerator InitialMapAutoLoad()
	{
		yield return new WaitForSeconds (2);
		LoadInitialMapFromServer ();
	}
	private bool CheckLocalSaveFile()
	{
		if(!saving)
			((Messenger)statusMsg).DisplayMessage("Checking for local save file...");

		bool localSaveExists = File.Exists(filePath + fileNameLocal + fileExt);

		if(!localSaveExists) 
			((Messenger)statusMsg).DisplayMessage("Local save file not found. Save locally first.");

		return(localSaveExists);
	}

	private bool CheckPlayerPrefsLocalSaveFile()
	{
		if(!saving)		
			((Messenger)statusMsg).DisplayMessage("Checking for PlayerPrefs save file...");

		bool localSaveExists =  PlayerPrefs.HasKey("savefile");
			
		if(!localSaveExists) 
			((Messenger)statusMsg).DisplayMessage("PlayerPrefs save file not found. Save PlayerPrefs first.");
		return(localSaveExists);
	}

	private bool CheckServerSaveFile()//local recording of a previous save ID on server
	{
		((Messenger)statusMsg).DisplayMessage("Checking for map ID...");//local - result of server save

		bool serverSaveExists = File.Exists(filePath + fileNameMapID + fileExt);

		if(!serverSaveExists) 
			((Messenger)statusMsg).DisplayMessage("You have no map ID."); //no loca ID - no server save file


		return(serverSaveExists);
		//checks if the mapcode was saved locally, not if it is still available on server
	}

	private bool CheckServerPlayerPrefsSaveFile()//local recording of a previous save ID on server
	{
		((Messenger)statusMsg).DisplayMessage("Checking for PlayerPrefs map ID...");//local - result of server save
		
		bool serverSaveExists = PlayerPrefs.HasKey("mapid");
		
		if(!serverSaveExists) 
			((Messenger)statusMsg).DisplayMessage("You have no PlayerPrefs map ID."); //no loca ID - no server save file
		
		
		return(serverSaveExists);
		//checks if the mapcode was saved locally, not if it is still available on server
	}

	private void GenerateMapid()
	{
		//generate a long random file name , to avoid duplicates and overwriting	
		string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
		char[] stringChars = new char[10];
				
		for (int i = 0; i < stringChars.Length; i++)
		{
			stringChars[i] = chars[Random.Range(0, chars.Length)];
		}
		
		myMapIDCode = new string(stringChars);	
		((Messenger)statusMsg).DisplayMessage("New map ID generated: "+ myMapIDCode);
	}
	
	public void SaveMapid()
	{
		//the user has to retrieve his own file, in case someone has attacked his city
		StreamWriter sWriter = new StreamWriter (filePath + fileNameMapID + fileExt);
		sWriter.WriteLine (myMapIDCode);
		sWriter.Flush ();
		sWriter.Close ();
		((Messenger)statusMsg).DisplayMessage("New map ID saved: "+ myMapIDCode);
	}

	public void SavePlayerPrefsMapid()
	{
		//the user has to retrieve his own file, in case someone has attacked his city
		PlayerPrefs.SetString("mapid", myMapIDCode);
		PlayerPrefs.Save ();
		((Messenger)statusMsg).DisplayMessage("New PlayerPrefs map ID saved: "+ myMapIDCode);
	}

	public void ReadMapid()
	{
		StreamReader sReader = new StreamReader(filePath + fileNameMapID + fileExt);
		myMapIDCode = sReader.ReadLine();
		((Messenger)statusMsg).DisplayMessage("Retrieved map ID: " + myMapIDCode);
	}

	public void ReadPlayerPrefsMapid()
	{
		myMapIDCode = PlayerPrefs.GetString("mapid");
		((Messenger)statusMsg).DisplayMessage("Retrieved PlayerPrefs map ID: " + myMapIDCode);
	}

	public void SaveToServer()
	{
		((Stats)stats).gameWasLoaded = true;//saving the game also prevents the user from loading on top

		//can upload to server only if the game was previously saved locally
		if(CheckLocalSaveFile() && !saving)
		{
			saving = true;
			StartCoroutine("UploadLevel");//force the local map save before this
			((Messenger)statusMsg).DisplayMessage("Uploading to server...");
		}
		else if (saving) 
		{
			((Messenger)statusMsg).DisplayMessage("Upload in progress. Please wait...");
		}
	}

	public void SavePlayerPrefsToServer()
	{
		((Stats)stats).gameWasLoaded = true;//saving the game also prevents the user from loading on top
		
		//can upload to server only if the game was previously saved locally
		if(CheckPlayerPrefsLocalSaveFile() && !saving)
		{
			saving = true;
			StartCoroutine("UploadPlayerPrefsLevel");//force the local map save before this
			((Messenger)statusMsg).DisplayMessage("Uploading PlayerPrefs to server...");
		}
		else if (saving) 
		{
			((Messenger)statusMsg).DisplayMessage("Upload in progress. Please wait...");
		}
	}

	private byte[] ReadAllBytes(string fileName)
	{
		byte[] buffer = null;
		using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
		{
			buffer = new byte[fs.Length];
			fs.Read(buffer, 0, (int)fs.Length);
		}
		return buffer;
	} 


	IEnumerator UploadPlayerPrefsLevel()   
	{
		//#if !UNITY_WEBPLAYER
		//stuff that isn't supported in the web player
		//#endif

		//savefile = PlayerPrefs.GetString("savefile");

		byte[] levelData =  Encoding.ASCII.GetBytes(PlayerPrefs.GetString("savefile"));

		//byte[] levelData = System.IO.File.ReadAllBytes(filePath + fileNameLocal + fileExt);//full local save file
		//byte[] levelData = Encoding.UTF8.GetBytes(filePath + fileNameLocal + fileExt);
		
		bool serverSaveExists = false;
		if(CheckServerPlayerPrefsSaveFile())
		{
			serverSaveExists = true;
			ReadPlayerPrefsMapid();
		}
		else
		{
			GenerateMapid();
			//SavePlayerPrefsMapid();//shortcut for the web browser not manipulating well the file check sequence below
		}
		
		/*
		- you save the generated ID, then retrieve the uploaded file, without the needs of listings
		- the same method is used to retrieve and validate any uploaded file
		- this method is similar to the one used by the games like Bike Baron
		- saves you from the hassle of making complex server side back ends which enlists available levels
		- you could enlist outstanding levels just by posting the level code on a server 
		- easier to share, without the need of user accounts or install procedures
		*/
		
		WWWForm form = new WWWForm();
		
		form.AddField("savefile","file");
		form.AddBinaryData( "savefile", levelData, myMapIDCode,"text/xml");//file
		
		//change the url to the url of the php file
		WWW w = new WWW(serverAddress + filesAddress +"?mapid=" + myMapIDCode+"&license="+license, form);//myUseridFile 
		
		yield return w;
		
		if (w.error != null)
		{
			((Messenger)statusMsg).DisplayMessage("File upload error.");
			print ( w.error );    
		}
		else
		{
			//this part validates the upload, by waiting 5 seconds then trying to retrieve it from the web
			//http://docs.unity3d.com/ScriptReference/WWW-uploadProgress.html

			/*
			    w.uploadProgress 
			  	public float uploadProgress; 
			  
				How far has the upload progressed (Read Only).
				This is a value between zero and one; 0 means nothing is sent yet, 1 means upload complete.

				IMPORTANT:
				uploadProgress is currently not fully implemented in the Web Player. If used in a Web Player it will report 0.5 during the upload and 1.0 when the upload is complete.
				Since all sending of data to the server is done before receiving data, uploadProgress will always be 1.0 when progress is larger than 0.


				w.isDone
				public bool isDone; 

				Is the download already finished? (Read Only)
				You should not write loops that spin until download is done; use coroutines instead. An empty loop that waits for isDone will block in the web player.

			 */

			if(w.isDone)//(w.uploadProgress == 1 && w.isDone)  See above problem with this check
			{
				yield return new WaitForSeconds(5);
				//change the url to the url of the folder you want it the levels to be stored, the one you specified in the php file
				WWW w2 = new WWW(serverAddress + filesAddress + "?get_user_map=1&mapid=" + myMapIDCode+"&license="+license);//returns a specific map
				
				yield return w2;
				
				if(w2.error != null)
				{
					((Messenger)statusMsg).DisplayMessage("Server file check error.");
					print ( w2.error );  
				}
				else
				{
					//then if the retrieval was successful, validate its content to ensure the map file integrity is intact
					if(w2.text != null && w2.text != "")
					{
						if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
						{
							//and finally announce that everything went well
							print ( "Received verification file map ID " + myMapIDCode + " contents are: \n\n" + w2.text);
							
							if(!serverSaveExists) 
							{
								SavePlayerPrefsMapid();// for some reason, the web server doesn't reach this point in the verification
							}
							
							((Messenger)statusMsg).DisplayMessage("Server PlayerPrefs upload complete.");
						}
						else
						{
							print ( "Map file " + myMapIDCode + " is invalid");
							print ( "Received verification file " + myMapIDCode + " contents are: \n\n" + w2.text);//although file is broken, prints the content of the retrieved file
							
							((Messenger)statusMsg).DisplayMessage("Server PlayerPrefs upload incomplete. Try again.");
						}
					}
					else
					{
						print ( "Map file " + myMapIDCode + " is empty");
						((Messenger)statusMsg).DisplayMessage("Server upload check failed.");
					}
				}
				
				
			}     
		}
		saving = false; //even if failed, user can try to save again
	}
	IEnumerator UploadLevel()   
	{
		//#if UNITY_WEBPLAYER	
		//#endif
		//#if !UNITY_WEBPLAYER	
		//#endif

		byte[] levelData = ReadAllBytes(filePath + fileNameLocal + fileExt);

		//byte[] levelData = System.IO.File.ReadAllBytes(filePath + fileNameLocal + fileExt);
		//byte[] levelData = Encoding.ASCII.GetBytes(filePath + fileNameLocal + fileExt);
		//byte[] levelData = Encoding.UTF8.GetBytes(filePath + fileNameLocal + fileExt);

		bool serverSaveExists = false;
		if(CheckServerSaveFile())
		{
			serverSaveExists = true;
			ReadMapid();
		}
		else
		{
			GenerateMapid();
		}

		/*
		- you save the generated ID, then retrieve the uploaded file, without the needs of listings
		- the same method is used to retrieve and validate any uploaded file
		- this method is similar to the one used by the games like Bike Baron
		- saves you from the hassle of making complex server side back ends which enlists available levels
		- you could enlist outstanding levels just by posting the level code on a server 
		- easier to share, without the need of user accounts or install procedures
		*/

		WWWForm form = new WWWForm();
						
		form.AddField("savefile","file");
		form.AddBinaryData( "savefile", levelData, myMapIDCode,"text/xml");//file
			
		//change the url to the url of the php file
		WWW w = new WWW(serverAddress + filesAddress +"?mapid=" + myMapIDCode+"&license="+license, form);//myUseridFile 

		yield return w;

		if (w.error != null)
		{
			((Messenger)statusMsg).DisplayMessage("File upload error.");
			print ( w.error );    
		}
		else
		{
			//this part validates the upload, by waiting 5 seconds then trying to retrieve it from the web
			if(w.uploadProgress == 1 && w.isDone)
			{
				yield return new WaitForSeconds(5);
				//change the url to the url of the folder you want it the levels to be stored, the one you specified in the php file
				WWW w2 = new WWW(serverAddress + filesAddress + "?get_user_map=1&mapid=" + myMapIDCode+"&license="+license);//returns a specific map
								
				yield return w2;

				if(w2.error != null)
				{
					((Messenger)statusMsg).DisplayMessage("Server file check error.");
					print ( w2.error );  
				}
				else
				{
					//then if the retrieval was successful, validate its content to ensure the map file integrity is intact
					if(w2.text != null && w2.text != "")
					{
						if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
						{
							//and finally announce that everything went well
							print ( "Received verification file map ID " + myMapIDCode + " contents are: \n\n" + w2.text);

							if(!serverSaveExists) SaveMapid();

							((Messenger)statusMsg).DisplayMessage("Server upload complete.");
						}
						else
						{
							print ( "Map file " + myMapIDCode + " is invalid");
							print ( "Received verification file " + myMapIDCode + " contents are: \n\n" + w2.text);//although file is broken, prints the content of the retrieved file

							((Messenger)statusMsg).DisplayMessage("Server upload incomplete. Try again.");
						}
					}
					else
					{
						print ( "Map file " + myMapIDCode + " is empty");
						((Messenger)statusMsg).DisplayMessage("Server upload check failed.");
					}
				}

			
			}     
		}
		saving = false; //even if failed, user can try to save again
	}
	public void LoadPlayerPrefsFromServer()//this sequence will also apply the adjustments if the map was attacked
	{
		if(((Stats)stats).gameWasLoaded) //prevents loading twice, since there are no safeties and the procedure should be automated at startup, not button triggered
		{
			((Messenger)statusMsg).DisplayMessage("Only one load per session is allowed. Canceling...");
			return;
		}
		
		if(CheckServerPlayerPrefsSaveFile())		
		{
			StartCoroutine("DownloadMyPlayerPrefsMap");//force the local map save before this
			((Messenger)statusMsg).DisplayMessage("Downoading map from server...");
		}			
	}
	public void LoadFromServer()//this sequence will also apply the adjustments if the map was attacked
	{
		if(((Stats)stats).gameWasLoaded) //prevents loading twice, since there are no safeties and the procedure should be automated at startup, not button triggered
		{
			((Messenger)statusMsg).DisplayMessage("Only one load per session is allowed. Canceling...");
			return;
		}

		if(CheckServerSaveFile())		
		{
			StartCoroutine("DownloadMyMap");//force the local map save before this
			((Messenger)statusMsg).DisplayMessage("Downoading map from server...");
		}			
	}
	public void LoadInitialMapFromServer()//this sequence will also apply the adjustments if the map was attacked
	{
		if(((Stats)stats).gameWasLoaded) //prevents loading twice, since there are no safeties and the procedure should be automated at startup, not button triggered
		{
			((Messenger)statusMsg).DisplayMessage("Only one load per session is allowed. Canceling...");
			return;
		}

		StartCoroutine("DownloadInitialMap");//force the local map save before this
		((Messenger)statusMsg).DisplayMessage("Downoading initial map from server...");
				
	}
	IEnumerator DownloadMyPlayerPrefsMap()   
	{
		((Messenger)statusMsg).DisplayMessage("Loading map from server...");
		ReadPlayerPrefsMapid ();

		WWWForm form = new WWWForm();		
		form.AddField("savefile","file");
		
		WWW w2 = new WWW(serverAddress + matchAddress + "?get_user_map=1&mapid=" + myMapIDCode+"&license="+license);
		
		yield return w2;
		
		if(w2.error != null)
		{
			((Messenger)statusMsg).DisplayMessage("Server load failed.");
			print("Server load error" + w2.error);
		}
		
		else
		{
			//then if the retrieval was successful, validate its content to ensure the map file integrity is intact
			if(w2.text != null && w2.text != "")
			{
				if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
				{
					print ( "Your map ID " + myMapIDCode + " contents are: \n\n" + w2.text);	
					
					WriteMyPlayerPrefsMapFromServer(w2.text);
					
					((SaveLoadMap)saveLoadMap).LoadFromPlayerPrefs();
					((Messenger)statusMsg).DisplayMessage("Map downloaded and saved locally.");
				}
				
				else
				{			
					print ( "Your map file is Invalid. Contents are: \n\n" + w2.text);
					//although file is broken, prints the content of the retrieved file
					((Messenger)statusMsg).DisplayMessage("Server dowload incomplete. Try again.");
				}
			}
			else
			{
				print ( "Your map file is empty");
				((Messenger)statusMsg).DisplayMessage("Server dowload failed. Empty map.");
			}
		}
	}

	IEnumerator DownloadMyMap()   
	{
		((Messenger)statusMsg).DisplayMessage("Loading map from server...");
		ReadMapid();

		WWWForm form = new WWWForm();		
		form.AddField("savefile","file");

		WWW w2 = new WWW(serverAddress + matchAddress + "?get_user_map=1&mapid=" + myMapIDCode+"&license="+license);
		
		yield return w2;
		
		if(w2.error != null)
		{
			((Messenger)statusMsg).DisplayMessage("Server load failed.");
			print("Server load error" + w2.error);
		}
		
		else
		{
			//then if the retrieval was successful, validate its content to ensure the map file integrity is intact
			if(w2.text != null && w2.text != "")
			{
				if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
				{
					print ( "Your map ID " + myMapIDCode + " contents are: \n\n" + w2.text);	

					WriteMyMapFromServer(w2.text);

					((SaveLoadMap)saveLoadMap).LoadFromServer();
					//((SaveLoadMap)saveLoadMap).LoadAttackFromServer ();
					((Messenger)statusMsg).DisplayMessage("Map downloaded and saved locally.");
				}
				
				else
				{			
					print ( "Your map file is Invalid. Contents are: \n\n" + w2.text);
					//although file is broken, prints the content of the retrieved file
					((Messenger)statusMsg).DisplayMessage("Server dowload incomplete. Try again.");
				}
			}
			else
			{
				print ( "Your map file is empty");
				((Messenger)statusMsg).DisplayMessage("Server dowload failed. Empty map.");
			}
		}
	}

	IEnumerator DownloadInitialMap()   
	{
		((Messenger)statusMsg).DisplayMessage("Loading initial map from server...");

		WWW w2 = new WWW(serverAddress + matchAddress + "?get_user_map=1&mapid=" + initialMapIDCode +"&license="+license);

		yield return w2;

		if(w2.error != null)
		{
			((Messenger)statusMsg).DisplayMessage("Server load failed.");
			print("Server load error" + w2.error);
		}

		else
		{
			//then if the retrieval was successful, validate its content to ensure the map file integrity is intact
			if(w2.text != null && w2.text != "")
			{
				if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
				{
					print ( "Your map ID " + initialMapIDCode + " contents are: \n\n" + w2.text);	

					WriteInitialMapFromServer(w2.text);

					((SaveLoadMap)saveLoadMap).LoadFromServer();
					//((SaveLoadMap)saveLoadMap).LoadAttackFromServer ();
					((Messenger)statusMsg).DisplayMessage("Map downloaded and saved locally.");
				}

				else
				{			
					print ( "Your map file is Invalid. Contents are: \n\n" + w2.text);
					//although file is broken, prints the content of the retrieved file
					((Messenger)statusMsg).DisplayMessage("Server dowload incomplete. Try again.");
				}
			}
			else
			{
				print ( "Your map file is empty");
				((Messenger)statusMsg).DisplayMessage("Server dowload failed. Empty map.");
			}
		}
	}


	private void WriteMyPlayerPrefsMapFromServer(string text)//saves a copy of the server map locally
	{
		PlayerPrefs.SetString("savefile", text);
		PlayerPrefs.Save ();
		StartCoroutine("DownloadMyPlayerPrefsMapAttack");
		((Messenger)statusMsg).DisplayMessage("Downloading attack results...");
	}

	private void WriteMyMapFromServer(string text)//saves a copy of the server map locally
	{
		StreamWriter sWriter = new StreamWriter (filePath + fileNameServer + fileExt);
		sWriter.Write(text);
		sWriter.Flush ();
		sWriter.Close ();
		StartCoroutine("DownloadMyMapAttack");
		((Messenger)statusMsg).DisplayMessage("Downloading attack results...");
	}
	private void WriteInitialMapFromServer(string text)//saves a copy of the server map locally
	{
		StreamWriter sWriter = new StreamWriter (filePath + fileNameServer + fileExt);
		sWriter.Write(text);
		sWriter.Flush ();
		sWriter.Close ();
	}

	IEnumerator DownloadMyPlayerPrefsMapAttack()   
	{		
		WWWForm form = new WWWForm();		
		form.AddField("savefile","file");
		
		WWW w2 = new WWW(serverAddress + matchAddress + "?get_user_map=1&mapid=" + myMapIDCode + attackExt+"&license="+license);
		
		yield return w2;
		
		if(w2.error != null)
		{
			((Messenger)statusMsg).DisplayMessage("Attack file download failed.");
			print("Server load error" + w2.error);
		}
		
		else
		{
			//then if the retrieval was successful, validate its content to ensure the map file integrity is intact
			if(w2.text != null && w2.text != "")
			{
				if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
				{
					print ( "Your map ID "+ myMapIDCode + attackExt +" contents are: \n\n" + w2.text);	
					
					WriteMyPlayerPrefsMapAttackFromServer(w2.text);
					
					((SaveLoadMap)saveLoadMap).LoadPlayerPrefsAttackFromServer();
					StartCoroutine("EraseAttackFromServer");
					((Messenger)statusMsg).DisplayMessage("Attack results downloaded.");
				}
				
				else
				{			
					print ( "Your map file is invalid. Contents are: \n\n" + w2.text);
					//although file is broken, prints the content of the retrieved file
					((Messenger)statusMsg).DisplayMessage("Attack results corrupted. Download failed.");
				}
			}
			else
			{
				print ( "Attack file is empty");
				((Messenger)statusMsg).DisplayMessage("Your town was not attacked.");
				
			}
		}
	}

	IEnumerator DownloadMyMapAttack()   
	{		
		WWWForm form = new WWWForm();		
		form.AddField("savefile","file");
		
		WWW w2 = new WWW(serverAddress + matchAddress + "?get_user_map=1&mapid=" + myMapIDCode + attackExt+"&license="+license);
		
		yield return w2;
		
		if(w2.error != null)
		{
			((Messenger)statusMsg).DisplayMessage("Attack file download failed.");
			print("Server load error" + w2.error);
		}
		
		else
		{
			//then if the retrieval was successful, validate its content to ensure the map file integrity is intact
			if(w2.text != null && w2.text != "")
			{
				if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
				{
					print ( "Your map ID "+ myMapIDCode + attackExt +" contents are: \n\n" + w2.text);	
					
					WriteMyMapAttackFromServer(w2.text);					
					((SaveLoadMap)saveLoadMap).LoadAttackFromServer();
					StartCoroutine("EraseAttackFromServer");
					((Messenger)statusMsg).DisplayMessage("Attack results downloaded.");
				}
				
				else
				{			
					print ( "Your map file is invalid. Contents are: \n\n" + w2.text);
					//although file is broken, prints the content of the retrieved file
					((Messenger)statusMsg).DisplayMessage("Attack results corrupted. Download failed.");
				}
			}
			else
			{
				print ( "Attack file is empty");
				((Messenger)statusMsg).DisplayMessage("Your town was not attacked.");

			}
		}
	}

	IEnumerator EraseAttackFromServer()   
	{
		((Messenger)statusMsg).DisplayMessage("Erasing attack on server...");
		//byte[] levelData = System.IO.File.ReadAllBytes(filePath + battleResultSaveFile + fileExt);//full local save file
		byte[] levelData = Encoding.ASCII.GetBytes("###StartofFile###\n" 
		                                           + "0" + "," 
		                                           + "0" + "," 
		                                           + "0"+ "," 
		                                           + "0" + 
		                                           "\n###EndofFile###");
		
		WWWForm form = new WWWForm();

		form.AddField("savefile","file");
		
		form.AddBinaryData( "savefile", levelData, myMapIDCode + attackExt,"text/xml");//file
				
		//change the url to the url of the php file
		WWW w = new WWW(serverAddress + filesAddress +"?mapid=" + myMapIDCode + attackExt+"&license="+license, form);//myUseridFile 

		yield return w;
	
		if (w.error != null)
		{
			((Messenger)statusMsg).DisplayMessage("Attack erase failed.");
			print ( w.error );    
		}
		else
		{
			//this part validates the upload, by waiting 5 seconds then trying to retrieve it from the web
			if(w.uploadProgress == 1 && w.isDone)
			{				
				yield return new WaitForSeconds(5);
				//change the url to the url of the folder you want it the levels to be stored, the one you specified in the php file
				WWW w2 = new WWW(serverAddress + filesAddress+"?get_user_map=1&mapid=" + myMapIDCode + attackExt+"&license="+license);//returns a specific map

				yield return w2;
				if(w2.error != null)
				{
					((Messenger)statusMsg).DisplayMessage("Attack erase check failed.");				
					print(w2.ToString());
				}
				else
				{
					//then if the retrieval was successful, validate its content to ensure the map file integrity is intact
					if(w2.text != null && w2.text != "")
					{
						if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
						{
							//and finally announce that everything went well
							print ( "Received verification file " + myMapIDCode + attackExt + " contents are: \n\n" + w2.text);

							((Messenger)statusMsg).DisplayMessage("Attack file from server\nhas been loaded and reset.");
						}
						else
						{
							print ( "Map file " + myMapIDCode + attackExt + " is invalid");//file
							print ( "Received verification file " + myMapIDCode + attackExt + " contents are: \n\n" + w2.text);//file although incorrect, prints the content of the retrieved file
							((Messenger)statusMsg).DisplayMessage("Attack file reset on server\nhas failed.");
						}
					}
					else
					{
						print ( "Map File " + myMapIDCode + attackExt + " is Empty");//file
						((Messenger)statusMsg).DisplayMessage("Attack file may be empty.");
					}
				}
			}     
		}
	}

	private void WriteMyPlayerPrefsMapAttackFromServer(string text)//saves a copy of the server map locally
	{
		PlayerPrefs.SetString(fileNameAttack, text);
		PlayerPrefs.Save ();
	}

	private void WriteMyMapAttackFromServer(string text)//saves a copy of the server map locally
	{
		StreamWriter sWriter = new StreamWriter (filePath + fileNameAttack + fileExt);
		sWriter.Write(text);
		sWriter.Flush ();
		sWriter.Close ();
	}

	public void LoadRandomFromServer()
	{
		StartCoroutine("DownloadRandomMap");//force the local map save before this
		((Messenger)statusMsg).DisplayMessage("Downloading random map...");
	}

	IEnumerator DownloadRandomMap()   
	{
		#if !UNITY_WEBPLAYER
		ReadMapid();
		#endif
		
		#if UNITY_WEBPLAYER
		ReadPlayerPrefsMapid();
		#endif

		WWW w2 = new WWW(serverAddress + filesAddress + "?get_random_map=1&mapid=" + myMapIDCode+"&license="+license); //ADAN FIXED 5/22 because you need to include the user's mapid with the get_random_map to prevent the user's map from being downloaded by accident
			
		yield return w2;

		if(w2.error != null)
		{
			((Messenger)statusMsg).DisplayMessage("Random map download failed.");
			print("Server load error" + w2.error);
		}

		else
		{
			//then if the retrieval was successful, validate its content to ensure the map file integrity is intact
			if(w2.text != null && w2.text != "")
			{
				if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
				{
					print ( "Random file contents are: \n\n" + w2.text);
					((Messenger)statusMsg).DisplayMessage("Random map downloaded successfully");
				}

				else
				{			
					print ( "Random map file is invalid. Contents are: \n\n" + w2.text);
					//although incorrect, prints the content of the retrieved file
					((Messenger)statusMsg).DisplayMessage("Random map download failed/incomplete.");
				}
			}
			else
			{
				print ( "Random map file is empty");
				((Messenger)statusMsg).DisplayMessage("Downloaded random map is empty.");
			}
		}
	}

	/*
	void OnGUI()//to test any function separately, skipping the normal command flow
	{
		if(GUILayout.Button("Click me!"))
		{
			SaveToServer();
		}
	}
	*/
}
