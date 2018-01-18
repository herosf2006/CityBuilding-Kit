using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FighterPathFinder : MonoBehaviour {  //finds the path to the target

	private Vector3 startPos, endPos;
	public Node startNode { get; set; }
	public Node goalNode { get; set; }

	public ArrayList pathArray;

	private Component helios;

	private int selectedTarget = 0;
	private List<Vector2> targetList = new List<Vector2>();

	void Start () 
	{
		helios =  GameObject.Find("Helios").GetComponent<Helios>();
		pathArray = new ArrayList();
	}

	public void FindPath()
	{
		startPos = transform.position;

		int myGroup = GetComponent<FighterController> ().assignedToGroup;

		if(targetList.Count == 0)
		{
			switch (myGroup) 
			{
				case 0:
					targetList =  ((Helios)helios).aiTargetVectorsO; 
					break;
				case 1:
					targetList =  ((Helios)helios).aiTargetVectorsI;
					break;
				case 2:
					targetList =  ((Helios)helios).aiTargetVectorsII;
					break;
				case 3:
					targetList =  ((Helios)helios).aiTargetVectorsIII;			
					break;			
			}
		}

		FindNextFree (myGroup);
	}

	private void FindNextFree(int myGroup)//since the corners are free, but might be inaccessible 
	{
		GetSurroundIndex(myGroup);//some of the units may have died; when surrounding the building, this unit's order might be different

		endPos = targetList [selectedTarget];// of aiTargets
		//Assign StartNode and Goal Node
		startNode = new Node (GridManager.instance.GetGridCellCenter (GridManager.instance.GetGridIndex (startPos)));
		goalNode = new Node (GridManager.instance.GetGridCellCenter (GridManager.instance.GetGridIndex (endPos)));
	
		pathArray = AStar.FindPath (startNode, goalNode);

	}

	private void GetSurroundIndex(int index)
	{
		int currentIndex = ((Helios)helios).surroundIndex[index];
		selectedTarget = currentIndex;

		if(currentIndex<targetList.Count-1)
		{
			((Helios)helios).surroundIndex[index]++;
		}
		else
		{
			((Helios)helios).surroundIndex[index] = 0;
		}
	}

	void OnDrawGizmos()
	{
		if (pathArray == null)
			return;
		
		if (pathArray.Count > 0)
		{
			int index = 1;
			foreach (Node node in pathArray)
			{
				if (index < pathArray.Count)
				{
					Node nextNode = (Node)pathArray[index];
					Debug.DrawLine(node.position, nextNode.position, Color.green);
					index++;
				}
			}
		}
	}

}
/*

//No longer needed - no area is inaccesible - the pathfinder reacted very badly to inaccessible areas;
//this is a sketch, it worked but glitchy, possible index errors and the system froze breefly during update
//it forced us to let units get much closer to buildings, so they can walk between them -
//a step forward, looks more realistic

if (pathArray == null) 
		{	
			((Helios)helios).StopUpdateUnits();

			targetList.RemoveAt(selectedTarget); 
			((Helios)helios).aiTargetVectorsCurrent.RemoveAt(selectedTarget);
			switch (myGroup) 
			{
			case 0:
				((Helios)helios).aiTargetVectorsO.RemoveAt(selectedTarget); 
				break;
			case 1:
				((Helios)helios).aiTargetVectorsI.RemoveAt(selectedTarget);
				break;
			case 2:
				((Helios)helios).aiTargetVectorsII.RemoveAt(selectedTarget);
				break;
			case 3:
				((Helios)helios).aiTargetVectorsIII.RemoveAt(selectedTarget);			
				break;			
			} 
			if(((Helios)helios).surroundIndex[myGroup]>0)
			((Helios)helios).surroundIndex[myGroup]--;

			//RemoveSurroundIndex(selectedTarget);
			FindNextFree (myGroup); 
			print ("index was not good: "+selectedTarget.ToString());

			((Helios)helios).StartUpdateUnits();

		}
 */
