using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronePad : MonoBehaviour {
	
	public GameObject drone;

	void Start () {	
		LaunchDrone ();
	}

	public void LaunchDrone()
	{
		GameObject droneFlyer = (GameObject)Instantiate(drone, transform.position+ new Vector3(0,0,-5), Quaternion.identity);
		droneFlyer.transform.parent = GameObject.Find ("GroupWeapons").transform;
	}

}
