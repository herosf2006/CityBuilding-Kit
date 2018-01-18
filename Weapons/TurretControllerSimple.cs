using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretControllerSimple : TurretControllerBase{		//cannons are instantiated in the middle of each building - simplified situation
		  
    void Start()
    {  
		InitializeComponents ();
    }

    void Update()
	{
		if (fire) 
		{//Input.GetMouseButtonDown(0)
			if (targetList.Count == 0) 
			{
				fire = false;
				return;
			}
			Aim (); 
			elapsedTime += Time.deltaTime;
			if (elapsedTime >= fireRate) {
				UpdateTarget ();
				//Aim();               
				elapsedTime = 0.0f;				             
				LaunchProjectile ();
			}
		} 
		else 
		{
			Search ();
		}
    }

}

