using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Messenger : MonoBehaviour {		//displays the status messages on the left side of the screen

	public UILabel UserMessages;
	public GameObject Bk;
	private string userMessagesTxt;
	
	private bool 
		displayMessage, 
		garbleOn = false, 		//flag to stop incoming messages during garble processing
		empOn = false, 			//under effects of EMP
		textOverflow;

	private float displayTimer = 0, idleTimer = 0, removeLineTimer = 0;

	private int addedQueTime = 0;

	private char[] garbledChars = new char[12]{' ', '_', '*', '?', '.', '^', '~', '-', ',','$','#','&'};

	public int 
		singleLineTime = 2,			//2 
		maxQueTime = 20,			//10 messages, then start removing oldest
		idleTime = 8;				//8

	private Component soundFX, bkUiSprite;

	void Start () {
		soundFX = GameObject.Find ("SoundFX").GetComponent<SoundFX> ();
		bkUiSprite = Bk.GetComponent<UISprite> ();
	}

	void Update () {
		if(displayMessage)
		{
			displayTimer += Time.deltaTime;
			idleTimer += Time.deltaTime;

			if(displayTimer>addedQueTime||idleTimer>idleTime)		//idleTimer - we have a large stack of identical messages
			{														//no point waiting 20 seconds
				displayTimer = 0;
				idleTimer = 0;
				ResetUserMessage();
			}

			if(textOverflow)
			{
				removeLineTimer+=Time.deltaTime;
				if(removeLineTimer>=1.0f)
				{
					RemoveLines();
					removeLineTimer = 0;
					displayTimer = 0;
					idleTimer = 0;
				}
			}
		}
	}

	public void EndGarble()
	{
		empOn = false;
	}

	public void GarbleMessage()				//garbles messages after EMP
	{
		empOn = true;
		garbleOn = true;		//prevent other operations on text during this
		string[] lines = userMessagesTxt.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

		string garbledLines = "";

		for (int i = 0; i < lines.Length; i++) 
		{
			char[] chars = lines[i].ToCharArray();

			List<char> charList = new List<char> (); 
			
			for (int j = 0; j < chars.Length; j++) 
			{
				int garb = UnityEngine.Random.Range (0, 4);
				
				if(garb==0)
				{
					int gIndex = UnityEngine.Random.Range(0,11);
					charList.Add(garbledChars[gIndex]);
				}
				else
				{
					charList.Add(chars[j]);
				}
			}

			char[] result = charList.ToArray();

			if(i < lines.Length-1)
				garbledLines += new string(result)+"\n";
			else
				garbledLines += new string(result);
		}

		userMessagesTxt = garbledLines;
		UserMessages.text = userMessagesTxt;
		garbleOn = false;		
	}

	private void CountLines()
	{
		string[] separateLines = userMessagesTxt.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
		addedQueTime = 0;

		for (int i = 0; i < separateLines.Length; i++) 
		{
			addedQueTime += singleLineTime; 
		}
	}

	public void DisplayMessage(string text)
	{
		if (garbleOn) return;	//no new messages during garble

		((SoundFX)soundFX).Move ();

		if (!Bk.activeSelf) { Bk.SetActive (true); }

		addedQueTime += singleLineTime;

		if(addedQueTime > maxQueTime)//in case messages keep coming, start scrolling down
		{
			textOverflow = true;
		}

		userMessagesTxt += text + "\n";

		CountLines ();
		idleTimer = 0;
		displayMessage = true;
		UserMessages.text = userMessagesTxt;

		if(empOn) GarbleMessage();
	}

	private void RemoveLines()
	{
		if(addedQueTime <= maxQueTime){ textOverflow = false; return;}//print ("removing one line. total time: " +addedQueTime.ToString());

		string[] lines = userMessagesTxt.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);//to preserve the last input //{"\r\n", "\n"}
		
		//int linesToSkip = (int)Math.Abs((addedQueTime - maxQueTime)/singleLineTime);			
		//print (linesToSkip.ToString());
		
		userMessagesTxt= "";
		addedQueTime = 0;
		displayTimer = 0;
		
		/*
		By removing just one line, this thing can create a very unusual glitch, where messages start piling up; 
		the reason is that sometimes, removing one line still allows the queue to grow indefinitely, 
		since this function runs not at update, but with each new message that arrives, at considerably longer intervals
		Possible cause- receiving multiple DisplayMessage requests per update cycle from different sources.
		*/

		//linesToSkip
		for (int i = 1; i < lines.Length-1; i++) //start from 1 - discard oldest message; also discard last empty line
		{
			addedQueTime += singleLineTime;
			userMessagesTxt += lines[i] + "\n" ;
		}
		UserMessages.text = userMessagesTxt;
	}

	private void ResetUserMessage()
	{
		((SoundFX)soundFX).End ();			//erase curent message queue
		userMessagesTxt = "";
		displayMessage = false;
		addedQueTime = 0; 
		UserMessages.text = userMessagesTxt;
		Bk.SetActive(false);
	}
}
