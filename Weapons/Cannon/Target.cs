using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour {


	public float 
		lerpTime = 0.5f,// time taken to move from the start to finish positions
		lerpDistance = 100,// How far the object should move when 'space' is pressed
		autoTimer,
		autoTime = 1;

	private int 
		resetCounter = 0, 
		targetZ = -1;

	//Whether we are currently interpolating or not
	private bool isLerping, evade, teleport, manual = true;
	
	//The start and finish positions for the interpolation
	private Vector3 startPosition, endPosition;
			
	//The Time.time value when we started the interpolation
	private float lerpStartTime;

	void Start()
	{
		endPosition = transform.position + Vector3.left* lerpDistance;
	}

	// Called to begin the linear interpolation
	void StartLerping()
	{
		isLerping = true;
		lerpStartTime = Time.time;
		
		//We set the start position to the current position, and the finish to 10 spaces in the 'forward' direction
		startPosition = transform.position;

	}
	
	void Update()
	{
		if(manual && Input.GetMouseButton(0))
		SetTargetPoint ();

		if(evade)
			AutoEvade ();

		if(teleport)
			Teleport();

		ManualControl ();
	}
	private void SetTargetPoint() 
	{			
		
		
		// Generate a plane that intersects the transform's position with an upwards normal.
		Plane playerPlane = new Plane(Vector3.back, new Vector3(0, 0, 0));//transform.position + 
		
		// Generate a ray from the cursor position
		
		Ray RayCast;
		
		if (Input.touchCount > 0)
			RayCast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
		else
			RayCast = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		// Determine the point where the cursor ray intersects the plane.
		float HitDist = 0;
		
		// If the ray is parallel to the plane, Raycast will return false.
		if (playerPlane.Raycast(RayCast, out HitDist))//playerPlane.Raycast
		{
			// Get the point along the ray that hits the calculated distance.
			Vector3 RayHitPoint = RayCast.GetPoint(HitDist);

			transform.position = new Vector3 (RayHitPoint.x, RayHitPoint.y, targetZ);
		}
	}

	private void AutoEvade()
	{
		autoTimer += Time.deltaTime;
		
		if(autoTimer>autoTime)
		{
			autoTimer = 0;
			int i= Random.Range(0,4);
			resetCounter++;
			
			if (resetCounter>10)
			{
				resetCounter=0;
				Reset();
				return;
			}
			
			switch (i) 
			{
			case 0:
				MoveLeft();
				break;
			case 1:
				MoveRight();
				break;
			case 2:
				MoveUp();
				break;
			case 3:
				MoveDown();
				break;
			case 4:
				Reset();
				break;					
			}			
		}
	}

	private void Teleport()
	{
		autoTimer += Time.deltaTime;
		
		if(autoTimer>autoTime)
		{
			transform.position= new Vector3(Random.Range(-500,500),Random.Range(-500,500),-1);
			autoTimer = 0;
		}
	}

	private void ManualControl()
	{				
		if (Input.GetKey(KeyCode.LeftArrow)) 
		{
			MoveLeft();
		}
		if (Input.GetKey(KeyCode.RightArrow)) 
		{
			MoveRight();
		}
		if (Input.GetKey(KeyCode.DownArrow)) 
		{
			MoveDown();
		}
		
		if (Input.GetKey(KeyCode.UpArrow)) 
		{
			MoveUp();
		}
		if(Input.GetKey(KeyCode.Space))
		{
			Reset();
		}

	}

	private void MoveLeft()
	{
		endPosition = transform.position + Vector3.left* lerpDistance;
		StartLerping();
	}

	private void MoveRight()
	{
		endPosition = transform.position + Vector3.right* lerpDistance;
		StartLerping();
	}

	private void MoveUp()
	{
		endPosition = transform.position + Vector3.up* lerpDistance;
		StartLerping();
	}

	private void MoveDown()
	{
		endPosition = transform.position + Vector3.down* lerpDistance;
		StartLerping();
	}
	private void Reset()
	{
		endPosition = new Vector3(0,0,-1) + Vector3.down* lerpDistance;
		StartLerping();
	}

	//We do the actual interpolation in FixedUpdate(), since we're dealing with a rigidbody
	void FixedUpdate()
	{
		if(isLerping)
		{
			//We want percentage = 0.0 when Time.time = lerpStartTime
			//and percentage = 1.0 when Time.time = lerpStartTime + lerpTime
			//In other words, we want to know what percentage of "lerpTime" the value
			//"Time.time - lerpStartTime" is.
			float timeSinceStarted = Time.time - lerpStartTime;
			float percentageComplete = timeSinceStarted / lerpTime;
			
			//Perform the actual lerping.  Notice that the first two parameters will always be the same
			//throughout a single lerp-processs (ie. they won't change until we hit the space-bar again
			//to start another lerp)
			transform.position = Vector3.Lerp (startPosition, endPosition, percentageComplete);
			
			//When we've completed the lerp, we set isLerping to false
			if(percentageComplete >= 1.0f)
			{
				isLerping = false;
			}
		}
	}

	void OnGUI()//to test 
	{	
		if (GUI.Button (new Rect(5, 30, 50, 20), "Target")) 
		{
		
		}

		if (GUI.Button (new Rect (60, 30, 60, 20), "Teleport")) 
		{
			teleport=!teleport;
			if(teleport) evade = false;
		}

		if (GUI.Button (new Rect (125, 30, 50, 20), "Evade")) 
		{
			evade=!evade;
			if(evade)teleport = false;
		}

		if (GUI.Button (new Rect (180, 30, 35, 20), "200")) 
		{
			lerpDistance = 200;
		}

		if (GUI.Button (new Rect (220, 30, 35, 20), "500")) 
		{
			lerpDistance = 500;
		}

		if (GUI.Button (new Rect (260, 30, 55, 20), "Reset")) 
		{
			Reset ();
		}

		/*
		if (GUI.Button (new Rect (5, 30, 45, 20), "Left")) 
		{
			endPosition = transform.position + Vector3.left* lerpDistance;
			StartLerping();
		}
		if (GUI.Button (new Rect (60, 30, 45, 20), "Right")) 
		{
			endPosition = transform.position + Vector3.right* lerpDistance;
			StartLerping();
		}
		if (GUI.Button (new Rect (115, 30, 45, 20), "Down")) 
		{
			endPosition = transform.position + Vector3.down* lerpDistance;
			StartLerping();
		}
		
		if (GUI.Button (new Rect (170, 30, 45, 20), "Up")) 
		{
			endPosition = transform.position + Vector3.up* lerpDistance;
			StartLerping();
		}
		*/

		/*
		if (GUI.Button (new Rect (320, 30, 60, 20), "Manual")) 
		{
			manual=!manual;
		}
		*/


	}



}

