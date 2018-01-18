using UnityEngine;
using System.Collections;

public class AnimController : MonoBehaviour {  //animation controller for dobbits/soldiers

	tk2dSpriteAnimator animator;

	//public tk2dSpriteAnimation attackAnimation, buildAnimation, idleAnimation, walkAnimation;

	public tk2dSpriteAnimation spriteAnimation;
	
	private string action,direction;

	public string soldierType = "";//EventTrigger

	private Component soundFX; 

	void Start () 
	{
		animator = GetComponent<tk2dSpriteAnimator>();

		action = "Walk";
		direction = "W";

		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();// //connects to SoundFx - a sound source near the camera

		if (soldierType == "EventTrigger")
		{
			animator.AnimationEventTriggered += FireWeapon;
		}

	}

	//order: change anim / turn /update char anim 


	public void ChangeAnim(string anim)
	{
		action = anim; 
		/*
		switch (anim) 
		{
		case "Idle":
			animator.Library = idleAnimation;
			break;
		case "Walk":
			animator.Library = walkAnimation;
			break;
		case "Attack":
			animator.Library = attackAnimation;
			break;
		case "Build":
			animator.Library = buildAnimation;	
			break;
		}	
		*/
	}


	public void Turn(string dir){direction = dir; }

	public void UpdateCharacterAnimation()
	{
		animator.Play(action + "_" + direction);
	}


	void  FireWeapon(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{

		if (soldierType == "EventTrigger")
		((SoundFX)soundFX).CopFire ();
		//.CannonFire ();
	}


}

