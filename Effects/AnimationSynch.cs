using UnityEngine;
using System.Collections;

public class AnimationSynch : MonoBehaviour {

	public tk2dSpriteAnimator animatorA, animatorB;


	// Use this for initialization
	void Start () {
		InvokeRepeating ("SynchAnimations", 5f, 5f);
	}

	private void SynchAnimations()
	{		
		animatorB.SetFrame (animatorA.CurrentFrame);
	}
}
