using UnityEngine;
using System.Collections;

public class Grave : MonoBehaviour {

	// adjusts zdepth of cross, so units can appear behind or in front of it
	//since the units move now based on center of sprite position, we will have to move the anchor higher
	int zeroZ = 0;

	void Start () {	//o stones 1 anchor 2 plank

		Vector3 anchorPos = transform.GetChild (1).position; 
		Vector3 plankPos = transform.GetChild (2).position;

		float correctiony = 10 / (anchorPos.y + 3300);//ex: fg 10 = 0.1   bg 20 = 0.05  
		//all y values must be positive, so we add the grid origin y 3207 +100 to avoid divide by 0; 
		//otherwise depth glitches around y 0

		transform.parent = GameObject.Find("GroupEffects").transform;
		
		//if(Mathf.Abs(correctiony)<1)//not necessary, the highest value of the correction is 0.1f, at the bottom of the map
		//{
			transform.GetChild(2).position = new Vector3(plankPos.x, plankPos.y, zeroZ - correctiony);	   
		//}
	}
	
	
}
