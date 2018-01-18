using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RemovableCreator : MonoBehaviour {

	private const int numberOfRemovables = 7; 

	private int 
			currentSelection, //one of the removable prefabs, randomly generated and then processed
			zeroZ = 0,
			grassZ = 2;

	public int removableIndex = -1;		//increments to give removables and grass patches an unique ID number

	public int[] removeTimes = new int[numberOfRemovables];

	public bool battleMap = false;

	public GameObject[] 
	RemovablesPf = new GameObject[numberOfRemovables];

	public GameObject 
		RemovableSelectedPanel, 
		RemovableTimerPf,
		GrassRemovablePf,
		GroupRemovables;

	private GameObject 
		selectedRemovable,
		selectedRemovableGrass,
		selectedRemovablePad,
		selectedRemovableTimer;

	public UILabel removePriceLb;
	public UISprite currencyIco;//0 gold 1 mana
	public string[] currencyIcoNames= new string[2];

	public TextAsset RemovablesXML;	//variables for loading building characteristics from XML
	private List<Dictionary<string,string>> removables = new List<Dictionary<string,string>>();
	private Dictionary<string,string> dictionary;

	private Component relay, stats, statusMsg;

	// Use this for initialization
	void Start () {	
		relay = GameObject.Find("Relay").GetComponent<Relay>();

		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();

		if(!battleMap)
		{
			stats = GameObject.Find("Stats").GetComponent<Stats>();
			removableIndex = ((Stats)stats).structureIndex;

			if(!((Stats)stats).removablesCreated)
			{
				//InitializeRemovables();
			}
		}
		GetRemovablesXML();
		RecordRemoveTime();//make sure this happens before game load if automatic


	}

	private void GetRemovablesXML()//reads buildings XML
	{
		XmlDocument xmlDoc = new XmlDocument(); 
		xmlDoc.LoadXml(RemovablesXML.text); 
		XmlNodeList removablesList = xmlDoc.GetElementsByTagName("Removable");
		
		foreach (XmlNode removableInfo in removablesList)
		{
			XmlNodeList removablesContent = removableInfo.ChildNodes;	
			dictionary = new Dictionary<string, string>();
			
			foreach (XmlNode removableItems in removablesContent) // levels itens nodes.
			{
				/*
					<Name>ClamA</Name>	
					<RemoveTime>5</RemoveTime>
					<Currency>Gold</Currency>				<!-- Save as 0 Gold 1 Mana -->
					<RemovePrice>50</RemovePrice>
					<XpAward>2</XpAward>
				*/
				if(removableItems.Name == "Name")
				{
					dictionary.Add("Name",removableItems.InnerText); // put this in the dictionary.
				}	
				if(removableItems.Name == "RemoveTime")
				{
					dictionary.Add("RemoveTime",removableItems.InnerText); 
				}
				if(removableItems.Name == "Currency")
				{
					dictionary.Add("Currency",removableItems.InnerText); 
				}
				if(removableItems.Name == "RemovePrice")
				{
					dictionary.Add("RemovePrice",removableItems.InnerText); 
				}
				if(removableItems.Name == "XpAward")
				{
					dictionary.Add("XpAward",removableItems.InnerText); 
				}
			}
			removables.Add(dictionary);
		}
	}

	private void RecordRemoveTime()
	{
		for (int i = 0; i < removables.Count; i++) 
		{
			removeTimes[i] = int.Parse(removables [i] ["RemoveTime"]);
		}
	}

	public void InitializeRemovables()
	{
		((Stats)stats).removablesCreated = true;
		((Stats)stats).gameWasLoaded = true;		//do not allow game loads after Removable generation

		Node[,] nodes = GridManager.instance.nodes;

		for (int i = 2; i < nodes.GetLength(0)-2; i++)
		{
			for (int j = 2; j < nodes.GetLength(1)-2; j++)
			{
				int k = 0;
				k = Random.Range(0,10);//about 1 in 10 probability to instantiate something

				if (k==0)
				{
					currentSelection = Random.Range(0,numberOfRemovables);

					Vector3 
						nodePos = nodes[i,j].position,	
						removablePos = new Vector3(nodePos.x,nodePos.y, zeroZ),
						grassPos = new Vector3(nodePos.x,nodePos.y, grassZ);

					GameObject removable = (GameObject)Instantiate(RemovablesPf[currentSelection], removablePos, Quaternion.identity);	
					GameObject removableGrass = (GameObject)Instantiate(GrassRemovablePf, grassPos, Quaternion.identity);	
					selectedRemovable = removable;
					selectedRemovableGrass = removableGrass;

					PlaceObject(currentSelection, i, j);
				}
			}
		}
		((Stats)stats).removablesCreated = true;
	}

	public void OnSelect(GameObject selectedRem)
	{
		selectedRemovable = selectedRem; 
		SelectRemovable ();
	}

	public void OK()//ok remove
	{
		Component remSelector = (RemovableSelector)selectedRemovable.GetComponent<RemovableSelector>();

		string removableType = ((RemovableSelector)remSelector).removableType;
		
		TypeToSelectionIndex(removableType);

		bool 			
			enoughtMoney = false,
			
			dobbitAvailable = ((Stats)stats).dobbits > ((Stats)stats).occupiedDobbits;	

		switch (removables [currentSelection] ["Currency"]) {
		case "Gold":
			enoughtMoney = ((Stats)stats).gold >= int.Parse (removables [currentSelection] ["RemovePrice"]);
			break;
		case "Mana":
			enoughtMoney = ((Stats)stats).mana >= int.Parse (removables [currentSelection] ["RemovePrice"]);
			break;		
		}

		if(enoughtMoney && dobbitAvailable)
		{
			switch (removables [currentSelection] ["Currency"]) {
			case "Gold":
				((Stats)stats).SubstractResources(int.Parse(removables [currentSelection] ["RemovePrice"]), 0, 0);
				break;
			case "Mana":
				((Stats)stats).SubstractResources(0,int.Parse(removables [currentSelection] ["RemovePrice"]), 0);
				break;		
			}

			((Stats)stats).occupiedDobbits++;

			((Stats)stats).UpdateUI();
		}
		else
		{
			if (!enoughtMoney) 
			{
				switch (removables [currentSelection] ["Currency"]) 
				{
				case "Gold":
					((Messenger)statusMsg).DisplayMessage ("Not enough Gold.");
					break;
				case "Mana":
					((Messenger)statusMsg).DisplayMessage ("Not enough Mana.");
					break;		
				}
			}

			if(!dobbitAvailable)
			((Messenger)statusMsg).DisplayMessage("You need a dobbit for this job.");

			return;
		}

		GameObject RemovableTimer = (GameObject)Instantiate(RemovableTimerPf, selectedRemovable.transform.position, Quaternion.identity);
		selectedRemovableTimer = RemovableTimer;
		SelectRemovableTimer ();
		//selectedRemovable.GetComponent<RemovableSelector> ().inRemoval = true;
		Deselect ();
	}

	private void SelectRemovableTimer()
	{

		Component 
			remTimer = selectedRemovableTimer.GetComponent<RemovableTimerSelector>(),
			remSelector = (RemovableSelector)selectedRemovable.GetComponent<RemovableSelector>();

		string remType = ((RemovableSelector)remSelector).removableType;
		
		TypeToSelectionIndex(remType);

		((RemovableTimerSelector)remTimer).removeTime = int.Parse(removables [currentSelection] ["RemoveTime"]);

		((RemovableTimerSelector)remTimer).xpAward = int.Parse(removables [currentSelection] ["XpAward"]);
		((RemovableTimerSelector)remTimer).removableName = removables [currentSelection] ["Name"];
		((RemovableTimerSelector)remTimer).crystalAward = GenerateReward ();
		((RemovableTimerSelector)remTimer).removableIndex = ((RemovableSelector)remSelector).removableIndex;
		((RemovableSelector)remSelector).inRemoval = 1;//0 for false, 1 for true
		((RemovableTimerSelector)remTimer).inRemovalB = true;

		selectedRemovableTimer.transform.parent = selectedRemovable.transform;
		((RemovableTimerSelector)selectedRemovableTimer.GetComponent("RemovableTimerSelector")).isSelected = false;
			
	}

	private int GenerateReward()
	{
		int winProbability =0, xpAward = 0;

		winProbability = Random.Range (0, 2);
		if (winProbability != 0)
			xpAward = Random.Range (1, 5);

		return xpAward;
	}


	public void Cancel()
	{
		Deselect ();
	}

	private void Deselect()
	{
		RemovableSelectedPanel.SetActive (false);
		selectedRemovable.GetComponent<RemovableSelector>().isSelected = false;
		((Relay)relay).pauseInput = false;		
	}

	private void SelectRemovable()//string tag
	{
		RemovableSelectedPanel.SetActive (true);

		Component remSelector = selectedRemovable.GetComponent<RemovableSelector>();

		string removableType = ((RemovableSelector)remSelector).removableType;

		TypeToSelectionIndex(removableType);

		RemovableSelectedPanel.transform.position = selectedRemovable.transform.position;	
		removePriceLb.text =  removables [currentSelection] ["RemovePrice"];

		//if(removables [currentSelection] ["Currency"]=="Gold")

		switch (removables [currentSelection] ["Currency"]) 
		{
		case "Gold":
			currencyIco.spriteName=currencyIcoNames[0];	//248 237 93 ->  0.97 0.92 0.36  248/255 etc
			removePriceLb.color=new Color(1,1,0);
			break;
		case "Mana":
			currencyIco.spriteName=currencyIcoNames[1];
			removePriceLb.color=new Color(0,1,1);
			break;
		}

		((Relay)relay).pauseInput = true;	
	}

	private void TypeToSelectionIndex(string type)
	{
		//"ClamA","ClamB","ClamC","TreeA","TreeB","TreeC","TreeD"
		switch (type) 					//converts the type to the dictionary index
		{
		case "ClamA":
			currentSelection = 0;
			break;
		case "ClamB":
			currentSelection = 1;
			break;
		case "ClamC":
			currentSelection = 2;
			break;
		case "TreeA":
			currentSelection = 3;
			break;
		case "TreeB":
			currentSelection = 4;
			break;
		case "TreeC":
			currentSelection = 5;
			break;
		case "TreeD":
			currentSelection = 6;
			break;
		}
	}


	private void PlaceObject(int currentSelection, int i, int j)//string tag, 
	{				
		removableIndex++;
		((Stats)stats).structureIndex++;

		Component 
		removableSelector = selectedRemovable.GetComponent<RemovableSelector>(),
		grassSelector =selectedRemovableGrass.GetComponent<GrassSelector>();

		((RemovableSelector)removableSelector).removableIndex = removableIndex;
		((RemovableSelector)removableSelector).iColumn = i;
		((RemovableSelector)removableSelector).jRow = j;
				
		((GrassSelector)grassSelector).grassIndex = removableIndex;

		selectedRemovable.transform.parent = GroupRemovables.transform;	
		selectedRemovableGrass.transform.parent = selectedRemovable.transform;
	}

	void OnGUI()
	{
		if (GUI.Button (new Rect (5, Screen.height*0.5f, 45, 25), "Trees"))
		{
			//I will leave this on manual control; When save/load are automated, include this as well

		if (!battleMap) 
		{
			if (!((Stats)stats).removablesCreated && !((Stats)stats).gameWasLoaded)
			{
				InitializeRemovables();
			} 
			else 
			{
					((Messenger)statusMsg).DisplayMessage ("You already generated the removables, or loaded a game.");
					((Messenger)statusMsg).DisplayMessage ("You wouldn't want a tree landing in the middle of a house, would you?");			
			}
		}
	}

}
}
