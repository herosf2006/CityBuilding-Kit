using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Xml;
using System.IO;
using System.Text;

//This script is active while the units menu is enabled on screen, then, the relevant info is passed to the unitProc

public class MenuUnit : MenuUnitBase {
				
	//Labels
	public UILabel HintLabel, TimeLabel, FinishPriceLb;
	private int priceInCrystals = 0;

	private bool resetLabels = false;//the finish now upper right labels 
	public GameObject UnitProcObj; //target game obj for unit construction progress processor; disabled at start	

	public UIGrid queGrid;
	public UIScrollView scrollView;
	public UIPanel scrollViewPanel;


	//numerics
	//button positions
	//private int[] 
	//OffScreenX = new int[numberOfUnits]{-700,-550,-400,-250,-100,50,200,350,500,650,},//where the que prefabs are stored 800,950
		//OnScreenX = new int[maxTypes]{ -50, -200, -350 };	//the three unit in training sockets

	private int 
		OffScreenY = 500,//Y positions ofscreen
		OnScreenY = 230,//y positions, on screen
		currentAction = -1;//action 0 cancel 1 finished 2 exitmenu

	private float z = -1f;
	
	//Buttons	
	public GameObject[] 
		//BuildButtons = new GameObject[numberOfUnits],
		QueButtons = new GameObject[numberOfUnits],//small icons that appear when a unit is in training
		ProgBars = new GameObject[numberOfUnits];//under que buttons	

	public UILabel[] 
		Counters = new UILabel[numberOfUnits],//0-10 numbers displayed on que buttons
		UnitNames = new UILabel[numberOfUnits],
		BuildTimes = new UILabel[numberOfUnits],
		BuildQuantity = new UILabel[numberOfUnits],
		UnitPrices = new UILabel[numberOfUnits];

	public List<GameObject> activeQueList = new List<GameObject>();//make private

	public TextAsset UnitsXML;
	private List<Dictionary<string,string>> units = new List<Dictionary<string,string>>();
	private Dictionary<string,string> dictionary;	

	private Component stats, statusMsg;
	public UnitProc unitProc;

	//Button messages
	public void OnBuild0()	{ VerifyConditions(0);  }
	public void OnBuild1()	{ VerifyConditions(1);	}
	public void OnBuild2()	{ VerifyConditions(2);	}
	public void OnBuild3()	{ VerifyConditions(3);	}
	public void OnBuild4()	{ VerifyConditions(4);	}
	public void OnBuild5()	{ VerifyConditions(5);	}
	public void OnBuild6()	{ VerifyConditions(6);	}
	public void OnBuild7()	{ VerifyConditions(7);	}
	public void OnBuild8()	{ VerifyConditions(8);	}
	public void OnBuild9()	{ VerifyConditions(9);	}
	//public void OnBuild10() { VerifyConditions(10); }
	//public void OnBuild11() { VerifyConditions(11); }
	
	public void OnCancel0()  { UnBuild(0,0); }
	public void OnCancel1()  { UnBuild(1,0); }
	public void OnCancel2()  { UnBuild(2,0); }
	public void OnCancel3()  { UnBuild(3,0); }
	public void OnCancel4()  { UnBuild(4,0); }
	public void OnCancel5()  { UnBuild(5,0); }
	public void OnCancel6()  { UnBuild(6,0); }
	public void OnCancel7()  { UnBuild(7,0); }
	public void OnCancel8()  { UnBuild(8,0); }
	public void OnCancel9()  { UnBuild(9,0); }
	//public void OnCancel10() { UnBuild(10,0);}
	//public void OnCancel11() { UnBuild(11,0);}
	

	void Start () {
		stats = GameObject.Find("Stats").GetComponent<Stats>();
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();
		//unitProc = UnitProcObj.GetComponent("UnitProc");		//load the remote script
		GetUnitsXML();
		UpdateXMLData ();
	}

	public void GetUnitsXML()
	{

		XmlDocument xmlDoc = new XmlDocument(); 
		xmlDoc.LoadXml(UnitsXML.text); 
		XmlNodeList unitsList = xmlDoc.GetElementsByTagName("Unit");
			
			foreach (XmlNode unitInfo in unitsList)
			{
			XmlNodeList unitsContent = unitInfo.ChildNodes;	
				dictionary = new Dictionary<string, string>();

				foreach (XmlNode unitItems in unitsContent) // levels itens nodes.
				{
				
				if(unitItems.Name == "Name")
				{
					dictionary.Add("Name",unitItems.InnerText); 
				}	
				if(unitItems.Name == "UnitType")
				{
					dictionary.Add("UnitType",unitItems.InnerText); 
				}	
				if (unitItems.Name == "Description") 
				{
					dictionary.Add ("Description", unitItems.InnerText); 
				}
				if(unitItems.Name == "Currency")
				{
					dictionary.Add("Currency",unitItems.InnerText); 
				}	
				if(unitItems.Name == "Price")
				{
					dictionary.Add("Price",unitItems.InnerText); 
				}
				if(unitItems.Name == "TimeToBuild")
				{
					dictionary.Add("TimeToBuild",unitItems.InnerText); 
				}
				if(unitItems.Name == "Life")
				{
					dictionary.Add("Life",unitItems.InnerText); 
				}	
				if(unitItems.Name == "Size")
				{
					dictionary.Add("Size",unitItems.InnerText); 
				}	
				if(unitItems.Name == "XpAward")
				{
					dictionary.Add("XpAward",unitItems.InnerText); 
				}
				if(unitItems.Name == "Upgrades")
				{
					dictionary.Add("Upgrades",unitItems.InnerText); 
				}
				if(unitItems.Name == "UpRatio")
				{
					dictionary.Add("UpRatio",unitItems.InnerText); 
				}
				}
				units.Add(dictionary);
			}
		
	}

	private void UpdateXMLData()
	{
		for (int i = 0; i < UnitPrices.Length; i++) 
		{
			UnitNames[i].text = units [i] ["Name"];
			UnitPrices[i].text = units [i] ["Price"];//prices
			BuildTimes[i].text = units[i]["TimeToBuild"];

			trainingTimes[i] = int.Parse (units[i]["TimeToBuild"]);//time to build
			sizePerUnit[i] = int.Parse (units[i]["Size"]);//time to build

			//in case user exits before passing the info to unit proc - MenuUnit is open
			unitProc.trainingTimes[i] = trainingTimes[i];			
			unitProc.sizePerUnit[i] = sizePerUnit[i];
		}
	}

	private void VerifyConditions(int currentSelection)
	{
		rebuild = false;
		bool canBuild = true;
		int price = int.Parse (units [currentSelection] ["Price"]);

		if(units [currentSelection] ["Currency"] == "Gold")//this is string, not boolean 
		{
			if(!((Stats)stats).EnoughGold(price))
			{				
				canBuild = false;
				((Messenger)statusMsg).DisplayMessage("Insufficient gold.");				
			}
		}
		else if(units [currentSelection] ["Currency"] == "Mana")
		{
			if(!((Stats)stats).EnoughMana(price))
			{
				canBuild = false;
				((Messenger)statusMsg).DisplayMessage("Insufficient mana.");
			}
		}
		else //if(units [currentSelection] ["Currency"] == "Crystals") - if you want to have units sold with crystals
		{
			if(!((Stats)stats).EnoughCrystals(price))
			{
				canBuild = false;
				((Messenger)statusMsg).DisplayMessage("Insufficient crystals.");
			}
		}

		if(trainingIndexes[currentSelection] == 10)
		{
			canBuild = false;
			((Messenger)statusMsg).DisplayMessage("10 units limit.");
		}

		if(((Stats)stats).occupiedHousing + 
		   int.Parse (units [currentSelection] ["Size"])>
		   ((Stats)stats).maxHousing)
		{
			canBuild = false;
			((Messenger)statusMsg).DisplayMessage("Increase your soldier housing capacity.");
		}

		if (canBuild) 
		{			

			if(units [currentSelection] ["Currency"] == "Gold")
			{	
				Pay (price, 0, 0); 
			}
			else if(units [currentSelection] ["Currency"] == "Mana")
			{
				Pay (0, price, 0); 
			}
			else //if(units [currentSelection] ["Currency"] == "Crystals")
			{
				((Stats)stats).crystals -= int.Parse (units [currentSelection] ["Price"]);
				Pay (0, 0, price); 
			}

			((Stats)stats).experience += int.Parse (units [currentSelection] ["XpAward"]);
			if(((Stats)stats).experience>((Stats)stats).maxExperience)
			((Stats)stats).experience=((Stats)stats).maxExperience;

			((Stats)stats).occupiedHousing += int.Parse (units [currentSelection] ["Size"]);

			((Stats)stats).UpdateUI();
			Build (currentSelection);
		} 
	}

	private void Pay(int gold, int mana, int crystals)
	{
		((Stats)stats).SubstractResources (gold, mana, crystals); 
	}
	private void Refund(int gold, int mana, int crystals)
	{
		((Stats)stats).AddResources (gold, mana, crystals); 
	}
	public void PassValuestoProc()
	{	
		pause = true;
		unitProc.Pause ();

		bool queEmpty = true;	//verify if there's anything under constuction
		
		for (int i = 0; i < trainingIndexes.Length; i++) 
		{
			if(trainingIndexes[i]>0)
			{
				queEmpty = false;
				break;
			}
		}
				
		if(!queEmpty)
		{			
			unitProc.currentSlidVal = currentSlidVal;	
			unitProc.currentTrainingTime = currentTrainingTime;			
			unitProc.queList.Clear();	//clear queIndex/trainingIndex/objIndex dictionary
																											
			for (int i = 0; i < trainingIndexes.Length; i++) 
			{			
				//((MenuUnitProc)menuUnitProc).trainingIndexes[i] = trainingIndexes[i];	//save units under construction, including 0s			
				if(trainingIndexes[i]>0)
				{
					unitProc.queList.Add(new Vector3(					// qIndex, objIndex, trainingIndex
						((QIndex)QueButtons[i].GetComponent("QIndex")).qindex,						
						((QIndex)QueButtons[i].GetComponent("QIndex")).objindex,	//same as i
					    trainingIndexes[i])); //number of units under construction
										
				}

			}
			unitProc.trainingTimes = trainingTimes;
			unitProc.SortList();
			EraseValues();
		}
		unitProc.sizePerUnit = sizePerUnit;//pass the weights regardless
		((Stats)stats).sizePerUnit = sizePerUnit;

		unitProc.Resume ();
	}
	private void EraseValues()
	{		
		for (int i = 0; i < trainingIndexes.Length; i++) 
		{			
			if(trainingIndexes[i]>0)
			{
				int a = trainingIndexes[i];		//while unbuilding, trainingIndexes[i] is modified - no longer valid references
				for (int j = 0; j < a; j++)
				{
					UnBuild(i,2);
				} 
			}
		}
		currentSlidVal = 0;
		timeRemaining = 0;
		currentTimeRemaining = 0;
		hours = minutes = seconds = 0 ; //?totalTime
		queList.Clear ();
		((UILabel)HintLabel).text ="Tap on a unit to summon them and read the description.";				
			
	}
		
	public void LoadValuesfromProc()
	{	
		unitProc.Pause ();

		pause = true;

		bool queEmpty = true;

		if(unitProc.queList.Count > 0){queEmpty = false;}//unit proc is disabled at start???
	
		if(!queEmpty)
		{					
			currentSlidVal = unitProc.currentSlidVal;
			currentTrainingTime = unitProc.currentTrainingTime;

			queList.Clear();
			
			for (int i = 0; i < unitProc.queList.Count; i++) 
			{					
				queList.Add(unitProc.queList[i]);	
			}
			
			unitProc.queList.Clear();	//reset remote list
			ReBuild();
		}
		pause = false;
	}
		
	private void ReBuild()
	{		
		rebuild = true;

		queList.Sort(delegate (Vector3 v1, Vector3 v2)// qIndex, objIndex, trainingIndex
		{
			return v1.x.CompareTo(v2.x);			
		});		
		
		for (int i = 0; i < queList.Count; i++) // qIndex, objIndex, trainingIndex
		{			
			for (int j = 0; j < queList[i].z; j++) 
				{
					Build((int)queList[i].y);
				}
		}

		progCounter = 0;	//delay first bar update 
		((UISlider)((ProgBars[(int)queList [0].y]).GetComponent("UISlider"))).value = currentSlidVal; //update the progress bar with the loaded value

		UnitProcObj.SetActive(false);
		UpdateTime ();
	}

	void FixedUpdate()
	{
		if (pause)
			return;
		if(queCounter>0)
		{
			ProgressBars();	//fix this - progress bars resets currentSlidVal at reload
		}		
		else if(resetLabels)
		{				
			((UILabel)TimeLabel).text = "-";
			((UILabel)FinishPriceLb).text = "-";
			currentSlidVal = 0; progCounter = 0;
			resetLabels = false;			
		}
	}
			
	void Build(int i)				
	{				
			resetLabels = true;
		
			bool iInQue = false;
						
			if(((QIndex)QueButtons[i].GetComponent("QIndex")).inque)
			{
				iInQue = true;
			}			
			
			if(iInQue)
			{
				trainingIndexes[i]++;
				((UILabel)Counters[i]).text = trainingIndexes[i].ToString();	
				((UILabel)HintLabel).text = units [i] ["Description"];
			}			
			
			else if(!iInQue )
			{	
				AddToScrollView (QueButtons [i]);

				trainingIndexes[i]++;
				
				((QIndex)QueButtons[i].GetComponent("QIndex")).inque = true;
				
				((QIndex)QueButtons[i].GetComponent("QIndex")).qindex = queCounter;			
			
				((UILabel)Counters[i]).text = trainingIndexes[i].ToString();
				
			queCounter++;
								
			((UILabel)HintLabel).text = units [i] ["Description"];
			}		
			else
			{
				((UILabel)HintLabel).text = "You can train 3 unit types at once.";
			}		

		UpdateTime ();		
	}

	private void AddToScrollView(GameObject QueButton)
	{
		QueButton.SetActive(true);
		//queGrid.gameObject.AddChild (QueButton.transform);
		queGrid.AddChild (QueButton.transform);//obsolete, but the recommended code doesn work

		QueButton.transform.localPosition = new Vector3 ((queGrid.GetChildList().Count-1)*-80, 0, 0);
		ResetScrollView ();

		if(!rebuild && queGrid.GetChildList().Count>8)// !rebuild = the list doesn't alight to the left(last widget) when menu is reopened - I prefer to see training progress
		{
			AdjustOffset ();
		}
	}

	private void RemoveFromScrollView(GameObject QueButton)
	{		
		queGrid.RemoveChild (QueButton.transform);
		QueButton.SetActive(false);
		OrderButtons();
		ResetScrollView ();
	}
	private void ResetScrollView()
	{
		queGrid.repositionNow = true;	
		scrollView.ResetPosition ();				
		queGrid.Reposition ();
	}

	private void AdjustOffset()//if the widget list extends beyond the left margin, show last widget added to the scroll view
	{
		List<Transform> tempList = queGrid.GetChildList ();
		int diff = tempList.Count - 8;	
		scrollView.MoveRelative(new Vector3(diff*80,0,0));
	}

	private void OrderButtons()//action 0 cancel 1 finished 2 exitmenu
	{
		SortQue();
		float posX = 0;
		for (int j = 0; j < activeQueList.Count; j++) 
		{	
			//being complex buttons, with children progress bars/buttons, they need to be disabled/enabled ???
			activeQueList [j].SetActive (false);	
			activeQueList[j].transform.localPosition = new Vector3(posX, 0, 0);
			((QIndex)activeQueList[j].GetComponent("QIndex")).qindex = j;
			posX -= 80;
			activeQueList [j].SetActive (true);

		}
	}

	private void SortQue()
	{		
		activeQueList.Clear();	

		for (int j = 0; j < QueButtons.Length; j++) 
		{	
			if(((QIndex)QueButtons[j].GetComponent("QIndex")).inque)
			{
				activeQueList.Add(QueButtons[j]);
			}

		}

		activeQueList.Sort(delegate(GameObject button1, GameObject button2)
			{				
				return ((QIndex)button1.GetComponent("QIndex")).qindex.CompareTo(((QIndex)button2.GetComponent("QIndex")).qindex); 
			});
	}

	void UnBuild(int i, int action)			// action 0 cancel 1 finished 2 exitmenu
	{	
		currentAction = action;
		if(action == 0)
		{
			hours = minutes = seconds = 0;
			int 
				itemPrice = int.Parse (units [i] ["Price"]);

			if(units [i] ["Currency"] == "Gold")//return value is max storage capacity allows it
			{
				if (itemPrice < (((Stats)stats).maxGold - (int)((Stats)stats).gold))
					Refund (itemPrice, 0, 0);
				else
				{
					Refund (((Stats)stats).maxGold - ((Stats)stats).gold, 0, 0);//refunds to max storag capacity
					((Messenger)statusMsg).DisplayMessage("Stop canceling units!\nYou are losing gold!");
				}
			}
			
			else if(units [i] ["Currency"] == "Mana")
			{
				if(itemPrice<(((Stats)stats).maxMana - (int)((Stats)stats).mana))
					Refund (0, itemPrice, 0);
				else
				{
					Refund (0, ((Stats)stats).maxMana-((Stats)stats).mana, 0);
					((Messenger)statusMsg).DisplayMessage("Stop canceling units!\nYou are losing mana!");
				}
			}
			else //if(units [i] ["Currency"] == "Crystals")
			{			
				Refund (0, 0, itemPrice);	
			}

			((Stats)stats).occupiedHousing -= int.Parse (units [i] ["Size"]);
			((Stats)stats).UpdateUI();
		}

		if(trainingIndexes[i]>1)
		{
			trainingIndexes[i]--;
			((UILabel)Counters[i]).text = trainingIndexes[i].ToString();
			((UISlider)((ProgBars[i]).GetComponent("UISlider"))).value = 0;	
		}
		else
		{		
			((QIndex)QueButtons[i].GetComponent("QIndex")).inque = false;
			((QIndex)QueButtons[i].GetComponent("QIndex")).qindex = 50;			
			((UISlider)((ProgBars[i]).GetComponent("UISlider"))).value = 0;
			queCounter--;
			trainingIndexes[i]--;				
			//QueButtons[i].transform.position = new Vector3(OffScreenX[i],OffScreenY,z);			
			//QueButtons[i].SetActive(false);			

			//OrderButtons ();
			RemoveFromScrollView(QueButtons[i]);

		}
		
		switch (action) {
		case 0:
			((UILabel)HintLabel).text ="Training canceled.";
		break;
		case 1:
			((UILabel)HintLabel).text ="Training complete.";
		break;			
		}	

		UpdateTime ();
	} 

	private IEnumerator LateDrag()
	{
		yield return new WaitForSeconds (0.2f);
		//scrollView.Drag ();
		scrollView.OnPan(new Vector2(10,0));
		//scrollView.GetComponent<UIScrollView> ().MoveAbsolute (new Vector3 (0,0,0));

	}

	private void UpdateTime()
	{
		timeRemaining = 0;
		
		for (int i = 0; i < trainingIndexes.Length; i++) 
		{
			timeRemaining += trainingIndexes[i]*trainingTimes[i];
		}
		if(activeQueList.Count>0)
		{
			currentTrainingTime = trainingTimes[((QIndex)activeQueList[0].GetComponent("QIndex")).objindex];
		}
		else
		{
			currentTrainingTime = 0;
		}
		timeRemaining -= currentSlidVal*currentTrainingTime;

		if(timeRemaining>0)
		{
			hours = (int)timeRemaining/60;
			minutes = (int)timeRemaining%60;
			seconds = (int)(60 - (currentSlidVal*currentTrainingTime*60)%60);			
		}

		if (minutes==60) minutes=0;
		if (seconds==60) seconds=0;

		if(hours>0 )
		{			
			((UILabel)TimeLabel).text = 
				hours.ToString() +" h " +
					minutes.ToString() +" m " +
					seconds.ToString() +" s ";			
		}
		else if(minutes > 0 )
		{
			((UILabel)TimeLabel).text = 
				minutes.ToString() +" m " +
					seconds.ToString() +" s ";			
		}
		else if(seconds > 0 )
		{
			((UILabel)TimeLabel).text = 
				seconds.ToString() +" s ";
		}
		
		if (timeRemaining >= 4320)	priceInCrystals = 150;
		else if (timeRemaining >= 2880)	priceInCrystals = 70;
		else if (timeRemaining >= 1440)	priceInCrystals = 45;
		else if (timeRemaining >= 600)	priceInCrystals = 30;
		else if (timeRemaining >= 180)	priceInCrystals = 15;
		else if (timeRemaining >= 60)	priceInCrystals = 7;
		else if (timeRemaining >= 30)	priceInCrystals = 3;
		else if (timeRemaining >= 0)	priceInCrystals = 1;
		
		((UILabel)FinishPriceLb).text = priceInCrystals.ToString();
	}

	private void ProgressBars()
	{
		//Time.deltaTime = 0.016; 60*Time.deltaTime = 1s ; runs at 60fps

		progCounter += Time.deltaTime*0.5f;
		if(progCounter > progTime)
		{		
			//print("prog bar tick");	
			SortQue();							
			int objIndex = ((QIndex)activeQueList[0].GetComponent("QIndex")).objindex;	
			currentTrainingTime = trainingTimes[objIndex];

			((UISlider)((ProgBars[objIndex]).GetComponent("UISlider"))).value += ((Time.deltaTime)/trainingTimes[objIndex]);

			currentSlidVal = ((UISlider)((ProgBars[objIndex]).GetComponent("UISlider"))).value;
			((UISlider)((ProgBars[objIndex]).GetComponent("UISlider"))).value = 
				Mathf.Clamp(((UISlider)((ProgBars[objIndex]).GetComponent("UISlider"))).value,0,1);
			
			if(((UISlider)((ProgBars[objIndex]).GetComponent("UISlider"))).value==1)
			{ 
				FinishObject();				
			}

			progCounter = 0;
			UpdateTime();				
		}				
	}
	
	private void FinishObject()
	{		
		int objIndex = ((QIndex)activeQueList[0].GetComponent("QIndex")).objindex;
		UnBuild(objIndex,1);	
		((Stats)stats).existingUnits[objIndex]++;
		((Stats)stats).UpdateUnitsNo();
	}

	private void IncreasePopulation()
	{
		for (int i = 0; i < trainingIndexes.Length; i++) 
		{			
			if(trainingIndexes[i]>0)
			{
				int a = trainingIndexes[i];		//trainingIndexes[i] is modified in loop - no longer valid references
				for (int j = 0; j < a; j++)
				{
					((Stats)stats).existingUnits[i]++;
				} 
			}
		}	
	}

	public void FinishNow()
	{
		if (priceInCrystals <= ((Stats)stats).crystals) 
		{
			((Stats)stats).crystals -= priceInCrystals;	
			((Stats)stats).UpdateUI();
			((UILabel)HintLabel).text ="Training complete.";
			IncreasePopulation();
			((Stats)stats).UpdateUnitsNo();
			EraseValues();
		} 

		else if(timeRemaining > 0)			
		{
			((Messenger)statusMsg).DisplayMessage("Not enough crystals");
		}
	}
}
