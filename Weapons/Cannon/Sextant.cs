using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sextant : MonoBehaviour{		
	    
	public GameObject targetObject;
	private Vector3 targetPosition;

	public bool 
		fire = false;

	private float 
		turretRotationSpeed = 0.5f,
		fireRate = 1.0f,
    	timeCounter;

	public int[] angles = new int[12]{160, 125, 65, 25, -5, -30, -55, -80, -105, -130, -150, -175};//ANGLE SIGN CHANGE -170 = 190
	public int anglePrecision = 24;

	private Component weaponAnimController;

    void Start()
    {
		weaponAnimController = GetComponent<WeaponAnimationController> ();
    }

    void Update()
	{        
		if(fire)
		{
			timeCounter += Time.deltaTime;
			if (timeCounter >= fireRate)
			{
				GetTarget();
				GetEventFrame();
				timeCounter = 0.0f;
			}
		}
    }
    
	private void GetEventFrame()
	{	
		Vector3 targetDir = gameObject.transform.position - targetPosition;

		if (targetDir != Vector3.zero)
		{
			int 
			eventFrame = 0,
			angle = (int)(Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg+10);

			for (int i = 0; i < angles.Length; i++) //cycle through angles looking for closest value
			{		
				if(Mathf.Abs(angle - angles[i]) <= anglePrecision)
				{					
					eventFrame = i;
					break;
				}
			}

			((WeaponAnimationController)weaponAnimController).RotateAndFire(eventFrame);// all as clockwise frames
    	}
	}

	private void GetTarget()
	{
		if(targetObject!=null)
			targetPosition = targetObject.transform.position;
	}

}