using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

/// <summary>
/// 
/// </summary>

public class BaseCreator : MonoBehaviour {

	#region Variables
	public int totalStructures = 20;				//will use this for iterations, since the const below is not visible in the inspector; trim excess elements !
	protected const int noOfStructures = 20;		//number of maximum existing structures ingame, of any kind - buildings, weapons, walls

	public int structureIndex = -1;				//associates the underlying grass with the building on top, so they can be reselected together

	public bool 	
	inCollision;// prevents placing the structure in inaccessible areas/on top of another building

	public bool 
	mouseFollow = true,
	isReselect = false,					//building is under construction or reselected
	myTown = true,						//stats game obj does not exist on battle map
	gridBased = false,					//are objects placed by position or by grid Row/Col index
	allInstant = false;

	public GameObject 
	StructureControlMenu, 				//menu that appears when you reselect a finished building - buttons: upgrade, move, ok, cancel	
	Scope,								// a crosshair that follows the middle of the screen, for placing a new building; position is adjusted to exact grid middle point
	ParentGroup,						// to keep all structures in one place in the ierarchy, they are parented to this empty object
	MovingPad;							// the arrow pad - when created/selected+move, the structures are parented to this object that can move

	public string 
	structureXMLTag, 
	structureClass;//Building,Wall,Weapon,Ambient

	public GameObject FieldFootstep;	

	public TextAsset StructuresXML;	//variables for loading building characteristics from XML

	//by correlating structurePrefabs with the associated grass patch, we will be able to use a common formula to instantiate them
	public GameObject[] 
		structurePf = new GameObject[noOfStructures],
		grassPf = new GameObject[10],
		constructionPf = new GameObject[10]; 		//Grass1xWall, Grass1xFence, Grass1xDeco, Grass1xRemovableI, Grass1xRemovableII, Grass1xRemovableIII, Grass2x,Grass3x,Grass4x,Grass5x; separated because removables and walls can be hundreds

	public int[] 			//buildings can have different size grass patches; weapons 2x2,walls 1x1 and removables 1x1 one type each
		grassTypes = new int [noOfStructures],
		constructionTypes = new int [noOfStructures],	//matching construction prefabs based on size
		pivotCorrections = new int[noOfStructures],		//set manually 0 false 1 true
		isArray =  new int[noOfStructures],				//set manually 0 false 1 true - placement in array mode rows of flowers, walls
		isInstant = new int[noOfStructures];			//set manually 0 false 1 true - no construction sequence - instantiate immediately

	protected int 
		currentSelection = 0,
		gridx = 256,						//necessary to adjust the middle screen "target" to the exact grid X position
		gridy = 181,						//necessary to adjust the middle screen "target" to the exact grid Y position
		padZ = -3,							//moving pad
		zeroZ = 0,
		grassZ = 2;	

	protected float 
		touchMoveCounter = 0,				//drag-moves the buildings in steps
		touchMoveTime = 0.1f,
		xmlLoadDelay = 0.4f;					//xml read is slow, some operations must be delayed

	protected Vector2 mousePosition;

	private bool 			
	pivotCorrection = false,
	displacedonZ = false;					//adjusts the position to match the grid
			
	private GameObject//make private
		selectedStructure,
		selectedGrass,
		selectedConstruction;				//current selected "under construction" prefab

	//private float initSequenceDelay = 0.2f;//necessary to load xml properly


	public List<Dictionary<string,string>> structures = new List<Dictionary<string,string>>();
	protected Dictionary<string,string> dictionary;

	//Interfaces

	public UIAtlas Portraits, PortraitsBW;
	
	public UISprite[] 
	NicheSprites = new UISprite[noOfStructures],	//for color/black&white availability switch
	PortraitSprites = new UISprite[noOfStructures];
	
	public UILabel[] 
	NameLbs = new UILabel[noOfStructures],			//price labels on the construction panel buttons
	TimeLbs = new UILabel[noOfStructures],
	QuantityLbs = new UILabel[noOfStructures],
	PriceLbs = new UILabel[noOfStructures];

	public int[] 
		existingStructures = new int[noOfStructures],// necessary to keep track of each structure type number and enforce the "max number of structures" condition
		allowedStructures = new int[noOfStructures];


	//Array creator
	//Multiple selection for fields

	public List<GameObject> starList = new List<GameObject> ();//make private
	public List<Vector3> spawnPointList = new List<Vector3> ();//make private
	public GameObject spawnPointStar;

	private bool //make private
	drawingField = false,
	startField = false,
	endField = false,
	isField = false,
	buildingFields = false;

	private float 
	starSequencer = 0.2f;//0.2f

	private Vector3 startPosition, endPosition;

	private int 
	startCell, endCell,  			//for start/end cells to draw the field
	startRow, startCol,
	currentFieldIndex, 
	fieldDetectorZ = 0;

	protected Component stats, soundFX, transData, cameraController, relay, statusMsg, menuMain, resourceGenerator;//protected


	//void Start () {}

	#endregion

	protected void InitializeComponents()
	{
		transData = GameObject.Find ("TransData").GetComponent<TransData>();
		relay = GameObject.Find ("Relay").GetComponent<Relay>();
		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();// //connects to SoundFx - a sound source near the camera
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();
		cameraController  = GameObject.Find("tk2dCamera").GetComponent<CameraController>();//to move building and not scroll map
		menuMain = GameObject.Find ("Main").GetComponent<MenuMain> ();

		if(myTown)
		{
			resourceGenerator = GameObject.Find("ResourceGenerator").GetComponent <ResourceGenerator>();
			stats = GameObject.Find("Stats").GetComponent <Stats>();//conects to Stats script
			structureIndex = ((Stats)stats).structureIndex;
		}
	}

	//"Academy","Barrel","Chessboard","Classroom","Forge","Generator","Globe","Summon","Toolhouse","Vault","Workshop"
	//receive a NGUI button message to build
	public void OnBuild0()	 { currentSelection=0; 	Verify(); }//when a building construction menu button is pressed
	public void OnBuild1()	 { currentSelection=1;  Verify(); }
	public void OnBuild2()	 { currentSelection=2;  Verify(); }
	public void OnBuild3()	 { currentSelection=3;  Verify(); }
	public void OnBuild4()	 { currentSelection=4; 	Verify(); }
	public void OnBuild5()	 { currentSelection=5;  Verify(); }
	public void OnBuild6()	 { currentSelection=6;  Verify(); }
	public void OnBuild7()	 { currentSelection=7;  Verify(); }
	public void OnBuild8()	 { currentSelection=8;  Verify(); }
	public void OnBuild9()	 { currentSelection=9;  Verify(); }
	public void OnBuild10()  { currentSelection=10; Verify(); }
	public void OnBuild11()  { currentSelection=11; Verify(); }
	public void OnBuild12()  { currentSelection=12; Verify(); }
	public void OnBuild13()  { currentSelection=13; Verify(); }
	public void OnBuild14()  { currentSelection=14; Verify(); }
	public void OnBuild15()  { currentSelection=15; Verify(); }
	public void OnBuild16()  { currentSelection=16; Verify(); }
	public void OnBuild17()  { currentSelection=17; Verify(); }
	public void OnBuild18()  { currentSelection=18; Verify(); }
	public void OnBuild19()  { currentSelection=19; Verify(); }
	public void OnBuild20()  { currentSelection=20; Verify(); }

	private void Verify()
	{
		if (isArray[currentSelection] == 0) 
		{			
			VerifyConditions ();
		}
		else 
		{			
			isField = true;
			drawingField = true;
			Delay ();
			VerifyConditions ();
		}
	}

	protected void Delay()
	{
		((Relay)relay).DelayInput();
	}

	//private float mouseClickTimer = 0, mouseClickTime = 0.3f;

	// Update is called once per frame
	protected void Update () {
		if (MovingPad.activeSelf) 
		{
			if(((Relay)relay).delay)
				return;

			MouseTouchMove();

			//This doesn't work properly?
			if(Input.touchCount > 0 && Input.GetTouch(0).tapCount == 2)
			{
				OK ();
			}

			if (Input.GetMouseButton (0)) 
			{
				OK ();
				/*
				mouseClickTimer += Time.deltaTime;
				if (mouseClickTimer > mouseClickTime) 
				{
					mouseClickTimer = 0;
				}
				*/
			} 


			/*//keep this, is for delete confirmation widget - extend to any confirmation
			 
			if(!mouseFollow)
				return;
			
			if (Input.GetMouseButton(0))// 
			{
				StartCoroutine(MouseOperations(0));
				//MouseOperations(0);
			}
			else if(Input.GetMouseButton(1))
			{
				StartCoroutine(MouseOperations(1));
				//MouseOperations(1);
			}
			*/
		}	
		if (drawingField) 
		{
			if(!((Relay)relay).delay && Input.GetMouseButtonUp(0))
			{
				RecordSpawnPoint();
			}
		}


	}

	private IEnumerator MouseOperations(int index)
	{
		yield return new WaitForSeconds (0.2f);

		if(!((Relay)relay).delay)
		{
			switch (index) 
			{
			case 0:
				//OK ();
				((MenuMain)menuMain).OnCloseConfirmationBuilding();
				break;
			case 1:
				((MenuMain)menuMain).OnConfirmationBuilding();
				break;
			}
		}
	}

	/*

	A bit of magic here; although the XMLs for various structures - Buildings, Weapons, Walls will be different, each child creator script
	will call and process a different Get***XML
	
	 */

	protected void ReadStructureXML()
	{
		switch (structureXMLTag) 
		{
		case "Building":
			GetBuildingsXML();
		break;
		case "Wall":
			GetWallsXML();
			break;	
		case "Weapon":
			GetWeaponsXML();
			break;	
		case "Ambient":
			GetAmbientXML ();
			break;
		}
	}


	protected void GetBuildingsXML()//reads structures XML
	{
		XmlDocument xmlDoc = new XmlDocument(); 
		xmlDoc.LoadXml(StructuresXML.text); 
		XmlNodeList structuresList = xmlDoc.GetElementsByTagName(structureXMLTag);
		
		foreach (XmlNode structureInfo in structuresList)
		{
			XmlNodeList structureContent = structureInfo.ChildNodes;	
			dictionary = new Dictionary<string, string>();
			
			foreach (XmlNode structureItem in structureContent) // levels itens nodes.
			{
				/*
				<Name>Dobbit Toolhouse</Name>		
				<Description>They live in the tiniest of houses, they don't talk much, and they're not much to look at. Yet, you'll never have enough of these guys. We'll make sure of that.</Description>
										
				<Currency>Crystals</Currency>				<!-- save as 0 Gold 1 Mana 2 Crystals to buy; production/storage building, buy with gold, produces mana -->			
				<Price>25</Price>							<!-- amount of resource necessary to pay for the building -->
				
				<ProdType>None</ProdType>					<!-- resource produced - gold/mana/none-->	
				<ProdPerHour>0</ProdPerHour>				<!-- the amount of the resource generated per hour -->			
				
				<StoreType>None</StoreType>					<!-- resource stored - gold/mana/dual/none-->	
				<StoreCap>0</StoreCap>						<!-- gold/mana/dual storage -->		
				<PopCap>0</PopCap>							<!-- population storage		-->			
						
				<TimeToBuild>1</TimeToBuild>				<!-- the time (in minutes) needed to create the building -->
				<Life>200</Life>							<!-- life vs damage -->
				<XpAward>100</XpAward>						<!-- experience awarded for the completion of an object -->
				<Upgrades>0</Upgrades>						<!-- how many levels does it have to max; -->		
				<UpRatio>0</UpRatio>						<!-- how much can you upgrade in relation to HQ; -->	
				*/
				
				if(structureItem.Name == "Name")
				{
					dictionary.Add("Name",structureItem.InnerText); // put this in the dictionary.
				}						
				if(structureItem.Name == "StructureType")
				{
					dictionary.Add("StructureType",structureItem.InnerText); // put this in the dictionary.
				}	
				if(structureItem.Name == "Description")
				{
					dictionary.Add("Description",structureItem.InnerText); 
				}
				if(structureItem.Name == "Currency")
				{
					dictionary.Add("Currency",structureItem.InnerText); 
				}
				if(structureItem.Name == "Price")
				{
					dictionary.Add("Price",structureItem.InnerText); 
				}				
				if(structureItem.Name == "ProdType")
				{
					dictionary.Add("ProdType",structureItem.InnerText); 
				}				
				if(structureItem.Name == "ProdPerHour")
				{
					dictionary.Add("ProdPerHour",structureItem.InnerText); 
				}
				if(structureItem.Name == "StoreType")
				{
					dictionary.Add("StoreType",structureItem.InnerText); 
				}
				if(structureItem.Name == "StoreResource")
				{
					dictionary.Add("StoreResource",structureItem.InnerText); 
				}
				if(structureItem.Name == "StoreCap")
				{
					dictionary.Add("StoreCap",structureItem.InnerText); 
				}
				if(structureItem.Name == "PopCap")
				{
					dictionary.Add("PopCap",structureItem.InnerText); 
				}
				if(structureItem.Name == "TimeToBuild")
				{
					dictionary.Add("TimeToBuild",structureItem.InnerText); 
				}
				if(structureItem.Name == "Life")
				{
					dictionary.Add("Life",structureItem.InnerText); 
				}
				if(structureItem.Name == "XpAward")
				{
					dictionary.Add("XpAward",structureItem.InnerText); 
				}
				if(structureItem.Name == "Upgrades")
				{
					dictionary.Add("Upgrades",structureItem.InnerText); 
				}
				if(structureItem.Name == "UpRatio")
				{
					dictionary.Add("UpRatio",structureItem.InnerText); 
				}
			}
			
			structures.Add(dictionary);
		}
	}

	protected void GetWallsXML()//reads buildings XML
	{
		XmlDocument xmlDoc = new XmlDocument(); 
		xmlDoc.LoadXml(StructuresXML.text); 
		XmlNodeList structureList = xmlDoc.GetElementsByTagName(structureXMLTag);//"Wall"
		
		foreach (XmlNode structureInfo in structureList)
		{
			XmlNodeList structureContent = structureInfo.ChildNodes;	
			dictionary = new Dictionary<string, string>();
			
			foreach (XmlNode structureItem in structureContent) // levels itens nodes.
			{
			/*
			<Name>Stone Wall NE</Name>	
			<Currency>Gold</Currency>			
			<Price>200</Price>
			<TimeToBuild>0</TimeToBuild>
			<Life>200</Life>
			<XpAward>2</XpAward>
			<Upgrades>0</Upgrades>						
			<UpRatio>0</UpRatio>	
			*/
				if(structureItem.Name == "Name")
				{
					dictionary.Add("Name",structureItem.InnerText); // put this in the dictionary.
				}
				if(structureItem.Name == "Currency")
				{
					dictionary.Add("Currency",structureItem.InnerText);
				}
				if(structureItem.Name == "Price")
				{
					dictionary.Add("Price",structureItem.InnerText);
				}	
				if(structureItem.Name == "TimeToBuild")
				{
					dictionary.Add("TimeToBuild",structureItem.InnerText);
				}	
				if(structureItem.Name == "Life")
				{
					dictionary.Add("Life",structureItem.InnerText);
				}
				if(structureItem.Name == "XpAward")
				{
					dictionary.Add("XpAward",structureItem.InnerText);
				}
				if(structureItem.Name == "Upgrades")
				{
					dictionary.Add("Upgrades",structureItem.InnerText); 
				}
				if(structureItem.Name == "UpRatio")
				{
					dictionary.Add("UpRatio",structureItem.InnerText); 
				}
			}
			structures.Add(dictionary);
		}
	}

	/*
		
		*/

	protected void GetWeaponsXML()//reads buildings XML
	{
		XmlDocument xmlDoc = new XmlDocument(); 
		xmlDoc.LoadXml(StructuresXML.text); 
		XmlNodeList structureList = xmlDoc.GetElementsByTagName(structureXMLTag);//"Wall"

		foreach (XmlNode structureInfo in structureList)
		{
			XmlNodeList structureContent = structureInfo.ChildNodes;	
			dictionary = new Dictionary<string, string>();

			foreach (XmlNode structureItem in structureContent) // levels itens nodes.
			{
				/*
		<Name>Bomb</Name>		
		<Description>Explosive Device.</Description>	
									
		<Currency>Gold</Currency>					
		<Price>25</Price>												
				
		<TimeToBuild>5</TimeToBuild>				

		<DamagePerSecond>25</DamagePerSecond>
		<Life>200</Life>
		
		<Range>50</Range>
		<FireRate>2</FireRate>	
		<DamageType>Area</DamageType>				
		<TargetType>Ground</TargetType>				
		<PreferredTarget>Any</PreferredTarget>		
		<DamageBonus>0</DamageBonus>				

		<XpAward>100</XpAward>						
		<Upgrades>0</Upgrades>							
		<UpRatio>0</UpRatio>	
			*/
				if(structureItem.Name == "Name")
				{
					dictionary.Add("Name",structureItem.InnerText); // put this in the dictionary.
				}
				if(structureItem.Name == "Description")
				{
					dictionary.Add("Description",structureItem.InnerText); // put this in the dictionary.
				}
				if(structureItem.Name == "Currency")
				{
					dictionary.Add("Currency",structureItem.InnerText);
				}
				if(structureItem.Name == "Price")
				{
					dictionary.Add("Price",structureItem.InnerText);
				}	
				if(structureItem.Name == "TimeToBuild")
				{
					dictionary.Add("TimeToBuild",structureItem.InnerText);
				}	
				if(structureItem.Name == "DamagePerSecond")
				{
					dictionary.Add("DamagePerSecond",structureItem.InnerText);
				}
				if(structureItem.Name == "Life")
				{
					dictionary.Add("Life",structureItem.InnerText);
				}

				if(structureItem.Name == "Range")
				{
					dictionary.Add("Range",structureItem.InnerText);
				}

				if(structureItem.Name == "FireRate")
				{
					dictionary.Add("FireRate",structureItem.InnerText);
				}

				if(structureItem.Name == "DamageType")
				{
					dictionary.Add("DamageType",structureItem.InnerText);
				}
				if(structureItem.Name == "TargetType")
				{
					dictionary.Add("TargetType",structureItem.InnerText);
				}
				if(structureItem.Name == "PreferredTarget")
				{
					dictionary.Add("PreferredTarget",structureItem.InnerText);
				}

				if(structureItem.Name == "DamageBonus")
				{
					dictionary.Add("DamageBonus",structureItem.InnerText);
				}

				if(structureItem.Name == "XpAward")
				{
					dictionary.Add("XpAward",structureItem.InnerText); 
				}
				if(structureItem.Name == "Upgrades")
				{
					dictionary.Add("Upgrades",structureItem.InnerText); 
				}
				if(structureItem.Name == "UpRatio")
				{
					dictionary.Add("UpRatio",structureItem.InnerText); 
				}
			}
			structures.Add(dictionary);
		}
	}

	protected void GetAmbientXML()//reads buildings XML
	{
		XmlDocument xmlDoc = new XmlDocument(); 
		xmlDoc.LoadXml(StructuresXML.text); 
		XmlNodeList structureList = xmlDoc.GetElementsByTagName(structureXMLTag);//"Wall"

		foreach (XmlNode structureInfo in structureList)
		{
			XmlNodeList structureContent = structureInfo.ChildNodes;	
			dictionary = new Dictionary<string, string>();

			foreach (XmlNode structureItem in structureContent) // levels itens nodes.
			{
				/*
				<Name>Palm Tree A</Name>	
				<Description>Palm Tree description.</Description>								
				<Currency>Mana</Currency>						
				<Price>100</Price>	
				<TimeToBuild>10</TimeToBuild>								
				<XpAward>100</XpAward>	
				*/
				if(structureItem.Name == "Name")
				{
					dictionary.Add("Name",structureItem.InnerText); // put this in the dictionary.
				}
				if(structureItem.Name == "Description")
				{
					dictionary.Add("Description",structureItem.InnerText); // put this in the dictionary.
				}
				if(structureItem.Name == "Currency")
				{
					dictionary.Add("Currency",structureItem.InnerText);
				}
				if(structureItem.Name == "Price")
				{
					dictionary.Add("Price",structureItem.InnerText);
				}	
				if(structureItem.Name == "TimeToBuild")
				{
					dictionary.Add("TimeToBuild",structureItem.InnerText);
				}	
				if(structureItem.Name == "XpAward")
				{
					dictionary.Add("XpAward",structureItem.InnerText); 
				}

			}
			structures.Add(dictionary);
		}
	}

	protected void UpdatePrices()
	{	
		for (int i = 0; i < totalStructures; i++) {
			PriceLbs [i].text = structures [i] ["Price"];
		}
	}

	private void UpdateStructuresAllowed()//string structureType
	{
		//allowedStructures = new int[totalStructures];

		switch (structureXMLTag) 
		{
		case "Building":			
			allowedStructures = ((Stats)stats).maxBuildingsAllowed;
			break;
		case "Wall":
			allowedStructures = ((Stats)stats).maxWallsAllowed;
			break;	
		case "Weapon":
			allowedStructures = ((Stats)stats).maxWeaponsAllowed;
			break;	
		case "Ambient":
			allowedStructures = ((Stats)stats).maxAmbientsAllowed;
			break;	
		}		
	}

	public void UpdateButtons()
	{
		StartCoroutine("UpdateLabelStats");
	}

	protected IEnumerator UpdateLabelStats()
	{
		yield return new WaitForSeconds (xmlLoadDelay);

		UpdateStructuresAllowed ();
		//print ("updated " + structureXMLTag);
		Color 
		black = new Color (0, 0, 0),
		brown = new Color (0.45f, 0.09f, 0),
		red = new Color (1, 0, 0);

		bool buildingAllowed;	

		for (int i = 0; i < totalStructures; i++) 
		{
			//int index = i;

			//if(structureXMLTag=="Wall")
				//index = SelectionToXMLIndexWalls(i);

			buildingAllowed =(allowedStructures[i]-existingStructures[i]>0);

			NameLbs[i].text = structures [i] ["Name"];
			TimeLbs[i].text = structures [i] ["TimeToBuild"];

			QuantityLbs[i].text =  existingStructures[i].ToString()+"/"+allowedStructures[i].ToString();
			PriceLbs[i].text = structures [i] ["Price"];

			if(buildingAllowed)
			{
				NicheSprites[i].spriteName = "stone_niche";
				PortraitSprites[i].atlas = Portraits;

				NameLbs[i].color = brown;
				TimeLbs[i].color = brown;
				QuantityLbs[i].color = brown;
				PriceLbs[i].color = brown;
			}
			else
			{
				NicheSprites[i].spriteName = "stone_niche_bw";
				PortraitSprites[i].atlas = PortraitsBW;

				NameLbs[i].color = black;
				TimeLbs[i].color = black;
				QuantityLbs[i].color = black;
				PriceLbs[i].color = black;
			}	

			bool hasMoney = false;

			if (structures [i] ["Currency"] == "Gold")//refunds the gold/mana 
			{
				if(((Stats)stats).gold + ((Stats)stats).deltaGoldPlus - ((Stats)stats).deltaGoldMinus >= int.Parse(structures [i] ["Price"]))
				{
					hasMoney = true;
				}
			} 
			else if (structures [i] ["Currency"] == "Mana")
			{
				if(((Stats)stats).mana + ((Stats)stats).deltaManaPlus - ((Stats)stats).deltaManaMinus>=int.Parse(structures [i] ["Price"]))
				{
					hasMoney = true;
				}
			}
			else //if (structures [currentSelection] ["Currency"] == "Crystals")
			{
				if(((Stats)stats).crystals +((Stats)stats).deltaCrystalsPlus-((Stats)stats).deltaCrystalsMinus>=int.Parse(structures [i] ["Price"]))
				{
					hasMoney = true;
				}	
			}

			if(!hasMoney && buildingAllowed)
			{
				((UILabel)PriceLbs[i]).color = red;
			}
		}
	}





	//	xy
	public void MoveNW(){Move(0);}	//	-+
	public void MoveNE(){Move(1);}	//	++
	public void MoveSE(){Move(2);}	//	+-
	public void MoveSW(){Move(3);}	//	--


	protected void MovingPadOn()//move pad activated and translated into position
	{		
		MovingPad.SetActive (true);
						
		selectedStructure.transform.parent = MovingPad.transform;
		selectedGrass.transform.parent = MovingPad.transform;
			
		if (isReselect) 
		{
			selectedGrass.transform.position = new Vector3 (selectedGrass.transform.position.x,
			                                                selectedGrass.transform.position.y,
			                                                selectedGrass.transform.position.z - 2.0f);//move to front 

			selectedStructure.transform.position = new Vector3 (selectedStructure.transform.position.x,
			                                                    selectedStructure.transform.position.y,
			                                                    selectedStructure.transform.position.z - 6);//move to front
			displacedonZ = true;
		}
		((CameraController)cameraController).movingBuilding = true;
		
	}

	protected void Move(int i)
	{
		if (((Relay)relay).pauseMovement || ((Relay)relay).delay) return;
		
		((SoundFX)soundFX).Move(); //128x64
		
		float stepX = (float)gridx / 2;
		float stepY = (float)gridy / 2;//cast float, otherwise  181/2 = 90, and this accumulats a position error;
		
		switch (i) 
		{
		case 0:
			MovingPad.transform.position += new Vector3(-stepX,stepY,0);		//NW	
			break;	
			
		case 1:
			MovingPad.transform.position += new Vector3(stepX,stepY,0);			//NE		
			break;
			
		case 2:
			MovingPad.transform.position += new Vector3(stepX,-stepY,0);		//SE		
			break;
			
		case 3:
			MovingPad.transform.position += new Vector3(-stepX,-stepY,0);		//SW		
			break;				
		}	
	}

	protected void MouseTouchMove()
	{
			touchMoveCounter += Time.deltaTime;
			
			if(touchMoveCounter > touchMoveTime)
			{
				touchMoveCounter=0;
				TouchMove();
				if(mouseFollow)
					MouseMove();
			}		
	}

	private void TouchMove()
	{
		if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)		        
		{
			Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
			
			if(touchDeltaPosition.x < 0)
			{
				if(touchDeltaPosition.y < 0)
				{
					MoveSW();
				}
				else if(touchDeltaPosition.y > 0)
				{
					MoveNW();
				}
			}
			else if(touchDeltaPosition.x > 0)
			{
				if(touchDeltaPosition.y < 0)
				{
					MoveSE();
					
				}
				else if(touchDeltaPosition.y > 0)
				{
					MoveNE();
				}
			}
		}
	}
	
	public void MouseMove()
	{
		GetMousePosition ();		
		Vector2 deltaPosition = mousePosition - new Vector2(selectedStructure.transform.position.x,
		                                                    selectedStructure.transform.position.y);
		
		if(Mathf.Abs(deltaPosition.x)>gridx||Mathf.Abs(deltaPosition.y)>gridy)
		{				
			if(deltaPosition.x < 0)
			{
				if(deltaPosition.y < 0)
				{
					MoveSW();
				}
				else if(deltaPosition.y > 0)
				{
					MoveNW();
				}
			}
			else if(deltaPosition.x > 0)
			{
				if(deltaPosition.y < 0)
				{
					MoveSE();
					
				}
				else if(deltaPosition.y > 0)
				{
					MoveNE();
				}
			}
		}
		
	}

	private void GetMousePosition() 
	{			
		Vector3 gridPos = new Vector3(0,0,0);
		
		// Generate a plane that intersects the transform's position with an upwards normal.
		Plane playerPlane = new Plane(Vector3.back, new Vector3(0, 0, 0));//transform.position + 
		
		// Generate a ray from the cursor position
		
		Ray RayCast;
		
		RayCast = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		// Determine the point where the cursor ray intersects the plane.
		float HitDist = 0;
		
		// If the ray is parallel to the plane, Raycast will return false.
		if (playerPlane.Raycast(RayCast, out HitDist))//playerPlane.Raycast
		{
			// Get the point along the ray that hits the calculated distance.
			Vector3 RayHitPoint = RayCast.GetPoint(HitDist);
			
			int indexCell = GridManager.instance.GetGridIndex(RayHitPoint);
			
			int col = GridManager.instance.GetColumn(indexCell);
			int row = GridManager.instance.GetRow(indexCell);
			
			gridPos = GridManager.instance.nodes[row,col].position;						
		}
		mousePosition = gridPos;	
	}

	public void AdjustStructureZ(int pivotIndex, int spriteIndex)
	{
		Vector3 pivotPos = selectedStructure.transform.GetChild (pivotIndex).position; //pivot
		Vector3 spritesPos = selectedStructure.transform.GetChild (spriteIndex).position;//sprites
		//Vector3 pos = selectedBuilding.transform.position;
		
		float correctiony = 10 / (pivotPos.y + 3300);//ex: fg 10 = 0.1   bg 20 = 0.05  
		//all y values must be positive, so we add the grid origin y 3207 +100 to avoid divide by 0; 
		//otherwise depth glitches around y 0
		
		selectedStructure.transform.GetChild(spriteIndex).position = new Vector3(spritesPos.x, spritesPos.y, zeroZ - correctiony);//	transform.GetChild(2).position   
		
	}

	protected void InstantiateStructure()//instantiates the building and grass prefabs
	{
		if(isInstant[currentSelection]==0)
		((Stats)stats).occupiedDobbits++;	//get one dobbit

		((Stats)stats).UpdateUI();			//to reflect the free/total dobbit ratio

		((Relay)relay).pauseInput = true;	//pause all other input - the user starts moving the building

		if (pivotCorrections [currentSelection] == 1)
			pivotCorrection = true;
		else
			pivotCorrection = false;			//used to flag necessary correction so all structures are aligned on the grid square

		GameObject structure = (GameObject)Instantiate(structurePf[currentSelection], new Vector3(0,0,zeroZ), Quaternion.identity);	
		GameObject grass = (GameObject)Instantiate(grassPf[grassTypes [currentSelection]], new Vector3(0,0,grassZ), Quaternion.identity);

		selectedStructure = structure;
		selectedGrass = grass;

		SelectStructure();

	}

	protected void SelectStructure() //after the grass/building prefabs are instantiated, they must be selected from the existing structures on the map
	{				
		StructureControlMenu.SetActive (true);// the move/upgrade/place/cancel, at the bottom of the screen

		selectedStructure.GetComponent<StructureSelector> ().grassType =
			selectedGrass.GetComponent<GrassSelector> ().grassType;
		
		int posX, posY;

		if(!isField)				
		{
			posX = (int) (Scope.transform.position.x-		//calculates the middle of the screen - the Scope position,
				Scope.transform.position.x%gridx); //and adjusts it to match the grid; the dummy is attached to the 2DToolkit camera
			posY = (int)(Scope.transform.position.y-
				Scope.transform.position.y%gridy);
		}
		else
		{
			posX = (int) spawnPointList[currentFieldIndex].x;
			posY = (int) spawnPointList[currentFieldIndex].y;
		}

		MovingPad.SetActive(true);//activates the arrow move platform					
		if(pivotCorrection)					
		{
			selectedStructure.transform.position = new Vector3(posX+gridx/2, posY, zeroZ-6);	//moves the building to position				
			selectedGrass.transform.position = new Vector3(posX+gridx/2, posY, grassZ-2);		//grass
			MovingPad.transform.position = new Vector3(posX+gridx/2, posY, padZ);				//move pad
		}

		else
		{
			selectedStructure.transform.position = new Vector3(posX, posY, zeroZ-6);			//the building must appear in front				
			selectedGrass.transform.position = new Vector3(posX, posY, grassZ-2);	
			MovingPad.transform.position = new Vector3(posX, posY, padZ);
		}

		if (!isField)
		{
		selectedStructure.transform.parent = MovingPad.transform;		//parents the selected building to the arrow moving platform
		selectedGrass.transform.parent = MovingPad.transform;			//parents the grass to the move platform
		}

		((Relay)relay).pauseInput = true;								//pause other input so the user can move the building	
		((CameraController)cameraController).movingBuilding = true;
			
	}

	public void PlaceStructure()
	{
		Vector3 
		grassPos = selectedGrass.transform.position,
		structurePos = selectedStructure.transform.position;

		if (!isReselect) {			
			((Stats)stats).structureIndex++;
			structureIndex = ((Stats)stats).structureIndex;//unique number for harvesting

			StructureSelector sSel = selectedStructure.GetComponent<StructureSelector> ();
			GrassSelector gSel = selectedGrass.GetComponent<GrassSelector> ();

			if (gridBased)
				RegisterGridPosition (sSel);

			sSel.structureIndex = structureIndex;
			((GrassSelector)selectedGrass.GetComponent ("GrassSelector")).grassIndex = structureIndex;//grassIndex and structureIex are paired
 
			//instantiates the construction prefab and pass the relevant info;
			if (isInstant [currentSelection] == 0) {
				GameObject Construction = (GameObject)Instantiate (constructionPf [constructionTypes [currentSelection]], 
					                          new Vector3 (structurePos.x, structurePos.y, structurePos.z + 6), 
					                          Quaternion.identity);
			
				selectedConstruction = Construction;
				ConstructionSelector cSel = selectedConstruction.GetComponent <ConstructionSelector> ();
				cSel.constructionIndex = structureIndex;
				cSel.buildingTime =	int.Parse (structures [currentSelection] ["TimeToBuild"]);				
				cSel.structureType = sSel.structureType;				
				cSel.grassType = gSel.grassType;


				cSel.ParentGroup = ParentGroup;			
				cSel.structureClass = structureClass;

				if (structureXMLTag == "Building") {					
					cSel.storageAdd = int.Parse (structures [currentSelection] ["StoreCap"]);					
				}
			}

			if (gridBased) {
				ConstructionSelector cSel = selectedConstruction.GetComponent <ConstructionSelector> ();
				cSel.iRow = ((StructureSelector)sSel).iRow;
				cSel.jCol = ((StructureSelector)sSel).jCol;
			}
		} 
		else 
		{
			selectedStructure.GetComponent<StructureSelector> ().DeSelect ();	
		}

		//((StructureSelector)selectedStructure.GetComponent("StructureSelector")).isSelected = false;
		//((GrassSelector)selectedGrass.GetComponent("GrassSelector")).isSelected = false;

		((GrassCollider)selectedGrass.GetComponentInChildren<GrassCollider>()).isMoving = false;		
		selectedGrass.GetComponentInChildren<GrassCollider>().enabled = false;		

		//--> Reselect
		if(!isReselect)
		{
			if (isInstant [currentSelection] == 0) {
				selectedConstruction.transform.parent = ParentGroup.transform;
				selectedGrass.transform.parent = selectedConstruction.transform;
			} else 
			{
				selectedGrass.transform.parent = selectedStructure.transform;
			}

			selectedGrass.transform.position = new Vector3 (grassPos.x,	grassPos.y,	grassPos.z + 2);//cancel the instantiation z correction
			selectedStructure.transform.position = new Vector3(structurePos.x, structurePos.y, structurePos.z+6); //6

			if (isInstant [currentSelection] == 0) {
				selectedStructure.transform.parent = selectedConstruction.transform;
				selectedStructure.SetActive (false);
				AdjustConstructionZ ();
			} else 
			{
				selectedStructure.transform.parent = ParentGroup.transform;
			}
		}
		else if(displacedonZ)
		{			

			//send the structures 6 z unit to the background
			selectedGrass.transform.position = new Vector3 (grassPos.x,	grassPos.y,	grassPos.z + 2);//move to back
			selectedStructure.transform.position = new Vector3(structurePos.x, structurePos.y, structurePos.z+6); //6

			selectedStructure.transform.parent = ParentGroup.transform;
			selectedGrass.transform.parent = selectedStructure.transform;	
			displacedonZ = false;
		}

		AdjustStructureZ (1,2);

		MovingPad.SetActive(false);
		((CameraController)cameraController).movingBuilding = false;
		StructureControlMenu.SetActive (false);
		//isReselect = false;
		StartCoroutine ("Deselect");
		//((Relay)relay).pauseInput = false;
		((MenuMain)menuMain).waitForPlacement = false;
		Delay ();			//delay and pause input = two different things 

	}	

	public void PlaceStructureGridInstant()
	{
		if (!isReselect) 
		{
			((Stats)stats).structureIndex++;
			structureIndex = ((Stats)stats).structureIndex;//unique number for pairing the buildings and the grass patches underneath

			((StructureSelector)selectedStructure.GetComponent ("StructureSelector")).structureIndex = structureIndex;
			((GrassSelector)selectedGrass.GetComponent ("GrassSelector")).grassIndex = structureIndex;
			PutBackInPlace ();
		} 
		else if (displacedonZ) 
		{
			PutBackInPlace ();
			displacedonZ = false;
		}

		selectedStructure.transform.parent = ParentGroup.transform;
		selectedGrass.transform.parent = selectedStructure.transform;	

		StructureSelector sSel = selectedStructure.GetComponent<StructureSelector>();

		//((StructureSelector)sSel).isSelected = false;

		RegisterGridPosition (sSel);

		((GrassCollider)selectedGrass.GetComponentInChildren<GrassCollider>()).isMoving = false;		
		selectedGrass.GetComponentInChildren<GrassCollider>().enabled = false;		

		AdjustStructureZ (1, 2);
		MovingPad.SetActive(false);
		((CameraController)cameraController).movingBuilding = false;
		StructureControlMenu.SetActive (false);
		isReselect = false;
		StartCoroutine ("Deselect");
		//((Relay)relay).pauseInput = false;
		((MenuMain)menuMain).waitForPlacement = false;
		Delay ();			//delay and pause input = two different things 

	}	

	private void PutBackInPlace()
	{
		selectedGrass.transform.position = new Vector3 (selectedGrass.transform.position.x,
			selectedGrass.transform.position.y,
			selectedGrass.transform.position.z + 2);//move to back

		selectedStructure.transform.position = new Vector3(selectedStructure.transform.position.x, 
			selectedStructure.transform.position.y, 
			selectedStructure.transform.position.z+6); //6		
	}
	private void RegisterGridPosition(StructureSelector sSel)
	{
		int indexCell = GridManager.instance.GetGridIndex(selectedStructure.transform.position);

		int row = GridManager.instance.GetRow(indexCell);
		int col = GridManager.instance.GetColumn(indexCell);

		sSel.iRow = row;
		sSel.jCol = col;
	}

	public void CancelObject()//cancel construction, or reselect building and destroy/cancel
	{	
		if (!isReselect) 
		{
			((Stats)stats).occupiedDobbits--;//frees the dobbit

			if (structures [currentSelection] ["Currency"] == "Gold")//refunds the gold/mana 
			{
				//((Stats)stats).gold += int.Parse (structures [currentSelection] ["Price"]);	
				((Stats)stats).AddResources (int.Parse (structures [currentSelection] ["Price"]),0,0);
			} 
			else if (structures [currentSelection] ["Currency"] == "Mana")
			{
				//((Stats)stats).mana += int.Parse (structures [currentSelection] ["Price"]);	
				((Stats)stats).AddResources (0, int.Parse (structures [currentSelection] ["Price"]), 0);
			}
			else //if (structures [currentSelection] ["Currency"] == "Crystals")
			{
				//((Stats)stats).crystals += int.Parse (structures [currentSelection] ["Price"]);	
				((Stats)stats).AddResources (0, 0, int.Parse (structures [currentSelection] ["Price"]));
			}

			((Stats)stats).ApplyMaxCaps();

		}

		if(structures [currentSelection] ["StoreType"]!="None")
		{
			DecreaseStorage(structures [currentSelection] ["StoreType"], 
				int.Parse (structures [currentSelection] ["StoreCap"]));
		}

		((Stats)stats).experience -= int.Parse (structures [currentSelection] ["XpAward"]);
		((Stats)stats).maxHousing -= int.Parse (structures [currentSelection] ["PopCap"]);

		Destroy(selectedStructure);
		UpdateExistingStructures (-1);//decreases the array which counts how many structures of each type you have 

		Destroy(selectedGrass);

		MovingPad.SetActive(false);					//deactivates the arrow building moving platform
		StructureControlMenu.SetActive (false);	//deactivates the buttons move/upgrade/place/cancel, at the bottom of the screen
		((Relay)relay).pauseInput = false;			//while the building is selected, pressing other buttons has no effect
		((Relay)relay).pauseMovement = false;		//the confirmation screen is closed
		if(isReselect){ isReselect = false;}		//end the reselect state	
		((Stats)stats).UpdateUI ();
	}

	private void DecreaseStorage(string resType, int value)//when a building is reselected and destroyed, the gold/mana storage capacity decrease; 
	{
		if(resType=="Gold")
		{
			((Stats)stats).maxGold -= value;//the destroyed building storage cap

		}
		else if (resType=="Mana") 
		{
			((Stats)stats).maxMana -= value;

		}	
		else if (resType=="Dual") //gold+mana
		{
			((Stats)stats).maxGold -= value;
			((Stats)stats).maxMana -= value;		
		}

		((Stats)stats).UpdateUI();//updates the interface numbers
	}

	//  verifies if the building can be constructed:
	//  exceeds max number of structures / enough gold/mana/free dobbits to build?
	//  pays the price to Stats; updates the Stats interface numbers
	protected void VerifyConditions()
	{
		//string type = buildingTags[currentSelection];
		int maxAllowed = 0;

		UpdateStructuresAllowed ();//structureXMLTag
		maxAllowed = allowedStructures[currentSelection];

		bool canBuild = true;//must pass as true through all verifications

		//max allowed structures ok?
		if (existingStructures[currentSelection] >= maxAllowed)//max already reached
		{					
			canBuild = false;
			((Messenger)statusMsg).DisplayMessage("Maximum " + maxAllowed.ToString() + 
				" structures of type " +
				structures [currentSelection] ["Name"]+". " +
				"Upgrade Summoning Circle to build more."); //displays the hint - you can have only 3 structures of this type
		}

		//make sure these xml tags are common - price, currency
		/*
		int xmlIndex = 0;
		if (structureXMLTag == "Wall") {
			xmlIndex = SelectionToXMLIndexWalls (currentSelection);	
		} else {
			xmlIndex = currentSelection;
		}
	*/
		int price = int.Parse (structures [currentSelection] ["Price"]);

		//enough gold?
		if (structures [currentSelection] ["Currency"] == "Gold") //this needs gold
		{
			int existingGold = ((Stats)stats).gold + ((Stats)stats).deltaGoldPlus - ((Stats)stats).deltaGoldMinus;

			if (existingGold < price) 
			{
				canBuild = false;
				((Messenger)statusMsg).DisplayMessage("Not enough gold.");//updates hint text
			}
		} 
		else  if (structures [currentSelection] ["Currency"] == "Mana")
		{
			int existingMana = ((Stats)stats).mana + ((Stats)stats).deltaManaPlus - ((Stats)stats).deltaManaMinus;

			if(existingMana < price)
			{
				canBuild = false;
				((Messenger)statusMsg).DisplayMessage("Not enough mana.");//updates hint text
			}
		}
		else  //if (structures [currentSelection] ["Currency"] == "Crystals")
		{
			int existingCrystals = ((Stats)stats).crystals + ((Stats)stats).deltaCrystalsPlus - ((Stats)stats).deltaCrystalsMinus;

			if(existingCrystals < price)
			{
				canBuild = false;
				((Messenger)statusMsg).DisplayMessage("Not enough crystals.");//updates hint text
			}
		}
		if (((Stats)stats).occupiedDobbits >= ((Stats)stats).dobbits) //dobbit available?
		{
			canBuild = false;
			((Messenger)statusMsg).DisplayMessage("You need more dobbits.");
		}

		if (canBuild) 
		{
			((MenuMain)menuMain).constructionGreenlit = true;//ready to close menus and place the building; 
			//constructionGreenlit bool necessary because the command is sent by pressing the button anyway

			((Stats)stats).experience += int.Parse (structures [currentSelection] ["XpAward"]); //incre	ases Stats experience  // move this to building finished 

			if(((Stats)stats).experience>((Stats)stats).maxExperience)
			{
				((Stats)stats).level++;
				((Stats)stats).experience-=((Stats)stats).maxExperience;
				((Stats)stats).maxExperience+=100;
			}

			//pays the gold/mana price to Stats
			if(structures [currentSelection] ["Currency"] == "Gold")
			{
				//((Stats)stats).gold -= price; 
				((Stats)stats).SubstractResources (price, 0, 0);

			}
			else if(structures [currentSelection] ["Currency"] == "Mana")
			{
				//((Stats)stats).mana -= price;
				((Stats)stats).SubstractResources (0, price, 0);
			}
			else //if(structures [currentSelection] ["Currency"] == "Crystals")
			{
				//((Stats)stats).crystals -= price;
				((Stats)stats).SubstractResources (0, 0, price);

			}

			UpdateButtons ();//payments are made, update all

			((Stats)stats).UpdateUI();//tells stats to update the interface - otherwise new numbers are updated but not displayed

			if (!isField || buildingFields) 
			{
				UpdateExistingStructures (+1);//an array that keeps track of how many structures of each type exist
				InstantiateStructure ();
			}
		} 
		else 
		{
			((MenuMain)menuMain).constructionGreenlit = false;//halts construction - the button message is sent anyway, but ignored
		}


	}

	public void ReloadExistingStructures(int index)
	{
		currentSelection = index;
		UpdateExistingStructures (1);
	}


	private void UpdateExistingStructures(int value)// +1 or -1
	{
		/*
		if you are allowed to have 50 wals/50 wooden fences, you can build any type, and the number 50 is decreased as a hole		
		*/
		switch (structureXMLTag) 
		{
		case "Building":
			existingStructures [currentSelection] += value;
			break;

		case "Wall":
			//stone walls and wood fences are considered 2 different types, each type can have 50 pieces of any kind at level 1
			if (currentSelection < 3) {
				for (int i = 0; i < 3; i++) {	
					existingStructures [i] += value;
				}
			} else 
			{
				for (int i = 3; i < existingStructures.Length; i++) {			
					existingStructures [i] += value;
				}
			}
			break;	

		case "Weapon":
			existingStructures [currentSelection] += value;
			break;
		case "Ambient":
			existingStructures [currentSelection] += value;
			break;
		}	
	}

	public void ConstructionFinished(string constructionType)//called by construction selector finish sequence
	{
		switch (constructionType) {  // move this to building finished 

		case "Building":
			/*
			<StoreType>None</StoreType>					<!-- resource stored - none/gold/mana/dual/soldiers-->	
			<StoreCap>0</StoreCap>						<!-- gold/mana/dual/soldiers storage -->			
			*/
			((Stats)stats).occupiedDobbits--;									//the dobbit previously assigned becomes available


			int structureTypeIndex = BuildingTypeToIndex (constructionType);
			int storeCapIncrease = int.Parse (structures [structureTypeIndex] ["StoreCap"]);

			if (structures [structureTypeIndex] ["StoreType"] == "None") {}			//get rid of the none types, most buildings store nothing
			else if (structures [structureTypeIndex] ["StoreType"] == "Soldiers")
				((Stats)stats).maxHousing += storeCapIncrease; 
			else if (structures [structureTypeIndex] ["StoreType"] == "Gold")
				((Stats)stats).maxGold += storeCapIncrease; 
			else if (structures [structureTypeIndex] ["StoreType"] == "Mana")
				((Stats)stats).maxMana += storeCapIncrease; 
			else if (structures [structureTypeIndex] ["StoreType"] == "Dual") {
				((Stats)stats).maxGold += storeCapIncrease;
				((Stats)stats).maxMana += storeCapIncrease; }
			break;

		default:
			break;
		}
		
	}
	private int BuildingTypeToIndex(string constructionType)
	{
		int structureTypeIndex = 0;
		switch (constructionType) 
		{


		default:
			break;
		}

		return structureTypeIndex;
	}

	/*
	public void ActivateStatsPad()//displays the small stats window
	{
		StatsPad.SetActive (true);

		StatsName.text = structures [currentSelection] ["Name"];
		StatsDescription.text = structures [currentSelection] ["Description"];

		ProductionLabel.SetActive (false);
		StatsCoin.SetActive (false);
		StatsMana.SetActive (false);

		if (structures [currentSelection] ["Name"] == "Gold Forge") 
		{
			ProductionLabel.SetActive (true);
			StatsCoin.SetActive (true);
			StatsGoldProduction.text = structures [currentSelection] ["ProdPerSec"];
		} 
		else if (structures [currentSelection] ["Name"] == "Mana Generator") 
		{
			ProductionLabel.SetActive (true);
			StatsMana.SetActive (true);
			StatsManaProduction.text = structures [currentSelection] ["ProdPerSec"];
		}

		StatsPad.transform.position = selectedStructure.transform.position;
	}
	*/
	/*
	public void DeactivateStatsPad()
	{
		StatsPad.SetActive (false);
	}

	public void DeactivateStatsPadSound()
	{
		((SoundFX)soundFX).Close();
		StatsPad.SetActive (false);
	}

	*/

	//receive a Tk2d button message to select an existing building; the button is in the middle of each building prefab and is invisible 

	public void OnReselect(GameObject structure, GameObject grass, string structureType)//string defenseType, 
	{		
		selectedStructure = structure;
		selectedGrass = grass;
		GetCurrentSelection (structureType);
		ReselectStructure ();
	}

	private void GetCurrentSelection(string structureType)
	{
		//{"Academy","Barrel","Chessboard","Classroom","Forge","Generator","Globe","Summon","Toolhouse","Vault","Workshop"};
		switch (structureType) 
		{
		case "Toolhouse": 	currentSelection = 0; 	break;
		case "Forge": 		currentSelection = 1;	break;
		case "Generator":	currentSelection = 2;	break;
		case "Vault":		currentSelection = 3;	break;
		case "Barrel":		currentSelection = 4;	break;
		case "Summon":		currentSelection = 5;	break;
		case "Academy":		currentSelection = 6;	break;
		case "Classroom":	currentSelection = 7;	break;
		case "Chessboard":	currentSelection = 8;	break;		
		case "Globe":		currentSelection = 9;	break;		
		case "Workshop":	currentSelection = 10;	break;
		case "Tatami":		currentSelection = 11;	break;
		}
	}

	private void ReselectStructure()//GameObject gameObj
	{	
		StructureControlMenu.SetActive (true);

		((Relay)relay).pauseInput = true;

		MovingPad.transform.position = 
			new Vector3(selectedStructure.transform.position.x,
				selectedStructure.transform.position.y, padZ);
			
		selectedGrass.GetComponentInChildren<GrassCollider>().enabled = true;
		((GrassCollider)selectedGrass.GetComponentInChildren<GrassCollider>()).isMoving = true;
	}

	public void Cancel()
	{
		CancelObject();
		Delay ();
	}

	public void OK()
	{
		if (((Relay)relay).currentAlphaTween != null) 
		{			

			if (((Relay)relay).currentAlphaTween.inTransition)			//force fade even if in transition
				((Relay)relay).currentAlphaTween.CancelTransition ();

			((Relay)relay).currentAlphaTween.FadeAlpha (false, 1);
			((Relay)relay).currentAlphaTween = null;
		}



		Delay ();

		inCollision = selectedGrass.GetComponentInChildren<GrassCollider>().inCollision;

		if (!inCollision) 
		{
			//if(isReselect){	DeactivateStatsPad();}

			if(allInstant)			
			PlaceStructureGridInstant ();			
			else
			PlaceStructure ();					
		}

		((Relay)relay).pauseMovement = false;	//the confirmation screen is closed
	}

	private IEnumerator Deselect()
	{
		yield return new WaitForSeconds(0.3f);
		isReselect = false;
		((Relay)relay).pauseInput = false;		//main menu butons work again
	}

	private void AdjustConstructionZ()
	{
		Vector3 pivotPos = selectedConstruction.transform.GetChild (1).position; //pivot
		Vector3 pos = selectedConstruction.transform.GetChild (3).position;//sprites
		//Vector3 pos = selectedStructure.transform.position;

		float correctiony = 10 / (pivotPos.y + 3300);//ex: fg 10 = 0.1   bg 20 = 0.05  
		//all y values must be positive, so we add the grid origin y 3207 +100 to avoid divide by 0; 
		//otherwise depth glitches around y 0

		selectedConstruction.transform.GetChild(3).position = new Vector3(pos.x, pos.y, zeroZ - correctiony);
	}

	public void ActivateMovingPad()//move pad activated and translated into position
	{
		if (!MovingPad.activeSelf) 
		{
			//DeactivateStatsPad ();
			MovingPadOn();
		}
	}

	/*

	This section deals with walls and fences, and the structure is a bit special. The XMl has 5 types:	
	StoneCorner, StoneWall, WoodenCorner,WoodenFence, WoodenEnd
	They all have different prices and life(hitpoints)
	This will allow you to build arrays of different elements, and complex fortifications, if you ever need to
	I hope I will have time to include arrays of identical elements as well (I will probably do this with stone walls)

	*/


	/*
	private int SelectionToXMLIndexWalls(int i)
	{
		int xmlIndex =0;

		switch (i) {
		//stone tower
		case 0:	xmlIndex = 0; break;

			//stone walls
		case 1: xmlIndex = 1; break;			
		case 2:	xmlIndex = 1; break;

			//wooden fence corners
		case 3:	xmlIndex = 2; break;
		case 4:	xmlIndex = 2; break;
		case 5:	xmlIndex = 2; break;
		case 6:	xmlIndex = 2; break;

			//wooden fences
		case 7:	xmlIndex = 3; break;
		case 8:	xmlIndex = 3; break;

			//wooden fence ends
		case 9: xmlIndex = 4; break;
		case 10: xmlIndex = 4; break;
		case 11: xmlIndex = 4; break;
		case 12: xmlIndex = 4; break;

		}
		return xmlIndex;
	}
	*/

	private void RecordSpawnPoint() 
	{				
		Vector3 gridPos = new Vector3(0,0,0);

		// Generate a plane that intersects the transform's position with an upwards normal.
		Plane playerPlane = new Plane(Vector3.back, new Vector3(0, 0, 0));//transform.position + 

		// Generate a ray from the cursor position

		Ray RayCast;

		if (Input.touchCount > 0)
			RayCast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
		else
			RayCast = Camera.main.ScreenPointToRay(Input.mousePosition);

		// Determine the point where the cursor ray intersects the plane.
		float HitDist = 0;

		// If the ray is parallel to the plane, Raycast will return false.
		if (playerPlane.Raycast(RayCast, out HitDist))//playerPlane.Raycast
		{
			// Get the point along the ray that hits the calculated distance.
			Vector3 RayHitPoint = RayCast.GetPoint(HitDist);

			int indexCell = GridManager.instance.GetGridIndex(RayHitPoint);

			int col = GridManager.instance.GetColumn(indexCell);
			int row = GridManager.instance.GetRow(indexCell);


			if(!GridManager.instance.nodes[row,col].isObstacle)
			{
				if(!startField)
				{
					if(!FieldFootstep.activeSelf)
					{
						FieldFootstep.SetActive (true);
						FieldFootstep.GetComponentInChildren<GrassCollider>().collisionCounter=0;
						FieldFootstep.GetComponentInChildren<GrassCollider> ().inCollision = false;
					}

					gridPos = GridManager.instance.nodes[row,col].position;//17x19
					startPosition = gridPos;
					startCell = indexCell;
					startCol = col; startRow = row;
					StartCoroutine(CreateStar (gridPos,row,col));
					startField = true;
				}
				else if(!endField)
				{
					gridPos = GridManager.instance.nodes[row,col].position;

					//don't overlapp || not on the same row/column
					if((gridPos == startPosition)||(startCol!=col)&&(startRow!=row))			
						return;
					
					startCol = 0; startRow = 0;
					endCell = indexCell;
					CreateStarArray();
					starSequencer += 0.1f;
					StartCoroutine(CreateStar (gridPos,row,col));
					endField = true;
				}
			}			
		}
	}
	private void CreateStarArray()
	{
		int startRow, startCol, endRow, endCol, leftRow, leftCol, rightRow, rightCol, 
		highCell, lowCell,
		highRow, highCol, lowRow, lowCol;//algorithm draws up --> down

		Vector3 gridPos = new Vector3(0,0,0);

		if (startCell > endCell) 
		{
			highCell = startCell;
			lowCell = endCell;
		}
		else
		{
			highCell = endCell;
			lowCell = startCell;
		}

		startRow = GridManager.instance.GetRow(highCell);
		startCol = GridManager.instance.GetColumn(highCell);

		endRow = GridManager.instance.GetRow(lowCell);
		endCol = GridManager.instance.GetColumn(lowCell);

		leftRow = startRow; leftCol = endCol; rightRow = endRow; rightCol = startCol;

		if (leftRow >= rightRow) { highRow=leftRow; lowRow=rightRow; }
		else { highRow=rightRow; lowRow=leftRow; }

		if (leftCol >= rightCol) { highCol=leftCol; lowCol=rightCol; }
		else { highCol=rightCol; lowCol=leftCol; }

		for (int i = highRow; i >= lowRow; i--) 
		{
			for (int j = lowCol; j <= highCol; j++) 
			{
				gridPos = GridManager.instance.nodes[i,j].position;
				if((i!=startRow||j!=startCol)&&(i!=endRow||j!=endCol))//&&(i!=endRow||j!=endCol) second exclusion not necessary
				{
					starSequencer += 0.1f;//0.1f
					StartCoroutine(CreateStar (gridPos, i, j));
				}
			}
		}

		StartCoroutine(LateOnCreateFields());
		//FieldSelectedPanel.SetActive (true);

		//gridPos = GridManager.instance.nodes[row,col].position;
	}
	private IEnumerator LateOnCreateFields()
	{
		yield return new WaitForSeconds (starSequencer+0.2f);//0.2f
		OnCreateFields ();
	}

	private IEnumerator CreateStar(Vector3 gridPos, int iRow, int jCol)
	{

		yield return new WaitForSeconds (starSequencer);
		//gridPos = GridManager.instance.nodes[i,j].position;
		FieldFootstep.transform.position = new Vector3(gridPos.x,gridPos.y,fieldDetectorZ);

		//yield return new WaitForSeconds (0.05f);//0.05f

		//bool fieldCollision = FieldFootstep.GetComponentInChildren<GrassCollider>().inCollision;

		//if (fieldCollision)
		//{
			//ResetFootstepPosition ();
			//FieldFootstep.GetComponentInChildren<GrassCollider>().inCollision = false;			
			//return false;
		//}

		GameObject Star = (GameObject)Instantiate (spawnPointStar, gridPos, Quaternion.identity);
		spawnPointList.Add(gridPos);

		starList.Add(Star);
		Component sSel = Star.GetComponent<Selector>();
		((Selector)sSel).iRow = iRow;
		((Selector)sSel).jCol = jCol;
	}

	public void OnCreateFields(){CreateFields (); }
	public void OnCloseFields(){CloseFields ();	}

	private void CreateFields()
	{
		buildingFields = true;

		for (int i = 0; i < starList.Count; i++) 
		{
			currentFieldIndex = i;
			OnFieldBuild();
		}
	
		DestroyStars ();
		CloseFields ();
	}


	private void OnFieldBuild()
	{		
		Verify ();

		if(((MenuMain)menuMain).constructionGreenlit)
		OK ();
	}
	private void CloseFields()
	{
		buildingFields = false;
		isField = false;
		drawingField = false;
		if (startField||endField) 
		{
			DestroyStars();
		}
		((Relay)relay).pauseInput = false;
		ResetFootstepPosition ();
		StartCoroutine("DeactivateFootstep");
	}
	private IEnumerator DeactivateFootstep()
	{
		yield return new WaitForSeconds (0.2f);
		FieldFootstep.SetActive (false);
	}

	private void ResetFootstepPosition()
	{
		Vector3 gridPos = GridManager.instance.nodes[2,2].position;
		FieldFootstep.transform.position = new Vector3(gridPos.x,gridPos.y,fieldDetectorZ);
	}


	private void DestroyStars()
	{
		for (int i = 0; i < starList.Count; i++) 
		{
			((Star)starList[i].GetComponent("Star")).die = true;
		}
		starList.Clear ();
		spawnPointList.Clear ();
		startField=false;
		endField = false;
		starSequencer = 0.2f;//0.2f
	}



} 