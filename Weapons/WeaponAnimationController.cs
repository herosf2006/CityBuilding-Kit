using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponAnimationController : MonoBehaviour { 
				
	/*
		Toolkit 2D animated structures have to be instantiated in-game, 
		not manually placed in the scene with the editor; 
		Otherwise, the event-based animations do not work properly.
	*/

	public tk2dSpriteAnimator spriteAnimator;

	private string 
		clockwiseExtension = "Clockwise",
		counterClockwiseExtension = "CounterClockwise",
		fireAnimationClip;

	private string 		
		rotationClip = "rotate",		
		currentClip; 				

	public int 
	fireAngles = 11,//if there are 12 angles (0-11), put this at 11
	previousFrame = 0;

	private bool 
		previousClockwise = true, 
		rotatingClockwise = false, 
		rotatingCounterClockwise = false,
		isFiring = false,
		justFired = false,
		newFire = false;
		
	private int 
		currentFrame, 		//handle as clockwise
		eventFrame,			//handle as clockwise 		
		anglesNo;			//used for rotating in the apropriate direction (avoid rotating "the long way")  

	public GameObject targetOb;

	private Component turretController, sextant; 

	// Use this for initialization
	void Start () {

		turretController = GetComponent<TurretControllerFrameEvent> ();
		sextant = transform.GetComponentInChildren<Sextant> ();
		anglesNo = ((Sextant)sextant).angles.Length;

		spriteAnimator.AnimationEventTriggered += AimAtTarget;

		RotateClockwise();//initialize animations
		Stop ();
	}

	private void FireWeapon()//frame 2 = frame 6
	{
		isFiring = true;

		int frameIndex;
		if (previousClockwise) frameIndex = previousFrame; 
		else frameIndex = fireAngles - previousFrame; 
		fireAnimationClip = "fire" + frameIndex;
		spriteAnimator.Play(fireAnimationClip);
		FinishFire();	
	}

	private void FinishFire()
	{
		((TurretControllerFrameEvent)turretController).Fire();
		justFired = true;	
		isFiring = false;
		spriteAnimator.AnimationCompleted = null;
	}

	private void TranslateFrame(int frameNo)
	{			
		//The clockwise and counterclockwise animation clips skip a frame when changing direction
		if(previousClockwise)
			currentFrame = frameNo;
		else 
			currentFrame = fireAngles-frameNo;
	}

	void  AimAtTarget(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		TranslateFrame (frameNo);

		if(currentFrame == eventFrame && newFire)
		{
			newFire = false;	
			Fire ();			
		}
	}

	private IEnumerator DelayedFire()
	{
		yield return new WaitForSeconds (0.1f);	
		newFire = false;
		Fire ();
	}

	public void RotateAndFire(int newEventFrame)
	{
		if (isFiring||newFire)
			return;

		spriteAnimator.AnimationEventTriggered +=  AimAtTarget;

		((TurretControllerFrameEvent)turretController).Aim ();
		eventFrame = newEventFrame;

		if (eventFrame == currentFrame)//fire a second time at the same angle
		{	
			StartCoroutine ("DelayedFire");
			newFire = true;
			return;
		}

		bool isClockwise = true;
		int deltaFrame = eventFrame - currentFrame;//receives a new fireat frame
		if(deltaFrame<0) isClockwise = false;

		if (currentFrame>anglesNo/2 && eventFrame<anglesNo/4||//6 3
			(currentFrame<anglesNo/4 && eventFrame>anglesNo/2))//3 6 
		{
			isClockwise = !isClockwise;
		}

		if (isClockwise){ RotateClockwise (); }
		else { RotateCounterClockwise (); }

		newFire = true;	
	}

	private void RotateClockwise()
	{	
		if (rotatingClockwise||isFiring) return;

		bool hasFired = false;

		if (!previousClockwise)
		{
			previousClockwise = true;
			if(!justFired) ConvertFrames ();
			else { hasFired = true; }
		}

		PlayClip (clockwiseExtension);

		if (hasFired) 
		{
			ConvertFrames();
			PlayClip (clockwiseExtension);
		}

		rotatingClockwise = true; rotatingCounterClockwise = false; justFired = false; 			
	}

	private void RotateCounterClockwise()
	{
		if (rotatingCounterClockwise||isFiring) return;

		bool hasFired = false;

		if (previousClockwise)
		{
			previousClockwise = false;
			if(!justFired) ConvertFrames ();
			else { hasFired = true; }
		}

		PlayClip (counterClockwiseExtension);

		if (hasFired) 
		{
			ConvertFrames();		
			PlayClip (counterClockwiseExtension);
		}

		rotatingCounterClockwise = true; rotatingClockwise = false; justFired = false;
	}

	private void PlayClip(string ext)
	{		
		currentClip = rotationClip+ext;
		spriteAnimator.PlayFromFrame(currentClip,previousFrame);
	}

	private void ConvertFrames()
	{		
		previousFrame = fireAngles- spriteAnimator.CurrentFrame;	
	}

	private void RegisterFrame()
	{		
		spriteAnimator.Stop();	
		previousFrame = spriteAnimator.CurrentFrame;	
	}

	private void Stop()
	{
		if (justFired||isFiring) return;	
		rotatingCounterClockwise = false; 
		rotatingClockwise = false;
		RegisterFrame ();
	}

	private void Fire()
	{
		if (isFiring)	return;
		Stop();
		FireWeapon();
	}

	public void StopAnimations()
	{
		StartCoroutine ("LateStopAnimations");
	}

	private IEnumerator LateStopAnimations()
	{
		yield return new WaitForSeconds (0.5f);
		spriteAnimator.Stop();	
	}
		
}


