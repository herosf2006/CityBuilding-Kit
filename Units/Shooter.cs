using UnityEngine;
using System.Collections;

public class Shooter: MonoBehaviour {		//soldiers shoot at targets using this

	public GameObject[] projectile;

	public int assignedToGroup = 0;
	public float fireRate;
	public bool shoot;
	public string projectileType="Star";

	private float fireCounter = 0;
	private int projectileZ = -11;

	private Component helios, fighterController, soundFx;

	void Start () {

		helios = GameObject.Find ("Helios").GetComponent<Helios> ();
		soundFx = GameObject.Find ("SoundFX").GetComponent<SoundFX> ();
		fighterController = GetComponent<FighterController> ();

		assignedToGroup = ((FighterController)fighterController).assignedToGroup;
	}

	void FixedUpdate () {
	
	if(shoot)
		{
			if(((Helios)helios).pauseAttack[((FighterController)fighterController).assignedToGroup])
				return;

			fireCounter += Time.deltaTime;

			if (fireCounter>fireRate) 
				{
				if(projectileType=="Star"){((SoundFX)soundFx).SoldierFire();}

					Instantiate(projectile[assignedToGroup],new Vector3(transform.position.x,transform.position.y, projectileZ),Quaternion.identity);
					fireCounter = 0;
				}
		}
	}

}
