using UnityEngine;
using System.Collections;

public class StructureSelector : BaseSelector {//attached to each building as an invisible 2dtoolkit button
	
	// Use this for initialization
	void Start () {
		InitializeComponents ();
		InitializeSpecificComponents ();
	}
	public void DeSelect()
	{
		if (structureClass == "Weapon")
			alphaTween.FadeAlpha (false, 1);
	}

	public void ReSelect()
	{
		if(((Relay)relay).delay||((Relay)relay).pauseInput) return;

		((StructureTween)scaleTween).Tween();

		if (((Relay)relay).currentAlphaTween != null) 
		{			

			if (((Relay)relay).currentAlphaTween.inTransition)			//force fade even if in transition
				((Relay)relay).currentAlphaTween.CancelTransition ();

			((Relay)relay).currentAlphaTween.FadeAlpha (false, 1);
			((Relay)relay).currentAlphaTween = null;
		}

		if (structureClass == "Weapon") 
		{					
			alphaTween.FadeAlpha (true, 1);
			((Relay)relay).currentAlphaTween = alphaTween;
		} 

		((SoundFX)soundFX).Click();

		if(!battleMap)
		{		
			if(!((StructureCreator)structureCreator).isReselect &&
				!((Relay)relay).pauseInput)
				{

				if (messageNotification != null&&((MessageNotification)messageNotification).isReady) 
					{
					((MessageNotification)messageNotification).FadeOut ();
					resourceGenerator.Harvest (structureIndex);
					((MessageNotification)messageNotification).isReady = false;
					return;
					}

					((BaseCreator)structureCreator).isReselect = true;						
					int childrenNo = gameObject.transform.childCount;//grass was parented last
					((BaseCreator)structureCreator).OnReselect(gameObject, gameObject.transform.GetChild (childrenNo-1).gameObject, structureType);	
				}
		}
		else if(structureClass=="Building"||structureClass=="Weapon")//the target select on the battle map
		{
			((Helios)helios).selectedStructureIndex = structureIndex;
			if(((Helios)helios).DeployedUnits.Count == 0)return; //ignore if there are no units deployed
	
			int assignedToGroup = -1;
			bool userSelect = false;  //auto or user target select

			for (int i = 0; i <= ((Helios)helios).instantiationGroupIndex; i++) //((BattleProc)battleProcSc).userSelect.Length
			{			
				if(((Helios)helios).userSelect[i])
				{
					assignedToGroup = i;
					((Helios)helios).userSelect[i] = false;
					userSelect = true;
					break;
				}
			}

			if(!userSelect)
			{
				assignedToGroup = ((Helios)helios).FindNearestGroup(transform.position);//designate a group to attack this building
			}

			if(assignedToGroup == -1) return;

			if(((Helios)helios).targetStructureIndex[assignedToGroup] != structureIndex)//if this building is not already the target of the designated group
			{
				switch (assignedToGroup) 
				{
				case 0:
					((Helios)helios).Select0();
					break;

				case 1:
					((Helios)helios).Select1();
					break;

				case 2:
					((Helios)helios).Select2();
					break;

				case 3:
					((Helios)helios).Select3();
					break;
				}

				((Helios)helios).targetStructureIndex[assignedToGroup] = structureIndex;	//pass relevant info to BattleProc for this new target building		
				((Helios)helios).targetCenter[assignedToGroup] = transform.position;
				((Helios)helios).FindSpecificBuilding();
				((Helios)helios).updateTarget[assignedToGroup] = true;
				((Helios)helios).pauseAttack[assignedToGroup] = true;
			}

		}
	}

	public void LateRegisterAsProductionBuilding()
	{
		StartCoroutine ("RegisterAsProductionBuilding");
	}

	private IEnumerator  RegisterAsProductionBuilding()//private IEnumerator 
	{			
		yield return new WaitForSeconds (0.5f);

		for (int i = 0; i < ((ResourceGenerator)resourceGenerator).basicEconomyValues.Length; i++) 
		{
			if (((ResourceGenerator)resourceGenerator).basicEconomyValues [i].StructureType == structureType) 
			{					
				CopyBasicValues (((ResourceGenerator)resourceGenerator).basicEconomyValues [i]);
				break;
			}							
		}
	}

	private void CopyBasicValues( EconomyBuilding basicValuesEB)
	{		
		//EconomyBuilding myEconomyParams = new EconomyBuilding();

		EconomyBuilding[] EBArray = EconomyBuildings.GetComponentsInChildren<EconomyBuilding>();

		bool buildingRegistered = false;

		if (EBArray.Length != 0) 
		{	
			foreach (EconomyBuilding eb in EBArray) 
			{
				if (eb.structureIndex == structureIndex) 
				{
					eb.structureIndex = structureIndex;
					eb.ProdPerHour = basicValuesEB.ProdPerHour;
					eb.StoreCap = basicValuesEB.StoreCap;
					eb.StructureType = structureType;
					eb.ProdType = basicValuesEB.ProdType;
					eb.StoreType = basicValuesEB.StoreType;
					eb.StoreResource = basicValuesEB.StoreResource;

					((ResourceGenerator)resourceGenerator).index++;
					productionListIndex = ((ResourceGenerator)resourceGenerator).index;
					((ResourceGenerator)resourceGenerator).existingEconomyBuildings.Add (eb);
					RegisterNotification ();

					buildingRegistered = true;
				}
			}
		}

		if (!buildingRegistered) 
		{
			EconomyBuilding	eb = EconomyBuildings.AddComponent<EconomyBuilding> ();

			eb.structureIndex = structureIndex;
			eb.ProdPerHour = basicValuesEB.ProdPerHour;
			eb.StoreCap = basicValuesEB.StoreCap;
			eb.StructureType = structureType;
			eb.ProdType = basicValuesEB.ProdType;
			eb.StoreType = basicValuesEB.StoreType;
			eb.StoreResource = basicValuesEB.StoreResource;

			((ResourceGenerator)resourceGenerator).index++;
			productionListIndex = ((ResourceGenerator)resourceGenerator).index;
			((ResourceGenerator)resourceGenerator).existingEconomyBuildings.Add (eb);

			RegisterNotification ();
		}

	}

	private void RegisterNotification()
	{
		MessageNotification m = GetComponent<MessageNotification> ();
		m.structureIndex = structureIndex;
		((ResourceGenerator)resourceGenerator).RegisterMessageNotification (m);		
	}

	public void LateCalculateElapsedProduction(int elapsedTime)
	{
		StartCoroutine(CalculateElapsedProduction(elapsedTime));
	}
	private IEnumerator CalculateElapsedProduction(int elapsedTime)//private IEnumerator
	{
		yield return new WaitForSeconds (1.0f);

		resourceGenerator.FastPaceProductionIndividual (productionListIndex, elapsedTime);
		/*
		for (int i = 0; i < ((ResourceGenerator)resourceGenerator).basicEconomyValues.Length; i++) 
		{
			if (((ResourceGenerator)resourceGenerator).basicEconomyValues [i].structureIndex == structureIndex) 
			{	
				print (structureIndex.ToString ());

				break;
			}							
		}
		*/
	}
}
