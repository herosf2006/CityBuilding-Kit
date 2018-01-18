using UnityEngine;
using System.Collections;

public class GhostHelper : MonoBehaviour {					//the helper assisting the user at first run

	public UILabel LbExplain;								//displayed in the middle of the screen

	public GameObject[] ArrowsMain = new GameObject[6]; 	// ArrowPurchaseUp, ArrowPurchaseRight, ArrowUnits, ArrowOptions, ArrowShop, ArrowCompetition
	public GameObject[] ButtonsMain = new GameObject[6]; 

	public bool[] itemSeen = new bool[6]; 					//purchaseSeen, unitsSeen, optionsSeen, shopSeen, competitionSeen;

	public int showAllCounter = 0;
	private bool writeNew, wait, introRunning = false;

	private float typeWriterDelay = 0.1f, typeWriterCounter = 0;

	private string currentString;
	private char[] fullCharArray;
	private int charCounter;

	private Component relay, stats, soundFX;

	void Start () 
	{
		relay = GameObject.Find ("Relay").GetComponent<Relay> ();
		stats = GameObject.Find ("Stats").GetComponent<Stats> ();
		soundFX = GameObject.Find ("SoundFX").GetComponent<SoundFX> ();

		InvokeRepeating ("CheckMenuReset", 3.0f, 1.0f); 	//the long pause gives the game time to load, see whether the helper was seen and deactivate it
		//InvokeRepeating ("CheckMapActivity", 1.0f, 1.0f);	//user is already playing with buildings underneath
	}

	void FixedUpdate () {

		if(writeNew&&!wait)		//the character by character typewriter 
		{
			typeWriterCounter += Time.deltaTime;
			if(typeWriterCounter > typeWriterDelay)
			{
				if(charCounter < fullCharArray.Length)
				{
					int rand = Random.Range(0,10);
					if(rand < 9)//charCounter%2==0
					((SoundFX)soundFX).TypeWriter();
					currentString += fullCharArray[charCounter];
					charCounter++;
				}
				else
				{
					writeNew = false; wait = true;
					charCounter = 0;
				}
				typeWriterCounter = 0;
			}
			LbExplain.text = currentString;
		}
	}

	private void CheckMenuReset()	//cycles to the next item or disables the helper at the end
	{
		if (showAllCounter == 6)
		{
			((Stats)stats).tutorialCitySeen = true;
			gameObject.SetActive (false);
		}

		if(!((Relay)relay).pauseInput)
		{
			NextArrow ();
			ResetButtons (true);
		}
		else
			ResetHelper();	//the game is in pauseInput  - the user is playing with the buildings underneath
	}

	/*
	private void CheckMapActivity()
	{
		if(((Relay)relay).pauseInput)	
			ResetHelper();		//the game is in pauseInput  - the user is playing with the buildings underneath
	}
	*/
	private void ResetButtons(bool b)
	{
		for (int i = 0; i < ButtonsMain.Length; i++) 
		{
			ButtonsMain[i].SetActive(b);	
		}
	}

	public void ResetHelper()	//user has pressed something, erase everything for now	
	{
		writeNew = false; wait = false; 
		currentString = ""; charCounter = 0;
		LbExplain.text = currentString;
		for (int i = 0; i < ArrowsMain.Length; i++) 
		{
			ArrowsMain[i].SetActive(false);
			ButtonsMain[i].SetActive(false);
		}
	}

	private void NextArrow()
	{
		//ArrowPurchase, ArrowUnits, ArrowOptions, ArrowShop, ArrowCompetition;
		//introSeen, purchaseSeen, unitsSeen, optionsSeen, shopSeen, competitionSeen;

		if (!itemSeen[0])
		{
			if(!((Stats)stats).gameWasLoaded)
				((SoundFX)soundFX).MusicOn(); //the first time the game is run + no user/auto save/load - start the music

			fullCharArray =("Welcome to the strategy kit.\n" +
				"Since this is the first time you are running it,\n" +
				"-or you can press buttons faster than we can count-\n" +
				"we will try to walk you through some of the features.").ToCharArray();
			if(!introRunning && this.gameObject.activeSelf)
			{
				StartCoroutine("EndIntro");
				introRunning = true;
			}
		}
		else if (!itemSeen[1])
		{
			fullCharArray =("Purchase resources\nto speed things up.").ToCharArray();
			ActivateArrow(0);
			ActivateArrow(1);
		}
		else if (!itemSeen[2])
		{
			fullCharArray = ("Keep track of all available units.") .ToCharArray();
			ActivateArrow(2);
		}
		else if (!itemSeen[3])
		{
			fullCharArray = ("Change sound settings,\nsave and load the game.") .ToCharArray();
			ActivateArrow(3);
		}
		else if (!itemSeen[4])
		{
			fullCharArray = ("Purchase new buildings and units.") .ToCharArray();
			ActivateArrow(4);
		}
		else if (!itemSeen[5])
		{
			fullCharArray = ("Once you have enough units,\nyou can launch an attack.") .ToCharArray();
			ActivateArrow(5);
		}
		writeNew = true;
	}

	private void ActivateArrow(int index)
	{
		ArrowsMain [index].SetActive (true);
	}

	private IEnumerator EndIntro()
	{
		yield return new WaitForSeconds (25.0f);
		if(!itemSeen[0])
		{
			showAllCounter++;
			itemSeen[0] = true;
			ResetHelper();
		}
	}

	public void OnPurchase()
	{	
		if(!((Relay)relay).pauseInput && !itemSeen[1])//pauseInput - ignores button press while in pause input - some other panel on screen - the ghost buttons are invisible, but in the same position and receiveing; 
		{											  //itemSeen - don't add to general count if the user presses the same button again	
			showAllCounter++;
			itemSeen[1] = true;
			if(!itemSeen[0])
			{
				StopCoroutine("EndIntro");//to avoid reseting the helper 
				showAllCounter++;
				itemSeen[0] = true; //skips the intro
			}
		}
		ResetHelper();
	}

	public void OnUnits()
	{
		if(!((Relay)relay).pauseInput && !itemSeen[2])
		{
			showAllCounter++;
			itemSeen[2] = true;
			if(!itemSeen[0])
			{
				StopCoroutine("EndIntro");
				showAllCounter++;
				itemSeen[0] = true; //skips the intro
			}
		}
		ResetHelper();
	}

	public void OnOptions()
	{
		if(!((Relay)relay).pauseInput && !itemSeen[3])
		{		
			showAllCounter++;
			itemSeen[3] = true;
			if(!itemSeen[0])
			{
				StopCoroutine("EndIntro");
				showAllCounter++;
				itemSeen[0] = true; //skips the intro
			}
		}
		ResetHelper();
	}

	public void OnShop()
	{
		if(!((Relay)relay).pauseInput && !itemSeen[4])
		{
			showAllCounter++;
			itemSeen[4] = true;
			if(!itemSeen[0])
			{
				StopCoroutine("EndIntro");
				showAllCounter++;
				itemSeen[0] = true; //skips the intro
			}
		}	
		ResetHelper();
	}

	public void OnCompetition()
	{
		if(!((Relay)relay).pauseInput  && !itemSeen[5])
		{
			showAllCounter++;
			itemSeen[5] = true;
			if(!itemSeen[0])
			{
				StopCoroutine("EndIntro");
				showAllCounter++;
				itemSeen[0] = true; //skips the intro
			}
		}
		ResetHelper();
	}

}
