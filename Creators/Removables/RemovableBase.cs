using UnityEngine;
using System.Collections;

public class RemovableBase : MonoBehaviour {

	//[HideInInspector]
	public bool 
		battleMap = false,
		isSelected = true;

	public int 
		removableIndex = -1,
		inRemoval = 0,//0 for false, 1 for true - they are doezens, so we will not use true/false, increases the size of the save file
		removeTime = 1, 
		iColumn,jRow;//the gridmanager node coordinates, smaller to store than x,y positions

	public string removableType;
		
	protected Component removableCreator, helios, relay, soundFX, tween, stats, statusMsg;
}
