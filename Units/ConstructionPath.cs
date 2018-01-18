using UnityEngine;
using System.Collections;

public class ConstructionPath : MonoBehaviour {		//the square path each dobbit is following during construction
	public bool bDebug = true;
	public float Radius = 0.5f;
	public Vector3[] 
		waypoints = new Vector3[4],
		adjWaypoints = new Vector3[4];

	void Start () 
	{
		for (int i = 0; i < adjWaypoints.Length; i++) 
		{
			adjWaypoints[i] = transform.position + waypoints[i];//adjWaypoints are adjusted with object position

			//the zdepth 1 of constructions creates a problem on the battlemap - construction dobbits don't appear correctly
			//in foreground/background of soldiers. maybe a float/double cast difference?
			float zDepth = adjWaypoints[i].z - 10 / (adjWaypoints[i].y + 3300);			//unitZ=-4 (Construction is at 1 !!!)
			adjWaypoints[i]= new Vector3(adjWaypoints[i].x,adjWaypoints[i].y,zDepth);	//inserts zdepth into path
		}
	}

	public float Length {
		get {
			return adjWaypoints.Length;
		}
	}
	public Vector3 GetPoint(int index) {
		return adjWaypoints[index];
	}
	
	void OnDrawGizmos() 
	{
		if (!bDebug) return;
		for (int i = 0; i < adjWaypoints.Length; i++) 
		{
			if (i + 1 < adjWaypoints.Length) 
			{
				Debug.DrawLine(adjWaypoints[i], adjWaypoints[i + 1], Color.red);
			} 
		}
	}	
}
