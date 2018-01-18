using UnityEngine;
using System.Collections;

public class PerimeterCollider : MonoBehaviour {		//used to color the grass red when in collision
		
	public int collisionCounter = 0;				//keeps track of how many other grass patches this one is overlapping

	public bool 			
		inCollision = false;		

	private TurretControllerBase turretController;

	void Start () 
	{
		turretController = transform.parent.GetComponent<TurretControllerBase> ();
	}

	void OnTriggerEnter(Collider collider)//OnCollisionEnter(Collision collision)
	{
		if (collider.gameObject.tag == "Unit") {	 
			collisionCounter++;						
			((TurretControllerBase)turretController).AddTarget(collider.gameObject);
			((TurretControllerBase)turretController).fire = true;
			inCollision = true;	
		}

	}
	void OnTriggerExit(Collider collider)//OnCollisionEnter(Collision collision)
	{
		if (collider.gameObject.tag == "Unit") {	 
			collisionCounter--;	

			((TurretControllerBase)turretController).RemoveTarget(collider.gameObject);

			if(collisionCounter == 0)
			{		
				inCollision = false;

				if (transform.parent.GetComponent<StructureSelector> ().structureType == "Catapult") 
				{
					transform.parent.GetComponent<WeaponAnimationController> ().StopAnimations ();
				}
			}
		}
	}

	/*
	void OnCollisionEnter(Collision col)
	{
		if(col.gameObject.tag == "Dobbit")//"Target"
		{	
			collisionCounter++;						
			((TurretControllerBase)turretController).AddTarget(col.gameObject);
			((TurretControllerBase)turretController).fire = true;
			inCollision = true;			
		}
	}
	
	void OnCollisionExit(Collision col)
	{
		if(col.gameObject.tag == "Dobbit")// "Target"   col.gameObject.layer == LayerMask.NameToLayer("Grass")
		{
			collisionCounter--;	
			((TurretControllerBase)turretController).RemoveTarget(col.gameObject);

			if(collisionCounter == 0)
			{		
				inCollision = false;
			}
		}
	}
*/

	
}
