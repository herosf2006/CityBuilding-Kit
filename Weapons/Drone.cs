using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Drone : MonoBehaviour{		
	
    public Transform body, tip, turret;  

	public Transform[] carrot = new Transform[2];
	private Vector3 destination, lightViewpoint; 

	public float 
	bodyRotSpeed = 0.5f,
	searchlightRotSpeed = 0.1f,
	speed = 100;

	public GameObject[] targets;
	public GameObject flashLight;
	private bool focus = false;//, finished = true;//refers to flashlight zoom in/out

	public enum DroneMode{Roam,Attack}
	public DroneMode currentState = DroneMode.Roam;
	
	private int swingCounter = 0;

    void Start()
    {  
		UpdateDestination();//chose a random grid cell
		InvokeRepeating("NewDestination",3.0f,3.0f);
    }

    void Update()
	{		
        UpdateDrone();
    }

	void FixedUpdate()
	{
		transform.position += 
			tip.transform.forward * speed * Time.deltaTime;
	}

	private void SearchForTarget()
	{		
		UpdateTargets ();

		if (targets.Length > 0 && currentState == DroneMode.Roam) 
		{				
			CancelInvoke ("NewDestination");
			currentState = DroneMode.Attack;
			InvokeRepeating ("StayOnTarget", 3, 3);
		} 
	}
	private void UpdateTargets()
	{
		targets = GameObject.FindGameObjectsWithTag ("Unit");
		if (targets.Length == 0 && currentState
		   == DroneMode.Attack) 
			RevertToRoam ();		
	}

	private void RevertToRoam()
	{
		CancelInvoke ("StayOnTarget");
		currentState = DroneMode.Roam;	
		InvokeRepeating("NewDestination",3.0f,3.0f);

	}
	private void StayOnTarget()
	{	
		//print("stay on target invoke");
		UpdateTargets ();
		int index = 0;		
		index = GetNearestTarget ();
		if (index != -1) 
		{
			destination = targets [index].transform.position;
			lightViewpoint = destination;
		}
	}

	private int GetNearestTarget()
	{		
		List<Vector2> closeTargets = new List<Vector2> ();//list with distances and index (1200,1), sorted by distance, returned by index
		closeTargets.Clear ();

		for (int i = 0; i < targets.Length; i++) {
			if(targets[i]!=null)
			closeTargets.Add(new Vector2(Vector2.Distance(targets[i].transform.position,transform.position), i));			
			
		}
		if (closeTargets.Count > 0) {
			closeTargets.Sort (delegate (Vector2 d1, Vector2 d2) {
				return d1.x.CompareTo (d2.x);
			});//sort by distance
			return (int)closeTargets [0].y;
		} 
		else 
		{
			return -1;
		}
	}

	void UpdateDrone()
	{	
		RotateBody();
	}

	private void RotateBody()
	{
		Vector3 targetDir = gameObject.transform.position - destination;
		float angle=0;

		if (targetDir != Vector3.zero) {
			angle = Mathf.Atan2 (targetDir.y, targetDir.x) * Mathf.Rad2Deg;//+10

			//transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); 						//instant rotation
			body.rotation = Quaternion.Slerp (body.rotation, 
				Quaternion.AngleAxis (angle, Vector3.forward), 
				Time.deltaTime * bodyRotSpeed); 							//slow gradual rotation	

			turret.rotation = Quaternion.Slerp (turret.rotation, 
				Quaternion.AngleAxis (angle, Vector3.forward), 
				Time.deltaTime * searchlightRotSpeed); 						//slow gradual rotation				

		} 
					
		RotateLight ();
	}

	private void RotateLight()
	{
		Vector3 targetDir = gameObject.transform.position - lightViewpoint;

		if (targetDir != Vector3.zero)
		{
			float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;//+10

			//transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); 						//instant rotation
			turret.rotation = Quaternion.Slerp(turret.rotation, 
				Quaternion.AngleAxis(angle, Vector3.forward), 
				Time.deltaTime *searchlightRotSpeed); 													//slow gradual rotation	
		}
	}

	private void UpdateDestination()
	{
		int gridCell = Random.Range(0,1295);//1296 cells in grid

		destination = GridManager.instance.GetGridCellPosition(gridCell);
	}

	private void NewDestination()
	{
		//print("new destination invoke");
		if (Vector3.Distance (gameObject.transform.position, destination) < 200) 
			UpdateDestination ();
		
		//if (currentState == DroneMode.Roam)
		SwingLight ();

		SearchForTarget ();
		//else
			//lightViewpoint = destination;
		
	}

	private void SwingLight()
	{	
		if (swingCounter == 0)
			swingCounter = 1;
		else
			swingCounter = 0;
		
		Vector3 targetDir = carrot[swingCounter].transform.position;
		lightViewpoint = targetDir;

	}
}

