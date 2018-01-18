using UnityEngine;
using System.Collections;

public class GhostHelperBattle : MonoBehaviour {			//the helper assisting the user at first attack on a foreign map

	public UILabel LbExplain;

	public GameObject[] 
		UnitGroupBt = new GameObject[4],					//to cycle the unit group buttons
		ArrowsMain = new GameObject[2], 					// ArrowOptions, ArrowUnits
		Stars = new GameObject[4]; 							// ArrowOptions, ArrowUnits

	private bool cycleBt = false, zoomOut = false, starsCreated = false, introRunning = false;	//used to cycle between unit group buttons

	private float cycleBtCounter, cycleBtTime = 1.0f;
	private int currentBtIndex=-1, starZ=-10;

	public bool[] itemSeen = new bool[3]; 					//introSeen, purchaseSeen, unitsSeen, optionsSeen, shopSeen, competitionSeen;
	public int showAllCounter = 0;

	private bool writeNew, wait;
	private float typeWriterDelay = 0.1f, typeWriterCounter = 0;

	private string currentString;
	private char[] fullCharArray;
	private int charCounter;

	private Component relay, soundFX, statsBattle, tk2DCamera ;//transData, 

	void Start () 
	{
		relay = GameObject.Find ("Relay").GetComponent<Relay> ();
		soundFX = GameObject.Find ("SoundFX").GetComponent<SoundFX> ();
		//transData = GameObject.Find ("TransData").GetComponent<TransData> ();
		statsBattle = GameObject.Find ("StatsBattle").GetComponent<StatsBattle> ();
		tk2DCamera = GameObject.Find ("tk2dCamera").GetComponent<tk2dCamera> ();
	
		InvokeRepeating ("CheckMenuReset", 5f, 5f);
	}

	void FixedUpdate () {

		if(writeNew&&!wait)
		{
			typeWriterCounter += Time.deltaTime;
			if(typeWriterCounter > typeWriterDelay)
			{
				if(charCounter < fullCharArray.Length)
				{
					int rand = Random.Range(0,10);
					if(rand < 9)//charCounter%2==0//skips some of the sounds for variation
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
		if(cycleBt)
		{
			cycleBtCounter+=Time.deltaTime;

			if(cycleBtCounter>cycleBtTime)
			{
				cycleBtCounter = 0;
				ResetGroupBt();

				if(currentBtIndex < UnitGroupBt.Length-1)
					currentBtIndex++;
				else
					currentBtIndex = 0;

				ActivateButton(currentBtIndex);
			}

		}
		if(zoomOut)
		{
			if(((tk2dCamera) tk2DCamera).ZoomFactor>0.2f) 
				((tk2dCamera) tk2DCamera).ZoomFactor-=0.003f;
			
			else if(!starsCreated)
				CreateStars();
		}
	}

	private void CreateStars() //surounds the map with stars on the edges
	{
		int rows = GridManager.instance.numOfRows;
		int cols = GridManager.instance.numOfColumns;
		Vector2 gridPos, starPos;

		for (int i = 1; i < rows-1; i++) //skip first / last positions - the corners
		{
			for (int j = 1; j < cols-1; j++) 
			{
				if(i==1||i==34||j==1||j==34)
				{
					gridPos = GridManager.instance.nodes[i,j].position;
					starPos = new Vector3(gridPos.x,gridPos.y,starZ);
					InitiateStar(gridPos);
				}
			}
		}
		starsCreated = true;
	}

	private void KillStars() //send all stars the message to fadeout
	{
		GameObject[] allStars=GameObject.FindGameObjectsWithTag("Star");

		for (int i = 0; i < allStars.Length; i++) 
		{
			if(allStars[i] == null) return;
			(allStars[i].GetComponent<Star>()).die = true;
			(allStars[i].GetComponent<Selector>()).isSelected = false;//interferes with unit deployment stars
		}	
	}

	private void InitiateStar(Vector2 gridPos)
	{
		int starType = Random.Range (0, 3);
		GameObject Star = (GameObject)Instantiate (Stars[starType], new Vector3(gridPos.x,gridPos.y,starZ), Quaternion.identity);
	}

	private void ResetGroupBt()
	{
		for (int i = 0; i < UnitGroupBt.Length; i++) 
		{
			UnitGroupBt[i].SetActive(false);
		}
	}
	private void ActivateButton(int index)
	{
		UnitGroupBt [index].SetActive (true);
	}

	private void CheckMenuReset()
	{
		if (showAllCounter == 3)
		{
			((StatsBattle)statsBattle).tutorialBattleSeen = true;
			gameObject.SetActive (false);
		}

		if(!((Relay)relay).pauseInput)
		NextArrow ();
	}

	public void ResetHelper()
	{
		writeNew = false; wait = false; 
		currentString = ""; charCounter = 0;
		LbExplain.text = currentString;
		for (int i = 0; i < ArrowsMain.Length; i++) 
		{
			ArrowsMain[i].SetActive(false);
		}
	}

	private void NextArrow()
	{
		//ArrowPurchase, ArrowUnits, ArrowOptions, ArrowShop, ArrowCompetition;
		//purchaseSeen, unitsSeen, optionsSeen, shopSeen, competitionSeen;

		if (!itemSeen[0])
		{
			fullCharArray =("Just a second to zoom out\n" +
							"so you can see the entire map.").ToCharArray();

			if(!introRunning)			// introRunning makes sure the coroutine is started only once
			{
				StartCoroutine("EndIntro");
				introRunning = true;
			}
			zoomOut = true;
		}

		else if (!itemSeen[1])
		{
			fullCharArray =("Access the units panel\n" +
							"and select the outer edges of the map.\n" +
							"You can deploy and control up to 4 separate squads.\n" +
							"Select a building, and the nearest squad will attack it.").ToCharArray();
			ActivateArrow(0);
			cycleBt = true; zoomOut = true;
		}
		else if (!itemSeen[2])
		{
			fullCharArray = ("Sound options,\nend battle or exit game.") .ToCharArray();
			ActivateArrow(1);
			cycleBt = false;
		}

		writeNew = true;
	}

	private void ActivateArrow(int index)
	{
		ArrowsMain [index].SetActive (true);
	}

	private IEnumerator EndIntro()
	{
		yield return new WaitForSeconds (10.0f);

		if(!itemSeen[0])
		{
			showAllCounter++;
			itemSeen[0] = true;
			ResetHelper();
		}
	}

	public void OnUnits()
	{
		if(!((Relay)relay).pauseInput && !itemSeen[1])
		{
			KillStars();
			cycleBt = false; zoomOut = false; 
			ResetGroupBt();
			showAllCounter++;

			if(!itemSeen[0])
			{
				StopCoroutine("EndIntro");
				showAllCounter++;
				itemSeen[0] = true; //skips the intro
			}

			itemSeen[1] = true;
		}
		ResetHelper();
	}

	public void OnOptions()
	{
		if(!((Relay)relay).pauseInput && !itemSeen[2])
		{	
			showAllCounter++;

			if(!itemSeen[0])
			{
				StopCoroutine("EndIntro");
				showAllCounter++;
				itemSeen[0] = true; //skips the intro
			}

			itemSeen[2] = true;
		}
		ResetHelper();
		cycleBt = false;
	}

}
