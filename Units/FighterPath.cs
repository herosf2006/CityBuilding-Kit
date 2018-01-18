using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FighterPath : MonoBehaviour {	//the path soldiers are using to get to their targets; updated in certain circumstances
	public bool bDebug = true;
	public float Radius = 30;//10 larger value to avoid units passing through the exact same point with wrong z
	public List<Vector3> waypoints;
	public FighterPathFinder pathFinder;

	void Start () 
	{
		waypoints = new List<Vector3>();
		waypoints.Add(transform.position);//since the list is empty
		pathFinder = GetComponent<FighterPathFinder>();
		//InvokeRepeating ("UpdatePath", 1.0f, 1.0f);//disabled for performance, one-shot centralized
	}

	public void UpdatePath()
	{
		waypoints.Clear();

		for (int i = 0; i < pathFinder.pathArray.Count; i++) 
		{			
			waypoints.Add(((Node)pathFinder.pathArray[i]).position);			
		}
	}

	public float Length {
		get {
			return waypoints.Count;
		}
	}
	public Vector3 GetPoint(int index) {
		return waypoints[index];
	}

	public Vector3 GetEndPoint() {
		return waypoints[waypoints.Count-1];
	}

	/* // no longer needed - when switching from freewalk (straight to target) to pathwalk, enters the path at the nearest point
	 * // since the animations are 8 angles only, we don't use freewalk 
	public int GetNearestPoint(Vector3 currentPosition)//returns closest path index
	{
		int closestPathIndex;
		List<Vector2> AllPathPoints = new List<Vector2>();
		AllPathPoints.Clear();

		for (int i = 0; i < waypoints.Count; i++) 
		{
			AllPathPoints.Add(new Vector2(i, Vector3.Distance(currentPosition, waypoints[i])));
		}
		AllPathPoints.Sort (delegate(Vector2 v1, Vector2 v2) 
		{
			return v1.y.CompareTo(v2.y);

		});

		closestPathIndex = (int)AllPathPoints [0].x;

	 	//if necessary to re-enter pathwalk a few waypoints ahead - avoid unit going back to first point
		int pathLength = waypoints.Count;
		int pointCorrection = 0;
		if (pathLength > 3)	pointCorrection = 3;
		else if(pathLength > 2)	pointCorrection = 2;
		else if(pathLength > 1)	pointCorrection = 1;

		return closestPathIndex + pointCorrection;// + pointCorrection
		print (pointCorrection.ToString());
	}
	*/

	void OnDrawGizmos() 
	{
		if (!bDebug) return;
		for (int i = 0; i < waypoints.Count; i++) 
		{
			if (i + 1 < waypoints.Count) 
			{
				Debug.DrawLine(waypoints[i], waypoints[i + 1], Color.red);
			} 
		}
	}
}
