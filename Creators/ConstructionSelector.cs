using UnityEngine;
using System.Collections;

public class ConstructionSelector : MonoBehaviour {		//controls the behaviour of a "building under construction", from placement to completion
	
	/*
		<ProdType>None</ProdType>					<!-- resource produced - gold/mana/none-->	
		<ProdPerHour>0</ProdPerHour>				<!-- the amount of the resource generated per hour -->			
		
		<StoreType>None</StoreType>					<!-- resource stored - none/gold/mana/dual/soldiers-->	
		<StoreCap>0</StoreCap>						<!-- gold/mana/dual/soldiers storage -->
	*/

	private bool inConstruction = true;

	public bool 
		finishedOffLine = false,								//for offline production then load
		isProductionBuilding = false,
		isSelected = true,								//for initial processing, right after the construction is instantiated
		battleMap = false;								//flag - some components only exist in hometown/battlemap
		
	//private bool 
	//isProductionBuilding = false;

	public float progTime = 0.57f, progCounter = 0;		//for progress timer, one minute

	public int 		
		elapsedTime,									//for offline production then load
		iRow,jCol,
		buildingTime = 1,
		remainingTime = 1,
		priceno,										//price displayed for "finish now" button. based on remaining time
		storageAdd,										//passes maxStorage to stats
		xpAdd,
		constructionIndex = -1,							//unique ID for constructions
		grassType;

	private int hours, minutes, seconds;				//for time remaining label
	public UILabel TimeCounterLb;

	public GameObject 
		Price,											//own child obj - has the price label
		ProgressBar;									//own child obj

	public GameObject ParentGroup;					//to parent the building after it's finished

	private GameObject[] selectedGrassType;	

	public string 
	structureType,			//Toolhouse, Cannon, ArcherTower, etc 
	structureClass;			//Building,Wall,Weapon,Ambient

	private Component soundFX, relay, statusMsg, stats;//, resourceGenerator;

	void Start () 
	{
		//GroupBuildings = GameObject.Find("GroupBuildings");

		relay = GameObject.Find("Relay").GetComponent<Relay>();
		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();

		if(!battleMap)
		{
			//resourceGenerator = GameObject.Find("ResourceGenerator").GetComponent<ResourceGenerator>();
			stats = GameObject.Find("Stats").GetComponent<Stats>();
		}
			
		//init price so user can't click fast on price 0
		//also adjusts by one minute to avoid starting in the next upper interval - visible at 30 or 60 minutes for example

		//remainingTime = buildingTime * (1 - (int)((UISlider)ProgressBar.GetComponent("UISlider")).value);

		remainingTime = buildingTime - 1;
		UpdatePrice (remainingTime);

	}

	public void DetermineParentGroup()
	{
		switch (structureClass) //Building,Wall,Weapon,Ambient
		{

		case "Building":
			ParentGroup = GameObject.Find ("GroupBuildings");
			break;
		case "Wall":
			ParentGroup = GameObject.Find ("GroupWalls");
			break;
		case "Weapon":
			ParentGroup = GameObject.Find ("GroupWeapons");
			break;
		case "Ambient":
			ParentGroup = GameObject.Find ("GroupAmbients");
			break;
		default:
			break;
		}
	}


	void FixedUpdate()
	{
		if(inConstruction)
		{
			ProgressBarUpdate();
		}
	}

	private void ProgressBarUpdate()
	{		
		progCounter += Time.deltaTime*0.5f;
		if(progCounter > progTime)
		{
			progCounter = 0;
			
			((UISlider)ProgressBar.GetComponent("UISlider")).value += (float)(Time.deltaTime/buildingTime);				//update progress bars values
			
			((UISlider)ProgressBar.GetComponent("UISlider")).value=
			Mathf.Clamp(((UISlider)ProgressBar.GetComponent("UISlider")).value,0,1);

			remainingTime = (int)(buildingTime * (1 - ((UISlider)ProgressBar.GetComponent("UISlider")).value));

			UpdatePrice (remainingTime);
			UpdateTimeCounter(remainingTime);

			if(((UISlider)ProgressBar.GetComponent("UISlider")).value == 1)				//building finished - the progress bar has reached 1												
			{
				((SoundFX)soundFX).BuildingFinished();

				if(!battleMap)															//if this building is not finished on a battle map
				{				
					//xml order: Forge Generator Vault Barrel Summon Tatami
					((Stats)stats).occupiedDobbits--;									//the dobbit previously assigned becomes available

					if(structureType=="Toolhouse")										//increases total storage in Stats																	
					{
						((Stats)stats).dobbits += 1;
					}
					else if(structureType=="Tatami")									//increases total storage in Stats																	
					{
						((Stats)stats).maxHousing += storageAdd;
					}
					else if(structureType=="Forge")
					{
						isProductionBuilding = true;			//((Stats)stats).maxGold += storageAdd;	
					}
					else if(structureType=="Generator")
					{
						isProductionBuilding = true;			//((Stats)stats).maxMana += storageAdd;					
					}

					else if(structureType=="Barrel")									//increases total storage in Stats																	
					{
						((Stats)stats).maxMana += storageAdd;
					}
					else if(structureType=="Vault")
					{
						((Stats)stats).maxGold += storageAdd;
					}

					((Stats)stats).UpdateUI();
				}

				foreach (Transform child in transform) 									//parenting and destruction of components no longer needed
				{					
					if(child.gameObject.tag == "Structure")//structureType
					{
						child.gameObject.SetActive(true);

						StructureSelector structureSelector = ((StructureSelector)child.gameObject.GetComponent ("StructureSelector"));
						structureSelector.inConstruction = false;

						if (battleMap)
							structureSelector.battleMap = true;
						else if (isProductionBuilding)
						{
							structureSelector.structureType = structureType;
							structureSelector.isProductionBuilding = true;

							structureSelector.LateRegisterAsProductionBuilding ();//0.5f

							if (finishedOffLine) 
							{								
								structureSelector.LateCalculateElapsedProduction (elapsedTime);//1.0f
							}



							/*
							MessageNotification m = child.GetComponent<MessageNotification> ();
							m.structureIndex = constructionIndex;
							((ResourceGenerator)resourceGenerator).RegisterMessageNotification (m);
							*/
						}
						foreach (Transform childx in transform) 
						{	
							if(childx.gameObject.tag == "Grass")
							{									
								childx.gameObject.transform.parent = child.gameObject.transform;
								child.gameObject.transform.parent = ParentGroup.transform;

								break;
							}
						}
						break;
					}
				}

				Destroy(this.gameObject);
				inConstruction = false;	
			}			
		}			
	}




	/*
	private void RegisterAsProductionBuilding()
	{	
		for (int i = 0; i < ((ResourceGenerator)resourceGenerator).basicEconomyValues.Length; i++) 
		{
			if (((ResourceGenerator)resourceGenerator).basicEconomyValues [i].StructureType == structureType) 
			{	
				CopyBasicValues (i, ((ResourceGenerator)resourceGenerator).basicEconomyValues [i]);
				break;
			}							
		}
	}

	private void CopyBasicValues(int i, EconomyBuilding basicValuesEB)
	{		
		EconomyBuilding myEconomyParams = new EconomyBuilding();

		myEconomyParams.structureIndex = constructionIndex;
		myEconomyParams.ProdPerHour = basicValuesEB.ProdPerHour;
		myEconomyParams.StoreCap = basicValuesEB.StoreCap;
		myEconomyParams.StructureType = structureType;
		myEconomyParams.ProdType = basicValuesEB.ProdType;
		myEconomyParams.StoreType = basicValuesEB.StoreType;
		myEconomyParams.StoreResource = basicValuesEB.StoreResource;

		((ResourceGenerator)resourceGenerator).existingEconomyBuildings.Add (myEconomyParams);
	}
	*/

	private void UpdateTimeCounter(int remainingTime)				//calculate remaining time
	{
		hours = (int)remainingTime/60; 
		minutes = (int)remainingTime%60;
		seconds = (int)(60 - (((UISlider)ProgressBar.GetComponent("UISlider")).value*buildingTime*60)%60);	

		if (minutes == 60) minutes = 0;
		if (seconds == 60) seconds = 0;

		UpdateTimeLabel ();
	}

	private void UpdateTimeLabel()									//update the time labels on top
	{
		if(hours>0 && minutes >0 && seconds>=0 )
		{			

			((UILabel)TimeCounterLb).text = 
				hours.ToString() +" h " +
					minutes.ToString() +" m " +
					seconds.ToString() +" s ";			
		}
		else if(minutes > 0 && seconds >= 0)
		{
			((UILabel)TimeCounterLb).text = 
				minutes.ToString() +" m " +
					seconds.ToString() +" s ";
			
		}
		else if(seconds > 0 )
		{
			((UILabel)TimeCounterLb).text = 
				seconds.ToString() +" s ";
		}

	}


	private void UpdatePrice(int remainingTime)					//update the price label on the button, based on remaining time		
	{
		/*
		0		30		1
		30		60		3
		60		180		7
		180		600		15
		600		1440	30
		1440	2880	45
		2880	4320	70
		4320			150
		 */

		if (remainingTime >= 4320) { priceno = 150; }
		else if (remainingTime >= 2880) { priceno = 70; }
		else if (remainingTime >= 1440) { priceno = 45;	}
		else if (remainingTime >= 600)	{ priceno = 30;	}
		else if (remainingTime >= 180) { priceno = 15; }
		else if (remainingTime >= 60) { priceno = 7; }
		else if (remainingTime >= 30) {	priceno = 3; }
		else if(remainingTime >= 0) { priceno = 1; }

		((tk2dTextMesh)Price.GetComponent("tk2dTextMesh")).text = priceno.ToString();
	}

	public void Finish()									
	{
		//if(!battleMap) //no need to check, the finish button is not visible on the battle map
		//{
		if (!((Relay)relay).pauseInput && !((Relay)relay).delay)  //panels are open / buttons were just pressed 
		{
			((SoundFX)soundFX).Click();
			if (((Stats)stats).crystals >= priceno) 
			{
				((Stats)stats).crystals -= priceno;
				((Stats)stats).UpdateUI();
				((UISlider)ProgressBar.GetComponent ("UISlider")).value = 1;
			} 
			else 
			{				
				((Messenger)statusMsg).DisplayMessage("Insufficient crystals");
			}
		}
		//}
	}
}
