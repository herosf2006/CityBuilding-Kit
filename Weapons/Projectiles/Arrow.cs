using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

	public int Speed = 3000;
	public GameObject impact, sparks;
	private Component soundFx;

	//private float curveY;
	//private bool upY = true;

	// Use this for initialization
	void Start () {	
		Destroy (gameObject, 2.0f);
		soundFx = GameObject.Find ("SoundFX").GetComponent<SoundFX> ();
	}
		
	void FixedUpdate()
	{
		transform.position += 
			transform.forward * Speed * Time.deltaTime;
		/*
		if (upY)
			curveY += 0.5f;
		else 
			curveY -= 0.5f;

		if (curveY > 2.5f)
			upY = false;

		transform.position += new Vector3 (0, curveY, 0);
		*/

	}

	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag == "Unit")
		{
			//((SoundFX)soundFx).SoldierFire();     
			Instantiate(impact, transform.position, Quaternion.identity);
			Instantiate(sparks, transform.position, Quaternion.identity);
			Destroy (gameObject);
		}
	}

}
