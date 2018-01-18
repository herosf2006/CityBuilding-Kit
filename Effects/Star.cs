using UnityEngine;
using System.Collections;

public class Star : MonoBehaviour {		//the stars placed on the edges on the map for unit instantiation

	public bool die = false;
	public float alpha = 1, scale = 1;
		
	void Start()
	{
		transform.parent = GameObject.Find("GroupEffects").transform;
	}

	void FixedUpdate () {

		if (die)
		{
			if(alpha > 0)
			{
				scale += 0.0001f;
				alpha -=0.0008f;
				
				transform.position += new Vector3 (0, 0.1f,0);
				gameObject.GetComponent<tk2dSprite> ().scale = new Vector3(scale,scale,1);
				Color currentCollor = gameObject.GetComponent<tk2dSprite> ().color;
				gameObject.GetComponent<tk2dSprite> ().color = new Color(currentCollor.r,currentCollor.g,currentCollor.b, alpha);

				
			}
			else
				Destroy(gameObject);
		}
	}
}