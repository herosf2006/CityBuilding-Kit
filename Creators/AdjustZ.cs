using UnityEngine;
using System.Collections;

public class AdjustZ : MonoBehaviour {

	public int pivotIndex = 0, spriteIndex = 1, zeroZ = 0;

	void Start () {
		AdjustStructureZ ();
	}

	public void AdjustStructureZ()//int pivotIndex, int spriteIndex
	{
		Vector3 pivotPos = transform.GetChild (pivotIndex).position; //pivot
		Vector3 spritesPos = transform.GetChild (spriteIndex).position;//sprites

		float correctiony = 10 / (pivotPos.y + 3300);//ex: fg 10 = 0.1   bg 20 = 0.05  
		//all y values must be positive, so we add the grid origin y 3207 +100 to avoid divide by 0; 
		//otherwise depth glitches around y 0

		transform.GetChild(spriteIndex).position = new Vector3(spritesPos.x, spritesPos.y, zeroZ - correctiony);//	transform.GetChild(2).position   

	}

}
