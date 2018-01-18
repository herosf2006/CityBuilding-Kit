using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.IO;

public class SaveLoadBase : MonoBehaviour {

	protected Node[,] nodes;

	protected const int 
		buildingTypesNo = 12,			//the building tags and prefabs
		grassTypesNo = 6,		
		removableGrassTypesNo = 3,		//single patch grass for removables tags and prefabs; they are auto random generated at load but not saved
		removableTypesNo = 7,			//the removable tags and prefabs
		wallTypesNo = 3,
		fenceTypesNo = 10,
		weaponTypesNo = 2,
		ambientTypesNo =16;

	protected int 
		zeroZ = 0,			//layer depths for building, correlate with BuildingCreator.cs
		grassZ = 2,			//layer depths for grass, correlate with BuildingCreator.cs
		cannonZ = -5,
		damageZ = -7;		

	public UIToggle soundToggle,ambientToggle,musicToggle;

	public GameObject[] 
	ConstructionPrefabs =  new GameObject[grassTypesNo],
	GrassPrefabs = new GameObject[grassTypesNo],//three types of grass patches
	BuildingPrefabs = new GameObject[buildingTypesNo],
	RemovablePrefabs = new GameObject[removableTypesNo],
	WallPrefabs = new GameObject[wallTypesNo],
	FencePrefabs = new GameObject[fenceTypesNo],
	WeaponPrefabs = new GameObject[weaponTypesNo],
	AmbientPrefabs = new GameObject[ambientTypesNo];

	protected GameObject[] Construction, Removables;//Grass, 

	protected List<GameObject> 
	StoneWalls = new List<GameObject>(), 
	WoodFences = new List<GameObject>(), 
	Weapons = new List<GameObject>(),
	Ambients = new List<GameObject>();

	protected List<EconomyBuilding> 
	existingEconomyBuildings = new List<EconomyBuilding>();

	protected GameObject 
		EconomyBuildings,
		GroupConstructions,
		GroupBuildings,//object used to parent the buildings, once they are instantiated
		GroupRemovables,
		GroupWalls,
		GroupAmbients,
		GroupWeapons,		
		GroupDamageBars,
		currentRemovable, 		
		currentStructure,
		currentGrass;	

	public GameObject 		
		RemovableTimerPf,
		UnitProc, 			//the data for units under construction is extracted from unitproc, 
							//that handles construction in a much simpler manner, having no graphics
		MenuUnit, 			//the object that holds the MenuUnit script; since it's disabled, must be linked manually;
		GhostHelper,
		BuildingCannon, 
		DamageBar ;

	//lists for these elements - unknown number of elements
	public List<GameObject> //make protected
		LoadedBuildings = new List<GameObject>(),
		LoadedConstructions = new List<GameObject>(),
		LoadedGrass = new List<GameObject>(),				//list is used to parent the grass to the buildings

		//LoadedCannons = new List<GameObject>(), 			//list is used to parent the cannons to the cannon group
		LoadedDamageBars = new List<GameObject>(),			//list is used to parent the damagebars to the damagebar group

		inRemovalList = new List<GameObject>();

		//removableList = new List<GameObject>();

	//protected List<GameObject[]> removableList = new List<GameObject[]>();


	protected DateTime saveDateTime, loadDateTime;//saveTime, currentTime
	protected TimeSpan timeDifference;

	//PlayerPrefs save variables
	protected string filePath;

	protected string savefile, currentLine;
	protected string[] savefileLines; 
	protected int lineIndex = 0;

	protected int[] existingBuildings = new int[buildingTypesNo];//the entire array is transfered to BuildingCreator.cs; 
	//records how many buildings of each type, when they are built/game is loaded

	protected bool //make protected
		isFileSave = true,
		isBattleMap = false;//true=filesave false=PlayerPrefs 

	//protected string filePath;

	protected StreamReader sReader;
	protected StreamWriter sWriter;

	protected Component 
		removableCreator, 
		statusMsg, soundFX, ghostHelper, 
		stats, transData,
		menuUnit, unitProc, resourceGenerator;//removableCreator, 

	public StructureCreator buildingCreator, wallCreator, weaponCreator, ambientCreator; 
	public MenuMain menuMain;

	protected void InitializeComponents()
	{
		/*
			print ("This version was released without being finished, and without the extensive three-month testing our previous versions went through." +
			"Features that are still in work:" +
			"\n- game-wide features like economy, upgrades and player progress;" +
			"\n- pathfinder and wall attacks (do not completely surround buildings with walls);" +
			"\n- since walls are not attacked, paths can be very long, which will create lag/low framerate;" +
			"\n- projectile trajectory/effects/sounds during battle are not set/rudimentary;" +
			"\n- numeric data processing, like loot/resource type on the attack map is inaccurate;" +
			"\n- minor glitches and imperfections;" +
			"\nHopefully, all these issues will be addressed in our next release.");
		*/

		GroupConstructions = GameObject.Find ("GroupConstructions");
		GroupBuildings = GameObject.Find ("GroupBuildings");
		GroupRemovables = GameObject.Find("GroupRemovables");
		GroupWalls = GameObject.Find("GroupWalls");
		GroupAmbients = GameObject.Find("GroupAmbients");
		GroupWeapons = GameObject.Find("GroupWeapons");

		//filePath = Application.persistentDataPath +"/";//iphone		

	//	buildingCreator = GameObject.Find ("BuildingCreator").GetComponent<StructureCreator> ();
		transData = GameObject.Find ("TransData").GetComponent<TransData> ();

		if(!isBattleMap)
		{
			EconomyBuildings = GameObject.Find ("EconomyBuildings");
			removableCreator = GameObject.Find ("RemovableCreator").GetComponent<RemovableCreator> ();

			menuUnit = MenuUnit.GetComponent("MenuUnit");//MenuUnit.sc - necessary when saving only
			unitProc = UnitProc.GetComponent("UnitProc");//UnitProc.sc script

			stats = GameObject.Find("Stats").GetComponent<Stats>();// Stats.cs for HUD data
			resourceGenerator = GameObject.Find ("ResourceGenerator").GetComponent<ResourceGenerator> ();
		}
		else
		{			
			GroupDamageBars = GameObject.Find ("GroupDamageBars");
		}

		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();				//console for user messages
		soundFX = GameObject.Find ("SoundFX").GetComponent<SoundFX> ();



		ghostHelper = GhostHelper.GetComponent("GhostHelper");

		loadDateTime = System.DateTime.Now;//current time		
		nodes = GridManager.instance.nodes;
	}

	protected void AdjustStructureZ(GameObject structure, int pivotIndex, int spriteIndex)
	{
		Vector3 pivotPos = structure.transform.GetChild (pivotIndex).position; //pivot
		Vector3 spritesPos = structure.transform.GetChild (spriteIndex).position;//sprites
		//Vector3 pos = selectedBuilding.transform.position;
		
		float correctiony = 10 / (pivotPos.y + 3300);//ex: fg 10 = 0.1   bg 20 = 0.05  
		//all y values must be positive, so we add the grid origin y 3207 +100 to avoid divide by 0; 
		//otherwise depth glitches around y 0
		
		structure.transform.GetChild(spriteIndex).position = new Vector3(spritesPos.x, spritesPos.y, zeroZ - correctiony);//	transform.GetChild(2).position   
		
	}

	protected void ReadNextLine()
	{
		if (isFileSave) 
		{
			currentLine = sReader.ReadLine();
		}
		else
		{
			currentLine = savefileLines[lineIndex];
			lineIndex++;
		}
	}

	protected void InstantiatePositionalStructures()
	{
		while (currentLine != "###GridStruct###") //Buildings - read till next header is found 
		{
			//Buildings: buildingType, buildingIndex, position.x, position.y
			ReadNextLine();

			if(currentLine != "###GridStruct###") //if next category reached, skip
			{
				string[] lineStructure = currentLine.Split(","[0]);
								
				string 	structureClass = lineStructure[0],
						structureType = lineStructure[1];

				int 	structureIndex = int.Parse(lineStructure[2]),
						grassType = int.Parse(lineStructure[3]);

				float 	posX= float.Parse(lineStructure[4]),
						posY= float.Parse(lineStructure[5]);

				int prefabIndex = -1;
				if (structureClass == "Building") {
					switch (structureType) { //based on tag
					//"Academy","Barrel","Chessboard","Classroom","Forge","Generator","Globe","Summon","Toolhouse","Vault","Workshop"					

					case "Toolhouse": 		prefabIndex = 0; break;		
					case "Forge": 			prefabIndex = 1; break;		
					case "Generator": 		prefabIndex = 2; break;	
					case "Vault": 			prefabIndex = 3; break;						
					case "Barrel": 			prefabIndex = 4; break;			
					case "Summon": 			prefabIndex = 5; break;	
					case "Academy":			prefabIndex = 6; break;					
					case "Classroom": 		prefabIndex = 7; break;							
					case "Chessboard": 		prefabIndex = 8; break;
					case "Globe": 			prefabIndex = 9; break;	
					case "Workshop": 		prefabIndex = 10; break;
					case "Tatami": 			prefabIndex = 11; break;
					}

					GameObject Building = (GameObject)Instantiate (BuildingPrefabs [prefabIndex], new Vector3 (posX, posY, zeroZ), Quaternion.identity);		
					GameObject Grass = (GameObject)Instantiate (GrassPrefabs [grassType], new Vector3 (posX, posY, grassZ), Quaternion.identity);

					if (isBattleMap) {
						//GameObject GrassCannon = (GameObject)Instantiate (BuildingCannon, new Vector3 (posX, posY, cannonZ), Quaternion.identity);
						GameObject GrassDamageBar = (GameObject)Instantiate (DamageBar, new Vector3 (posX, posY - 200, grassZ), Quaternion.identity);
						//ProcessCannon (GrassCannon, structureIndex);
						ProcessDamageBar (GrassDamageBar, structureIndex);
					} 
					else 
					{
						buildingCreator.ReloadExistingStructures (prefabIndex);
					}
					existingBuildings [prefabIndex]++;
					ProcessPositionalStructure (Building, Grass, structureClass, structureIndex, grassType);
				} 
				else if (structureClass == "Weapon") 
				{	
					switch (structureType) {					
					case "Cannon": 			prefabIndex = 0;	break;	
					case "ArcherTower":		prefabIndex = 1; break;
					case "Catapult": 		prefabIndex = 2; break;	
					case "DronePad": 		prefabIndex = 3; break;
					case "Bomb": 			prefabIndex = 4; break;	
					case "AirMine":			prefabIndex = 5; break;
					}

					GameObject Weapon = (GameObject)Instantiate (WeaponPrefabs [prefabIndex], new Vector3 (posX, posY, zeroZ), Quaternion.identity);
					GameObject Grass = (GameObject)Instantiate (GrassPrefabs [grassType], new Vector3 (posX, posY, grassZ), Quaternion.identity);

					if (isBattleMap) 
					{
						//GameObject GrassCannon = (GameObject)Instantiate (BuildingCannon, new Vector3 (posX, posY, cannonZ), Quaternion.identity);
						GameObject GrassDamageBar = (GameObject)Instantiate (DamageBar, new Vector3 (posX, posY - 200, grassZ), Quaternion.identity);
						//ProcessCannon (GrassCannon, structureIndex);
						ProcessDamageBar (GrassDamageBar, structureIndex);
					}
					else
						weaponCreator.ReloadExistingStructures (prefabIndex);
					
					ProcessPositionalStructure (Weapon, Grass, structureClass, structureIndex, grassType);
				}
			}
		}
	}

	protected void InstantiateGridStructures()
	{
		while (currentLine != "###Construction###") //Walls
		{
			//Construction: buildingType, constructionIndex, buildingTime, remainingTime, storageIncrease, position.x, position.y
			ReadNextLine();
			if (currentLine != "###Construction###") 
			{
				string[] lineStructure = currentLine.Split ("," [0]);//reads the line, values separated by ","

				string structureClass = lineStructure[0];
				string structureType = lineStructure[1];
				int structureIndex = int.Parse(lineStructure[2]);

				int iRow = int.Parse(lineStructure[3]);
				int jCol = int.Parse(lineStructure[4]);

				float posX = nodes[iRow,jCol].position.x;
				float posY = nodes[iRow,jCol].position.y;

				int prefabIndex = -1;

				if (structureClass == "StoneWall") {

					switch (structureType) 
					{					
					case "StoneTower":		prefabIndex = 0; break;
					case "StoneWallNE":		prefabIndex = 1; break;
					case "StoneWallNW":		prefabIndex = 2; break;
					}

					GameObject Wall = (GameObject)Instantiate(WallPrefabs[prefabIndex], new Vector3(posX,posY,zeroZ), Quaternion.identity);
					GameObject Grass = (GameObject)Instantiate(GrassPrefabs[1], new Vector3(posX,posY,grassZ), Quaternion.identity);

					if (isBattleMap) 
					{
						//GameObject GrassCannon = (GameObject)Instantiate (BuildingCannon, new Vector3 (posX, posY, cannonZ), Quaternion.identity);
						//GameObject GrassDamageBar = (GameObject)Instantiate (DamageBar, new Vector3 (posX, posY, grassZ), Quaternion.identity);
						//ProcessCannon (GrassCannon, structureIndex);
						//ProcessDamageBar (GrassDamageBar, structureIndex);
					}
					else
						wallCreator.ReloadExistingStructures (prefabIndex);

					ProcessGridBasedStructure (Wall, Grass, structureClass, structureType, structureIndex, iRow, jCol);
					//ProcessGridBasedStructure(GameObject structure, GameObject grass, string structureClass, string structureType, int structureIndex, int iRow, int jCol)/

				} 
				else if (structureClass == "WoodFence") 
				{
					switch (structureType) 
					{				
					case "WoodCornerN":		prefabIndex = 0; break;
					case "WoodCornerS":		prefabIndex = 1; break;
					case "WoodCornerE": 	prefabIndex = 2; break;
					case "WoodCornerW":		prefabIndex = 3; break;	
					case "WoodFenceNE":		prefabIndex = 4; break;
					case "WoodFenceNW":		prefabIndex = 5; break;
					case "WoodEndNE":		prefabIndex = 6; break;
					case "WoodEndNW":		prefabIndex = 7; break;
					case "WoodEndSE":		prefabIndex = 8; break;
					case "WoodEndSW":		prefabIndex = 9; break;		

					}

					GameObject Fence = (GameObject)Instantiate(FencePrefabs[prefabIndex], new Vector3(posX,posY,zeroZ), Quaternion.identity);
					GameObject Grass = (GameObject)Instantiate(GrassPrefabs[1], new Vector3(posX,posY,grassZ), Quaternion.identity);


					if (isBattleMap) 
					{
						//GameObject GrassCannon = (GameObject)Instantiate (BuildingCannon, new Vector3 (posX, posY, cannonZ), Quaternion.identity);
						//GameObject GrassDamageBar = (GameObject)Instantiate (DamageBar, new Vector3 (posX, posY, grassZ), Quaternion.identity);
						//ProcessCannon (GrassCannon, structureIndex);
						//ProcessDamageBar (GrassDamageBar, structureIndex);
					}
					else
						wallCreator.ReloadExistingStructures (prefabIndex);

					ProcessGridBasedStructure (Fence, Grass, structureClass, structureType, structureIndex, iRow, jCol);

				} 
				else if (structureClass == "Ambient") 
				{					
					switch (structureType) 
					{					
					case "PalmTreeA":	prefabIndex = 0; break;
					case "PalmTreeB":	prefabIndex = 1; break;
					case "PalmTreeC":	prefabIndex = 2; break;
					case "PalmTreeD":	prefabIndex = 3; break;
					case "TallTreeA":	prefabIndex = 4; break;
					case "TallTreeB":	prefabIndex = 5; break;
					case "SmallTree":	prefabIndex = 6; break;
					case "Flag":		prefabIndex = 7; break;
					case "Anthurium":	prefabIndex = 8; break;
					case "Daffodils":	prefabIndex = 9; break;
					case "Foxgove":		prefabIndex = 10; break;
					case "Ginger":		prefabIndex = 11; break;
					case "Heather":		prefabIndex = 12; break;
					case "Pansies":		prefabIndex = 13; break;
					case "Protea":		prefabIndex = 14; break;
					case "Scarlet":		prefabIndex = 15; break;
					}

					GameObject Ambient = (GameObject)Instantiate(AmbientPrefabs[prefabIndex], new Vector3(posX,posY,zeroZ), Quaternion.identity);
					GameObject Grass = (GameObject)Instantiate(GrassPrefabs[0], new Vector3(posX,posY,grassZ), Quaternion.identity);

					/*
					if (isBattleMap) 
					{
						GameObject GrassCannon = (GameObject)Instantiate (BuildingCannon, new Vector3 (posX, posY, cannonZ), Quaternion.identity);
						GameObject GrassDamageBar = (GameObject)Instantiate (DamageBar, new Vector3 (posX, posY - 200, grassZ), Quaternion.identity);	

						ProcessCannon (GrassCannon, structureIndex);
						ProcessDamageBar (GrassDamageBar, structureIndex);
					}
					*/
					if (!isBattleMap) 
						ambientCreator.ReloadExistingStructures (prefabIndex);
					
					ProcessGridBasedStructure (Ambient, Grass, structureClass, structureType, structureIndex, iRow, jCol);

				}
			}
		}
	}

	protected void InstantiateConstructions()//adapt to all prefab types
	{		
		while (currentLine != "###Removables###") //Construction
		{
			//Construction: buildingType, constructionIndex, buildingTime, remainingTime, storageIncrease, position.x, position.y
			ReadNextLine();
			if (currentLine != "###Removables###") 
			{
				string[] lineConstruction = currentLine.Split ("," [0]);//reads the line, values separated by ","

				string 	structureClass = lineConstruction[0],
						structureType = lineConstruction[1];

				int 	structureIndex = int.Parse(lineConstruction[2]),
				 		grassType = int.Parse(lineConstruction[3]),
				 		buildingTime = int.Parse(lineConstruction [4]),
				 		remainingTime = int.Parse(lineConstruction [5]), 
				 		storageAdd = int.Parse(lineConstruction [6]),
						iRow = int.Parse(lineConstruction [7]),
						jCol = int.Parse(lineConstruction [8]);

				float	posX = float.Parse (lineConstruction [9]),
						posY = float.Parse (lineConstruction [10]);
						
				GameObject Grass = (GameObject)Instantiate(GrassPrefabs[grassType], new Vector3(posX,posY,grassZ), Quaternion.identity);
				GameObject Construction = (GameObject)Instantiate(ConstructionPrefabs[grassType], new Vector3(posX,posY,zeroZ), Quaternion.identity);

				int prefabIndex = -1;			

				if (structureClass == "Building") {
					switch (structureType) {				
					//Construction: buildingType, constructionIndex, buildingTime, remainingTime,storageincrease,  position.x, position.y

					case "Toolhouse":		prefabIndex = 0; break;		
					case "Forge":			prefabIndex = 1; break;			
					case "Generator":		prefabIndex = 2; break;	
					case "Vault":			prefabIndex = 3; break;							
					case "Barrel":			prefabIndex = 4; break;	
					case "Summon":			prefabIndex = 5; break;		
					case "Academy":			prefabIndex = 6; break;							
					case "Classroom":		prefabIndex = 7; break;					
					case "Chessboard":		prefabIndex = 8; break;								
					case "Globe":			prefabIndex = 9; break;	
					case "Workshop":		prefabIndex = 10;break;		
					case "Tatami":			prefabIndex = 11;break;		
					}

					GameObject  Building = (GameObject)Instantiate (BuildingPrefabs [prefabIndex], new Vector3 (posX, posY, zeroZ), Quaternion.identity);
					ProcessConstruction(Building, Construction, Grass, structureClass, structureType, structureIndex, grassType, buildingTime, remainingTime, storageAdd,iRow,jCol);
					if (isBattleMap) 
					{
						//GameObject GrassCannon = (GameObject)Instantiate (BuildingCannon, new Vector3 (posX, posY, cannonZ), Quaternion.identity);
						GameObject GrassDamageBar = (GameObject)Instantiate (DamageBar, new Vector3 (posX, posY - 200, grassZ), Quaternion.identity);
						//ProcessCannon (GrassCannon, structureIndex);
						ProcessDamageBar (GrassDamageBar, structureIndex);
					}
					else
						buildingCreator.ReloadExistingStructures (prefabIndex);
					existingBuildings [prefabIndex]++;
				}
				else if (structureClass == "Weapon") {
					switch (structureType) {				
					//Construction: buildingType, constructionIndex, buildingTime, remainingTime,storageincrease,  position.x, position.y

					case "Cannon":		 	prefabIndex = 0; break;	
					case "ArcherTower":		prefabIndex = 1; break;
					case "Catapult":		prefabIndex = 2; break;	
					case "DronePad":		prefabIndex = 3; break;
					case "Bomb":		 	prefabIndex = 4; break;	
					case "AirMine":			prefabIndex = 5; break;		

					}
					if (isBattleMap) 
					{
						//GameObject GrassCannon = (GameObject)Instantiate (BuildingCannon, new Vector3 (posX, posY, cannonZ), Quaternion.identity);
						GameObject GrassDamageBar = (GameObject)Instantiate (DamageBar, new Vector3 (posX, posY - 200, grassZ), Quaternion.identity);
						//ProcessCannon (GrassCannon, structureIndex);
						ProcessDamageBar (GrassDamageBar, structureIndex);
					}
					else
						weaponCreator.ReloadExistingStructures (prefabIndex);

					GameObject  Weapon = (GameObject)Instantiate (WeaponPrefabs [prefabIndex], new Vector3 (posX, posY, zeroZ), Quaternion.identity);
					ProcessConstruction( Weapon, Construction, Grass, structureClass, structureType, structureIndex, grassType, buildingTime, remainingTime, storageAdd,iRow,jCol);
				}
				else if (structureClass == "Ambient") {
					switch (structureType) {				
					//Construction: buildingType, constructionIndex, buildingTime, remainingTime,storageincrease,  position.x, position.y

					case "PalmTreeA":	prefabIndex = 0; break;		
					case "PalmTreeB":	prefabIndex = 1; break;			
					case "PalmTreeC":	prefabIndex = 2; break;	
					case "PalmTreeD":	prefabIndex = 3; break;	
					case "TallTreeA":	prefabIndex = 4; break;	
					case "TallTreeB":	prefabIndex = 5; break;	
					case "SmallTree":	prefabIndex = 6; break;	
					case "Flag":		prefabIndex = 7; break;	
					case "Anthurium":	prefabIndex = 8; break;	
					case "Daffodils":	prefabIndex = 9; break;	
					case "Foxglove":	prefabIndex = 10;break;	
					case "Ginger":		prefabIndex = 11;break;
					case "Heather":		prefabIndex = 12;break;
					case "Pansies":		prefabIndex = 13;break;
					case "Protea":		prefabIndex = 14;break;
					case "Scarlet":		prefabIndex = 15;break;
					}
					//construction prefab 6 is the "seeding" site

					if(!isBattleMap)
						ambientCreator.ReloadExistingStructures (prefabIndex);
					
					GameObject  Ambient = (GameObject)Instantiate (AmbientPrefabs [prefabIndex], new Vector3 (posX, posY, zeroZ), Quaternion.identity);
					ProcessConstruction(Ambient, Construction, Grass, structureClass, structureType, structureIndex, grassType, buildingTime, remainingTime, storageAdd,iRow,jCol);
				}			
			}
		}
	}

	protected void InstantiateRemovables()
	{
		int[] removeTimes = new int[removableTypesNo];

		if (!isBattleMap)
			removeTimes = ((RemovableCreator)removableCreator).removeTimes;
		else		
			removeTimes = ((TransData)transData).removeTimes;			

		while (currentLine != "###RemovableTimers###") //Removables
		{
			//Construction: buildingType, constructionIndex, buildingTime, remainingTime, storageIncrease, position.x, position.y
			ReadNextLine();
			if (currentLine != "###RemovableTimers###") 
			{
				string[] lineRemovable = currentLine.Split ("," [0]);//reads the line, values separated by ","
				
				string removableType = lineRemovable[0];
				int removableIndex = int.Parse(lineRemovable[1]);
				int inRemoval = int.Parse(lineRemovable[2]);
				
				int iColumn = int.Parse(lineRemovable[3]);
				int jRow = int.Parse(lineRemovable[4]);
				
				float posX = nodes[iColumn,jRow].position.x;
				float posY = nodes[iColumn,jRow].position.y;				

				GameObject Grass = (GameObject)Instantiate(GrassPrefabs[1], new Vector3(posX,posY,grassZ), Quaternion.identity);

				int removeTime = 0,
				prefabIndex =-1;
				
				switch (removableType) 
				{					
				case "ClamA":	prefabIndex = 0; break;
				case "ClamB":	prefabIndex = 1; break;
				case "ClamC":	prefabIndex = 2; break;					
				case "TreeA":	prefabIndex = 3; break;
				case "TreeB":	prefabIndex = 4; break;
				case "TreeC":	prefabIndex = 5; break;
				case "TreeD":	prefabIndex = 6; break;					
				}

				GameObject Removable = (GameObject)Instantiate(RemovablePrefabs[prefabIndex], new Vector3(posX,posY,zeroZ), Quaternion.identity);
				//if(!isBattleMap)
				removeTime = removeTimes[prefabIndex];

				ProcessRemovable(Removable, Grass, removableType, removableIndex, inRemoval, removeTime, iColumn, jRow);
			}
		}

	}

	protected void InstantiateRemovableTimers()
	{
		while (currentLine != "###Numerics###") //Removable Timers
		{
			//Construction: buildingType, constructionIndex, buildingTime, remainingTime, storageIncrease, position.x, position.y
			ReadNextLine();
			if (currentLine != "###Numerics###") 
			{
				string[] lineRemovableTimer = currentLine.Split ("," [0]);//reads the line, values separated by ","
				
				int timerIndex = int.Parse(lineRemovableTimer[0]);
				int remainingTime = int.Parse(lineRemovableTimer[1]);
				
				GameObject RemovableTimer = (GameObject)Instantiate(RemovableTimerPf, new Vector3(), Quaternion.identity);
				
				ProcessRemovableTimer(RemovableTimer, timerIndex,remainingTime);
			}
		}
	}

	protected void ProcessPositionalStructure(GameObject structure, GameObject grass, string structureClass, int structureIndex, int grassType)//string buildingTag,
	{
		StructureSelector sSel = structure.GetComponent<StructureSelector> ();

		sSel.structureIndex = structureIndex;//unique int to pair buildings and the grass underneath
		sSel.grassType = grassType;
		sSel.inConstruction = false;

		GrassSelector gSel = grass.GetComponent<GrassSelector>();				
		gSel.grassIndex = structureIndex;				
		gSel.structureClass = structureClass;

		//((StructureSelector)structureSelector).isSelected = false;

		//if(isBattleMap)
		sSel.battleMap = isBattleMap;

		grass.transform.parent = structure.transform;

		switch (structureClass) {
		case "Building":
			structure.transform.parent = GroupBuildings.transform;
			break;		
		case "Weapon":
			structure.transform.parent = GroupWeapons.transform;
			break;
		default:
			break;
		}

		if (!isBattleMap) 
		{	
			string structureType = sSel.structureType;
			if (structureType == "Forge" || structureType == "Generator") 
			{				
				//sSel.resourceGenerator = (ResourceGenerator)resourceGenerator;
				sSel.isProductionBuilding = true;
				sSel.LateRegisterAsProductionBuilding ();
			}
		}
		//structureSelector.structureType = structureType;
		//structureSelector.isProductionBuilding = true;
		//structureSelector.RegisterAsProductionBuilding ();

		LoadedGrass.Add (grass);
		LoadedBuildings.Add (structure);//the list is sorted and then used to pair the buildings and the grass by index
		//all grass is recorded in the same list, for both buildings and constructions; after they are sorted by index, 
		//the first batch goes to finished buildings, the rest goes to underconstruction
		AdjustStructureZ (structure, 1, 2);
			
	}

	protected void ProcessGridBasedStructure(GameObject structure, GameObject grass, string structureClass, string structureType, int structureIndex, int iRow, int jCol)// int removeTime, int goldPrice, int manaPrice, int goldGain, int manaGain, 
	{			
		StructureSelector sSel = structure.GetComponent<StructureSelector>();

		sSel.structureType = structureType;
		sSel.structureIndex = structureIndex;
		sSel.iRow = iRow;
		sSel.jCol = jCol;

		sSel.battleMap=isBattleMap;

		currentStructure = structure;

		GrassSelector gSel = grass.GetComponent<GrassSelector>();				
		gSel.grassIndex = structureIndex;				
		gSel.structureClass = structureClass;
		currentGrass = grass;				

		AdjustStructureZ (currentStructure, 1, 2);
		currentGrass.transform.parent = currentStructure.transform;

		switch (structureClass) 
		{
		case "StoneWall":
			structure.transform.parent = GroupWalls.transform;
			break;
		case "WoodFence":
			structure.transform.parent = GroupWalls.transform;
			break;
		case "Ambient":
			structure.transform.parent = GroupAmbients.transform;
			break;

		default:
			break;
		}
	}

	/*
	private void ProcessCannon(GameObject cannon, int grassIndex)
	{
		((StructureSelector)cannon.GetComponent ("StructureSelector")).battleMap = isBattleMap;
		((StructureSelector)cannon.GetComponent ("StructureSelector")).structureIndex = grassIndex;
		cannon.transform.parent = GroupCannons.transform;
		LoadedCannons.Add(cannon);			
	}
	*/
	private void ProcessDamageBar(GameObject damageBar, int grassIndex)
	{		
		((Selector)damageBar.GetComponent ("Selector")).index = grassIndex;
		((Selector)damageBar.GetComponent ("Selector")).isSelected = false;
		damageBar.transform.parent = GroupDamageBars.transform;
		LoadedDamageBars.Add(damageBar);				
	}


	protected void ProcessConstruction(GameObject structure, GameObject construction, GameObject grass, string structureClass, string structureType, int structureIndex, int grassType, int buildingTime, int remainingTime, int storageAdd,int iRow,int jCol)
	{		
		ConstructionSelector cSel = construction.GetComponent<ConstructionSelector>();


		cSel.structureClass = structureClass;
		cSel.structureType = structureType;
		cSel.constructionIndex = structureIndex;
		cSel.buildingTime = buildingTime;

		cSel.battleMap = isBattleMap;
		cSel.grassType = grassType;
		construction.GetComponentInChildren<UISlider>().value = 1-(float)remainingTime/buildingTime;
		cSel.remainingTime = remainingTime;

		cSel.storageAdd = storageAdd;
		cSel.iRow = iRow;
		cSel.jCol = jCol;

		cSel.isSelected = false;

		if(isBattleMap)			
			for (int i = 0; i < 3; i++) 
			{
				construction.transform.GetChild(0).transform.GetChild(i).gameObject.SetActive(false);//1 deactivate finish now button/progress bar display
			}
		
		LoadedConstructions.Add(construction);

		AdjustStructureZ (construction, 1, 3);

		cSel.DetermineParentGroup ();

		grass.transform.parent = construction.transform;
		grass.GetComponent<GrassSelector> ().structureClass = structureClass;
		grass.GetComponent<GrassSelector> ().grassIndex = structureIndex;
		structure.transform.parent = construction.transform;
		structure.SetActive(false);
		construction.transform.parent = GroupConstructions.transform;

		StructureSelector sSel = structure.GetComponent<StructureSelector>();
		
		sSel.structureIndex = structureIndex;
		sSel.grassType = grassType;
		sSel.battleMap = isBattleMap;
		//((StructureSelector)buildingSelector).isSelected = false;	

		if (isBattleMap && structureClass != "Ambient" ) //&& structureType!="Bomb"
		{
			LoadedGrass.Add (grass);
			LoadedBuildings.Add (structure);
		}
		AdjustStructureZ (structure, 1, 2);			

	}

	protected void ProcessRemovable(GameObject removable, GameObject grass, string removableType, int removableIndex, int inRemoval, int removeTime, int iColumn, int jRow)// int removeTime, int goldPrice, int manaPrice, int goldGain, int manaGain, 
	{	
		RemovableSelector rSel = removable.GetComponent<RemovableSelector>();
		
		rSel.removableIndex = removableIndex;
		rSel.iColumn = iColumn;
		rSel.jRow = jRow;

		//if(isBattleMap)
		rSel.battleMap = isBattleMap;

		if(inRemoval==1)
		{
			inRemovalList.Add(removable);//prepare to attach timers
			rSel.removeTime = removeTime;
			rSel.inRemoval = 1;
			//records the removeTime, only for removables already inRemoval
		}
		
		currentRemovable = removable;
			
		GrassSelector gSel = grass.GetComponent<GrassSelector>();		
		gSel.grassIndex = removableIndex;

		currentGrass = grass;				
		currentGrass.transform.parent = currentRemovable.transform;
		currentRemovable.transform.parent = GroupRemovables.transform;
		
	}
	
	protected void ProcessRemovableTimer(GameObject removableTimer, int timerIndex,int remainingTime)
	{
		RemovableTimerSelector rtSel = removableTimer.GetComponent<RemovableTimerSelector>();
		rtSel.removableIndex = timerIndex;
		rtSel.remainingTime = remainingTime;
		rtSel.inRemovalB = true;

		rtSel.battleMap = isBattleMap;

		if(isBattleMap)
			removableTimer.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);//deactivate finish now button

		for (int i = 0; i < inRemovalList.Count; i++) 
		{
			
			if(inRemovalList[i].GetComponent<RemovableSelector>().removableIndex==timerIndex)
			{
				//passes the removeTime, only for removables already inRemoval
				rtSel.removeTime = inRemovalList[i].GetComponent<RemovableSelector>().removeTime;
				removableTimer.transform.position=inRemovalList[i].transform.position;
				removableTimer.transform.parent=inRemovalList[i].transform;
				break;
			}
		}			
	}
	/*
	private IEnumerator LateUpdateProduction(int index, int timeSpan)
	{
		yield return new WaitForSeconds (0.2f);
		//EconomyBuilding lastEconomyBuilding = ((ResourceGenerator)resourceGenerator).existingEconomyBuildings [index];
		((ResourceGenerator)resourceGenerator).FastPaceProductionIndividual (index, timeSpan);
	}
	*/
	protected void LateCalculateElapsedTime()
	{
		StartCoroutine ("CalculateElapsedTime");
	}

	private IEnumerator CalculateElapsedTime()//protected void
	{
		yield return new WaitForSeconds (2);

		timeDifference = loadDateTime.Subtract (saveDateTime);
		
		//everything converted to minutes
		int elapsedTime = timeDifference.Days * 24 * 60 + timeDifference.Hours * 60 + timeDifference.Minutes; 

		if(!isBattleMap)
		((ResourceGenerator)resourceGenerator).FastPaceEconomyAll (elapsedTime);
		
		
		GameObject[] constructionsInProgress = GameObject.FindGameObjectsWithTag ("Construction");

		for (int i = 0; i < constructionsInProgress.Length; i++) 
		{
			ConstructionSelector cSel = constructionsInProgress [i].GetComponent <ConstructionSelector>();//GetComponent ("ConstructionSelector")

			int buildingTime = cSel.buildingTime; 
			int remainingTime = cSel.remainingTime;
			
			Component slider = (UISlider)constructionsInProgress [i].GetComponentInChildren (typeof(UISlider));

			//Finish constructions of any kind if elapsed time>construction time
			if (elapsedTime >= remainingTime) {
				
				if (cSel.structureType == "Forge" || cSel.structureType == "Generator") 
				{	
					int timeSpan = elapsedTime - remainingTime;
					cSel.finishedOffLine = true;
					cSel.elapsedTime = timeSpan;

					//print(timeSpan.ToString());
				} 

				((UISlider)slider.GetComponent ("UISlider")).value = 1;//(UISlider)slider.GetComponent ("UISlider"))
				cSel.progCounter = 1.0f;

				//print("elapsedTime-remainingTime = " + (elapsedTime-remainingTime).ToString());
			} 
			else //everything under 1 minute will appear as finished at reload - int approximation, not an error
				//Update progress bars for constructions which are not finished
			{
				((UISlider)slider.GetComponent ("UISlider")).value += (float)elapsedTime / (float)buildingTime;
				
			}
		}
		
		GameObject[] removableTimers = GameObject.FindGameObjectsWithTag("RemovableTimer");
		
		for (int i = 0; i < removableTimers.Length; i++) 
		{
			RemovableTimerSelector rtSel = removableTimers [i].GetComponent <RemovableTimerSelector>();

			int removeTime = rtSel.removeTime;
			int remainingTime = rtSel.remainingTime;
			
			Component slider = removableTimers [i].GetComponentInChildren (typeof(UISlider));
			
			if (elapsedTime >= remainingTime) 
			{
				((UISlider)slider.GetComponent ("UISlider")).value = 1.0f;
				rtSel.progCounter = 1.0f;
				
			} else 
			{//everything under 1 minute will appear as finished at reload - int approximation, not an error
				((UISlider)slider.GetComponent ("UISlider")).value += (float)elapsedTime / (float)removeTime;			
			}
		}
		
		//Calculate the progession in unit construction que
		if(!isBattleMap)
		{
			int substractTime = elapsedTime;

			//Units

			List<Vector3> queList = new List<Vector3> ();	//qIndex, objIndex, trainingIndex

			queList = ((UnitProc)unitProc).queList;
			queList.Sort (delegate (Vector3 v1, Vector3 v2) { return v1.x.CompareTo (v2.x); });	
			
			const int numberOfUnits = 12;
			int[] trainingTimes = new int[numberOfUnits];
			trainingTimes = ((UnitProc)unitProc).trainingTimes;
			
			int currentTrainingTime;
			
			for (int i = 0; i < queList.Count; i++) 
			{
				if (substractTime > 0) 
				{
					currentTrainingTime = trainingTimes [(int)queList [i].y];
					int trainingIndex = (int)queList [i].z;
					
					while (trainingIndex > 0) 
					{
						if (substractTime > currentTrainingTime) 
						{
							substractTime -= currentTrainingTime;
							((Stats)stats).existingUnits[(int)queList [i].y]++;
							trainingIndex --;
							((UnitProc)unitProc).timeRemaining -= currentTrainingTime;
							queList [i] = new Vector3 (queList [i].x, queList [i].y, trainingIndex);
						} 
						else 					
						{
							((UnitProc)unitProc).currentTrainingTime = currentTrainingTime;
							((UnitProc)unitProc).currentSlidVal += (float)substractTime / (float)currentTrainingTime;
							
							if(currentTrainingTime - substractTime > 0)
							{
								((UnitProc)unitProc).timeRemaining = currentTrainingTime - substractTime;
							}
							else
							{
								((UnitProc)unitProc).timeRemaining = 1;
							}
							
							queList [i] = new Vector3 (queList [i].x, queList [i].y, trainingIndex);
							substractTime = 0;
							break;
						}
					}
				} 
				else 
				{ break;}
			}
			
			bool allZero = true;
			
			for (int i = 0; i < queList.Count; i++) {
				if ((int)queList [i].z != 0) {
					allZero = false;
					break;
				}
			}
			
			if (allZero) {
				((UnitProc)unitProc).queList.Clear ();
				UnitProc.SetActive(false);
				
			} else {
				((UnitProc)unitProc).queList = queList;						
				//((UnitProc)unitProc).SortList();//irrelevant - our qlist is already sorted
			}

			//Buildings

			//((ResourceGenerator)resourceGenerator).InitializeEconomy();

			StartCoroutine ("LateUpdateEconomy");
			StartCoroutine("LateUpdateUI");//some data reaches stats with latency

		}


	}
	
	private IEnumerator LateUpdateUI()
	{				
		yield return new WaitForSeconds (0.50f);
		//((Stats)stats).ApplyMaxCaps ();
		((Stats)stats).UpdateUI();//one-time interface update; called only after changes - once a second because of production
		((Stats)stats).UpdateUnitsNo ();
	}

	private IEnumerator LateUpdateEconomy()
	{				
		yield return new WaitForSeconds (2.00f);

		foreach (var eB in existingEconomyBuildings) 
		{
			for (int i = 0; i < ((ResourceGenerator)resourceGenerator).existingEconomyBuildings.Count; i++) 
			{
				if ((((ResourceGenerator)resourceGenerator).existingEconomyBuildings [i]).structureIndex == eB.structureIndex) 
				{
					(((ResourceGenerator)resourceGenerator).existingEconomyBuildings [i]).ModifyGoldAmount (eB.storedGold);
					(((ResourceGenerator)resourceGenerator).existingEconomyBuildings [i]).ModifyManaAmount (eB.storedMana);
					break;
				}
			}
		}
	}

	private void InstantiateObjects()
	{			
		InstantiatePositionalStructures ();
		InstantiateGridStructures ();

		InstantiateConstructions ();
		InstantiateRemovables ();
		InstantiateRemovableTimers ();

		if(!isBattleMap)
		UpdateButtons ();
	}
	private void UpdateButtons()
	{
		buildingCreator.UpdateButtons ();
		wallCreator.UpdateButtons ();
		weaponCreator.UpdateButtons ();
		ambientCreator.UpdateButtons ();
	}

	private void ClearLists()
	{
		LoadedBuildings.Clear ();
		LoadedConstructions.Clear ();
		LoadedGrass.Clear ();
		//LoadedCannons.Clear ();
		LoadedDamageBars.Clear ();
	}
	protected void LoadGame()//string savefile
	{			

		if(isFileSave)
		{
			((Messenger)statusMsg).DisplayMessage("Loading from LocalFile...");
		}
		else
		{
			savefileLines = savefile.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
			lineIndex = 0;

			((Messenger)statusMsg).DisplayMessage("Loading from PlayerPrefs...");			
		}

		if(!isBattleMap)
		((Stats)stats).gameWasLoaded = true;

		ClearLists ();

		currentLine = "";

		while (currentLine != "###PosStruct###") //goes through the headers, without doing anything, till it finds the ###Buildings### header
		{
			ReadNextLine();
		}
		InstantiateObjects ();
		if(!isBattleMap)
		((Stats)stats).removablesCreated = true;

		ReadNextLine();
		if (!isBattleMap) 
		{
			string[] structureIndexes = currentLine.Split(","[0]);
			((StructureCreator)buildingCreator).structureIndex = int.Parse (structureIndexes [0]);
			((StructureCreator)wallCreator).structureIndex = int.Parse (structureIndexes [1]);
			((StructureCreator)weaponCreator).structureIndex = int.Parse (structureIndexes [2]);
		}
		//UNITS
		ReadNextLine();//#Add verification for empty que

		if (!isBattleMap) 
		{
			UnitProc.SetActive (true);

			string[] currentUnitinProgress = currentLine.Split ("," [0]);

			((UnitProc)unitProc).currentSlidVal = float.Parse (currentUnitinProgress [0]);
			((UnitProc)unitProc).currentTrainingTime = int.Parse (currentUnitinProgress [1]);
		}

		ReadNextLine();//load training times
		if (!isBattleMap) 
		{
			
			string[] trainingTimes = currentLine.Split ("," [0]);

			for (int i = 0; i < trainingTimes.Length; i++) {
				int value = int.Parse (trainingTimes [i]);
				((UnitProc)unitProc).trainingTimes [i] = value;
				((MenuUnit)menuUnit).trainingTimes [i] = value; //info is loaded late from xml; user could finish all units with one crystal
			}
		}

		ReadNextLine();//load population weights
		if (!isBattleMap) 
		{			
			string[] populationWeights = currentLine.Split ("," [0]);
			for (int i = 0; i < populationWeights.Length; i++) {
				int value = int.Parse (populationWeights [i]);
				((UnitProc)unitProc).sizePerUnit [i] = value;
				((MenuUnit)menuUnit).sizePerUnit [i] = value; //info is loaded late from xml;
				((Stats)stats).sizePerUnit [i] = value;
			}
		}

		ReadNextLine();//load existing units

		if (!isBattleMap) {
			string[] existingUnits = currentLine.Split ("," [0]);
			for (int i = 0; i < existingUnits.Length; i++) {
				((Stats)stats).existingUnits [i] = int.Parse (existingUnits [i]);
			}

		/*ReadNextLine();//load battle units
		string[] battleUnits = currentLine.Split(","[0]);
		
		for (int i = 0; i < battleUnits.Length; i++) 
		{
			((Stats)stats).deployedUnits[i] = int.Parse(battleUnits[i]);
		}
		*/

			((UnitProc)unitProc).queList.Clear ();
		

			while (currentLine != "###Economy###") {
				ReadNextLine ();
				if (currentLine != "###Economy###") {
				
					string[] unitQue = currentLine.Split ("," [0]);

					if (currentLine != "###Economy###") {
						((UnitProc)unitProc).queList.Add (new Vector3 (
							float.Parse (unitQue [0]),
							float.Parse (unitQue [1]),
							float.Parse (unitQue [2])));
					}
				}
			}
			//((UnitProc)unitProc).start = true;
			((UnitProc)unitProc).SortList ();
			((MenuUnit)menuUnit).unitProc = ((UnitProc)unitProc);
			((MenuUnit)menuUnit).GetUnitsXML ();
		} 
		else 
		{
			while (currentLine != "###Economy###") 
				ReadNextLine ();
		}

		if (!isBattleMap) 
		{ 
			while (currentLine != "###Stats###") 
			{
				ReadNextLine ();
				if (currentLine != "###Stats###") 
				{
					string[] economyParams = currentLine.Split ("," [0]);

					EconomyBuilding eB = EconomyBuildings.AddComponent<EconomyBuilding> ();//search				
					eB.structureIndex = int.Parse(economyParams [0]);
					eB.storedGold = int.Parse(economyParams [1]);
					eB.storedMana = int.Parse(economyParams [2]);
					existingEconomyBuildings.Add (eB);	
				}
			}
		}
		else 
		{
			while (currentLine != "###Stats###") 
				ReadNextLine ();
		}

		//Stats: experience,dobbits,occupiedDobbit,gold,mana,crystal,,maxStorageGold,maxStroageMana,maxCrystals,forgeRates,generatorRates
		ReadNextLine();

		string[] statsTx = currentLine.Split(","[0]);

		if(!isBattleMap)
		{	
		((Stats)stats).level = int.Parse(statsTx[0]);
		((Stats)stats).townHallLevel= int.Parse(statsTx[1]);
		((Stats)stats).structureIndex = int.Parse(statsTx[2]);
		((Stats)stats).experience = int.Parse(statsTx[3]);
		((Stats)stats).maxExperience = int.Parse(statsTx[4]);
		((Stats)stats).dobbits = int.Parse(statsTx[5]);
		((Stats)stats).occupiedDobbits = int.Parse(statsTx[6]);

		((Stats)stats).occupiedHousing = int.Parse(statsTx[7]);
		((Stats)stats).maxHousing = int.Parse(statsTx[8]);

		((Stats)stats).gold = int.Parse(statsTx[9]);
		((Stats)stats).mana = int.Parse(statsTx[10]);
		((Stats)stats).crystals = int.Parse(statsTx[11]);	

		((Stats)stats).maxGold = int.Parse(statsTx[12]);
		((Stats)stats).maxMana = int.Parse(statsTx[13]);
		((Stats)stats).maxCrystals = int.Parse(statsTx[14]);	

		//((Stats)stats).productionRates[0] = float.Parse(statsTx[11]);	
		//((Stats)stats).productionRates[1] = float.Parse(statsTx[12]);	
		}


		bool tutorialCitySeen = Boolean.Parse(statsTx [15]);//Convert.ToBoolean(statsTx [11]);
		if(!isBattleMap)
		((Stats)stats).tutorialCitySeen = tutorialCitySeen;
		if (tutorialCitySeen) GhostHelper.SetActive (false);

		if (!isBattleMap) 
		{
			bool tutorialBattleSeen = Boolean.Parse (statsTx [16]);
			((Stats)stats).tutorialBattleSeen = tutorialBattleSeen;

			bool removablesCreated = Boolean.Parse (statsTx [17]);
			((Stats)stats).removablesCreated = removablesCreated;
		
			bool 
			soundOn = Boolean.Parse(statsTx [18]),
			ambientOn = Boolean.Parse(statsTx [19]),//ambient save/load next release
			musicOn = Boolean.Parse(statsTx [20]);

			((SoundFX)soundFX).ChangeSound (soundOn);
			if (!soundOn) 
			{				
				soundToggle.Start ();
				soundToggle.Set (false, false);
			}
			if (!ambientOn) 
			{
				((SoundFX)soundFX).ToggleAmbient ();
				ambientToggle.Start ();
				ambientToggle.Set (false, false);
			}

			((SoundFX)soundFX).ChangeMusic (musicOn);
			if (!musicOn) 
			{				
				musicToggle.Start ();
				musicToggle.Set (false, false);
			}

			((Stats)stats).UpdateUI ();
		}


		ReadNextLine();
		saveDateTime = DateTime.Parse(currentLine);
		//ProcessZ();

		LateCalculateElapsedTime ();

		if(isFileSave)
			sReader.Close();

		((Messenger)statusMsg).DisplayMessage("Game loaded.");
		//print ("current time " + loadDateTime.ToString ());//to check if the time was calculated properly
		//print ("saved time " + saveDateTime.ToString ());
	}
}
