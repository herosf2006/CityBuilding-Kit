using UnityEngine;
using System.Collections;

public class RemovableSelector : RemovableBase {//attached to each building as an invisible 2dtoolkit button

	int zeroZ = 0;

	// Use this for initialization
	void Start () {

		tween = GetComponent<StructureTween> ();
		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();
		relay = GameObject.Find("Relay").GetComponent<Relay>();
		//statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();

		if(!battleMap)				
		removableCreator = GameObject.Find("RemovableCreator").GetComponent<RemovableCreator>();
		//stats = GameObject.Find("Stats").GetComponent<Stats>();

		if (battleMap) 
		{				
			helios =  GameObject.Find("Helios").GetComponent<Helios>();
		}

		else
		{
			stats = GameObject.Find("Stats").GetComponent<Stats>();
		}

		AdjustZ ();

	}

	private void AdjustZ()
	{

		//Z position Adjust
		
		Vector3 pivotPos = transform.GetChild (1).position; 
		Vector3 spritePos = transform.GetChild (2).position;
		
		float correctiony = 10 / (pivotPos.y + 3300);//ex: fg 10 = 0.1   bg 20 = 0.05  
		//all y values must be positive, so we add the grid origin y 3207 +100 to avoid divide by 0; 
		//otherwise depth glitches around y 0
		
		transform.GetChild(2).position = new Vector3(spritePos.x, spritePos.y, zeroZ - correctiony);	  

	}

	public void Select()
	{
		if(((Relay)relay).delay||((Relay)relay).pauseInput||inRemoval==1) return;

		((StructureTween)tween).Tween();
		((SoundFX)soundFX).Click();

		if(!battleMap)
		{		
			if(!((Relay)relay).pauseInput)//!((BuildingCreator)buildingCreator).isReselect &&
			{
				isSelected = true;//mark this for resellection

				((RemovableCreator)removableCreator).OnSelect(this.gameObject);//removableType

			}
		}
		else //the target select on the battle map
		{
			//((Helios)helios).selectedBuildingIndex = buildingIndex;
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
					
		}
	}
}
