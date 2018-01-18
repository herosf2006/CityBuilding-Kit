using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;


public class CameraController:MonoBehaviour
{

	[HideInInspector]
	public bool movingBuilding = false;

	private bool 
		moveNb, 					//bools for button controlled moving Move North, East, South, West
		moveEb, 
		moveSb, 
		moveWb, 
		zoomInb, 
		zoomOutb,

		fadeMoveNb, 					//bools for fade out moving Move North, East, South, West
		fadeMoveSb, 
		fadeMoveEb, 
		fadeMoveWb, 
		fadeZoomInb, 
		fadeZoomOutb,

		fade = false,
		fadePan = false,
		fadeZoom = false,

		fadeTouch = false,
		//fadeTouchPan = false,
		//fadeTouchZoom = false,
		stillMovingTouch = false; 
		

	private float
		lastPanX,
		lastPanY,
		zoomMax = 1.0f,						//caps for zoom
		zoomMin = 0.25f,
		zoom, 								// the current camera zoom factor
		currentFingerDistance,
		previousFingerDistance,
		camStepDefault,
		zoomStepDefault;

	public float
		touchPanSpeed = 4.0f,				
		camStep = 15,
		zoomStep = 0.01f,					

		camStepFade = 0.3f,
		zoomStepFade = 0.00005f;
			 							

	private Vector3 
		//initPos = new Vector3(0,0,-10),
		RayHitPoint;
		
	private Vector2 
		velocity, 
		target;

	//[HideInInspector]
	public int 								//camera bounds
	northMax = 4200,	//4200,	
	southMin = -4300,  	//-4300	
	eastMax = 5200,	//5200		
	westMin = -5200;	//-5200	

	private tk2dCamera spriteLightKitCam;
	public GameObject SpriteLightKitCam;
	private Relay relay;
	private tk2dCamera cameratk2D;


	[SerializeField]
	private Camera cam;

	[SerializeField]
	private float speed	= 0.0f;
	private Vector3 Origin;
	private Vector3 Diference;
//	private Vector3 pos;
	private bool Drag=false;
	private Vector3 vel;

	[SerializeField]
	private ToggleGroup navigationButtonsToggleGroup;

	private float cameraSpeedX;
	private float cameraSpeedY;

	private Vector2 cameraPos;

	[SerializeField]
	private bool moveWithButtons;

	void Awake() 
	{
		cam = GetComponent<Camera>();
		cameratk2D = gameObject.GetComponent<tk2dCamera>();
		relay = GameObject.Find ("Relay").GetComponent<Relay>();
		spriteLightKitCam = SpriteLightKitCam.GetComponent<tk2dCamera>();
		camStepDefault = camStep;
		zoomStepDefault = zoomStep;
//		InvokeRepeating("UpdateFunctionality", 0.0f, 0.01f);
	}

	void Start(){}

	//to prevent selecting the buildint underneath the move buttons

	private void Delay()
	{
		((Relay)relay).DelayInput(); 
	}

	//FadeMove
	private void FadeMoveN() { moveSb = false; moveNb = true; fade = true; fadePan = true; camStep = camStepDefault;}	 
	private void FadeMoveS() { moveNb = false; moveSb = true; fade = true; fadePan = true; camStep = camStepDefault;}	
	private void FadeMoveE() { moveWb = false; moveEb = true; fade = true; fadePan = true; camStep = camStepDefault;}	 
	private void FadeMoveW() { moveEb = false; moveWb = true; fade = true; fadePan = true; camStep = camStepDefault;}   

	//Zoom
	public void ZoomIn() { zoomOutb = false; if(!zoomInb) zoomInb = true; else StopZoom(); Delay (); }//FadeOutZoom();
	public void ZoomOut() { zoomInb = false; if(!zoomOutb) zoomOutb = true;	else StopZoom(); Delay (); }

	//FadeZoom
	public void FadeZoomIn()  { zoomOutb = false; zoomInb = true;  fade = true; fadeZoom = true; zoomStep = zoomStepDefault; }	
	public void FadeZoomOut() { zoomInb = false;  zoomOutb = true; fade = true; fadeZoom = true; zoomStep = zoomStepDefault; }

	// conditions keep the camera from going off-map, leaving a margin for zoom-in/out

	private void MoveCameraWithButtons()
	{
		if(navigationButtonsToggleGroup.ActiveToggles().Any())
		{
			Origin = transform.position;
			Diference = new Vector3();

			transform.position += new Vector3(cameraSpeedX, cameraSpeedY, 0);

			float x = Mathf.Clamp(transform.position.x, westMin, eastMax);
			float y = Mathf.Clamp(transform.position.y, southMin, northMax);

			transform.position = new Vector3(x, y, 0);
		}
	}
		
	void LateUpdate()
	{	
		if(fade)
		{
			if(fadePan)
			{
				camStep-= camStepFade;
				if(camStep<=0)
				{
					StopMove();
					camStep = camStepDefault;
					fadePan = false;
				}
			}
			if(fadeZoom)
			{
				zoomStep-= zoomStepFade;
				if(zoomStep<=0.008)//0.002
				{
					StopZoom();
					zoomStep = zoomStepDefault;
					fadeZoom = false;
				}
			}

			if(!fadePan&&!fadeZoom)
				fade = false;
		}
		MoveCamera();
		MoveCameraWithButtons();
		MouseZoom ();
		TouchMoveZoom ();
		ButtonMoveZoom ();
		UpdateSpriteLightKitCam ();
	}

	/// <summary>
	/// Moves the camera.
	/// </summary>
	private void MoveCamera()
	{
		if(!navigationButtonsToggleGroup.ActiveToggles().Any())
		{
			if(Input.GetMouseButton (0) && Input.mousePosition.x < Screen.width && Input.mousePosition.y < Screen.height)
			{   

				navigationButtonsToggleGroup.SetAllTogglesOff();

				Diference = (cam.ScreenToWorldPoint(Input.mousePosition)) - cam.transform.position;
				if(Drag == false)
				{

					Drag = true;
					Origin = cam.ScreenToWorldPoint (Input.mousePosition);
				}
			}
			else
			{
				Drag=false;
			}

			cameraPos = new Vector3((Origin-Diference).x, (Origin-Diference).y);
			float x = Mathf.Clamp(cameraPos.x, westMin, eastMax);
			float y = Mathf.Clamp(cameraPos.y, southMin, northMax);
			cameraPos = new Vector2(x, y);
			cam.transform.position = Vector3.SmoothDamp(cam.transform.position, cameraPos, ref vel, Time.smoothDeltaTime * speed);
		}
	}

	public void MoveCameraWithButtons(int value)
	{
		StopMove();
		Delay();
		switch(value)
		{
			//Move left
			case 0:
				cameraSpeedX = -camStep;
				cameraSpeedY = 0;
			break;
			//Move up
			case 1:
				cameraSpeedX = 0;
				cameraSpeedY = camStep;
			break;
			//Move right
			case 2:
				cameraSpeedX = camStep;
				cameraSpeedY = 0;
			break;
			//Move down
			case 3:
				cameraSpeedX = 0;
				cameraSpeedY = -camStep;
			break;
			default:
			break;
		}
		vel = new Vector3();
	}

	private void UpdateSpriteLightKitCam()
	{
		SpriteLightKitCam.transform.position = transform.position;
		spriteLightKitCam.ZoomFactor = cameratk2D.ZoomFactor;
	}
	private void MouseZoom()
	{

		if (Input.GetAxis("Mouse ScrollWheel")<0) 
		{
			zoomInb = false; if(!zoomOutb) zoomOutb = true;
		}
		else if (Input.GetAxis("Mouse ScrollWheel") > 0) 
		{
			zoomOutb = false; if(!zoomInb) zoomInb = true; 
		}
		/*
		else
		{
			StopZoom();  //this cancels button zoom commands
		}
		*/
	}
	private void ButtonMoveZoom()
	{
		zoom = cameratk2D.ZoomFactor;		

		//zoom in/out

		if(zoomInb && zoom<zoomMax)
		{
			cameratk2D.ZoomFactor += zoomStep;   //In
			fadeZoomOutb = false; fadeZoomInb = true; 
		}
		else if(zoomOutb && zoom>zoomMin)
		{
			cameratk2D.ZoomFactor -= zoomStep;	//Out
			fadeZoomInb = false; fadeZoomOutb = true;
		}
	}

	private void TouchMoveZoom()
	{
		zoom = cameratk2D.ZoomFactor;

		if (Input.touchCount > 1 && Input.GetTouch(0).phase == TouchPhase.Moved//chech for 2 fingers on screen
				&& Input.GetTouch(1).phase == TouchPhase.Moved) 
		{
			if(!((Relay)relay).delay) Delay ();
			Vector2 touchPosition0 = Input.GetTouch(0).position;//positions for both fingers for pinch zoom in/out
			Vector2 touchPosition1 = Input.GetTouch(1).position;
				
			currentFingerDistance = Vector2.Distance(touchPosition0,touchPosition1);//distance between fingers

			//MANUAL ZOOM

			if (currentFingerDistance>previousFingerDistance)
			{
				if(zoom<zoomMax)
				{
					cameratk2D.ZoomFactor += zoomStep;
					fadeZoomOutb = false; fadeZoomInb = true; FadeZoomIn();
				}
			}

			else if(currentFingerDistance<previousFingerDistance && zoom>zoomMin)
			{
				cameratk2D.ZoomFactor -= zoomStep;
				fadeZoomInb = false; fadeZoomOutb = true; FadeZoomOut(); 
			}

			previousFingerDistance = currentFingerDistance;
		
		}
		else if (Input.touchCount ==1 && Input.GetTouch(0).phase == TouchPhase.Stationary)// 
		{
			StopAll ();		
		}

	}

	private void ResetFadePan()
	{
		fadeMoveNb=false;
		fadeMoveSb=false;
		fadeMoveEb=false;
		fadeMoveWb=false;
	}

	private void FadeOutPan()
	{
		if(fadeMoveEb){FadeMoveE();fadeMoveEb=false;}
		else if(fadeMoveWb){FadeMoveW();fadeMoveWb=false;}

		if(fadeMoveNb){FadeMoveN();fadeMoveNb=false;}
		else if(fadeMoveSb){FadeMoveS();fadeMoveSb=false;}	
	}

	private void FadeOutPanTouch()
	{
		fadeTouch = true;
	}

	private void FadeOutZoom()
	{
		if(fadeZoomInb){FadeZoomIn();fadeZoomInb=false;} 
		else if(fadeZoomOutb){FadeZoomOut();fadeZoomOutb=false;}
	}

	private void StopAll()
	{
		moveNb=false; moveSb=false; moveEb=false; moveWb=false;
		zoomInb = false; zoomOutb = false;	
		fadeTouch = false;
	}

	private void StopMove()
	{
		moveNb=false; moveSb=false; moveEb=false; moveWb=false;
	}
	private void StopZoom()
	{
		zoomInb = false; zoomOutb = false;	
	}
}