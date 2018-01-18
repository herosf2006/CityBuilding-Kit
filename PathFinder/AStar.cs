using UnityEngine;
using System.Collections;

public class AStar
{   
    public static PriorityQueue closedList, openList;

	private static ArrayList CalculatePath(Node node)//calculate final path 
    {
        ArrayList list = new ArrayList();
        while (node != null)
        {
            list.Add(node);
            node = node.parent;
        }
        list.Reverse();
        return list;
    }
	  
	private static float EstimateCost(Node startNode, Node goalNode)//calculate estimated cost to goal
    {
        Vector3 cost = startNode.position - goalNode.position;
        return cost.magnitude;
    }
	  
	public static ArrayList FindPath(Node start, Node goal)//find path between start node and goal node
    {        
        openList = new PriorityQueue();
        openList.Push(start);
        start.nodeTotalCost = 0.0f;
        start.estimatedCost = EstimateCost(start, goal);

        closedList = new PriorityQueue();
        Node node = null;

        while (openList.Length != 0)//0
        {
			node = openList.First();

            if (node.position == goal.position)
            {
                return CalculatePath(node);
            }
			
            ArrayList neighbours = new ArrayList();
            GridManager.instance.GetNeighbours(node, neighbours);
			           
            for (int i = 0; i < neighbours.Count; i++)
            {
               
				Node neighbourNode = (Node)neighbours[i]; //cost between neighbour nodes

                if (!closedList.Contains(neighbourNode))
                {					
					float cost = EstimateCost(node, neighbourNode);	//cost from current node to neighbour node
	                
					float totalCost = node.nodeTotalCost + cost;//total cost from start to this neighbor node

					float neighbourNodeEstCost = EstimateCost(neighbourNode, goal);	//estimated cost from neighbor node to goal			

					//pass neighbour node properties
					neighbourNode.nodeTotalCost = totalCost;
	                neighbourNode.parent = node;
	                neighbourNode.estimatedCost = totalCost + neighbourNodeEstCost;
		                
					if (!openList.Contains(neighbourNode))//add neighbour node to list if absent
	                {
	                    openList.Push(neighbourNode);
	                }
                }
            }
			 
            closedList.Push(node);
            openList.Remove(node);
        }
		       
		if (node.position != goal.position) //return null if finished looping and goal not found
        {
            Debug.LogError("Goal Not Found");
			//Debug.Log("Goal Not Found");
            return null;
        }
		       
		return CalculatePath(node); //calculate path based on final node
    }
}
