using UnityEngine;
using System.Collections;
using System;

public class Node : IComparable
{    
    public float 
		nodeTotalCost = 1,				//total cost up to this node
   		estimatedCost = 0.0f;			//estimated cost from this node to goal
    
	public bool isObstacle = false;		//node is obstacle ?
    public Node parent = null;			//node parent in linked list
    public Vector3 position;           
    
	public Node(Vector3 pos)
    {       
        position = pos;
    }
	   
	public void MarkAsObstacle()  { isObstacle = true; }//this.bObstacle 

	public void MarkAsFree() { isObstacle = false; }
  
	public int CompareTo(object obj) // compare nodes using estimated total cost
    {
        Node node = (Node)obj;
        if (estimatedCost < node.estimatedCost)
            return -1;
        if (estimatedCost > node.estimatedCost)
            return 1;

        return 0;
    }
}
