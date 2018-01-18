using UnityEngine;
using System.Collections;

public class PriorityQueue 
{   
	private ArrayList nodes = new ArrayList(); // array to store the priority queue
	   
	public int Length//number of nodes 
    {
        get { return this.nodes.Count; }
    }
	   
	public bool Contains(object node)//node already in queue ?
    {
        return this.nodes.Contains(node);
    }

	public Node First()//get first node in queue
    {
		if (this.nodes.Count > 0)
		{
			return (Node)this.nodes[0];
		}

		return null;
	}

	public void Push(Node node)//add node to priority queue and sort by cost
    {
        this.nodes.Add(node);
        this.nodes.Sort();
    }
	  
	public void Remove(Node node)//remove node from priority queue and sort by cost
    {
        this.nodes.Remove(node);
        this.nodes.Sort();
    }

}
