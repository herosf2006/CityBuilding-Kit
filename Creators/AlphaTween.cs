using UnityEngine;
using System.Collections;

public class AlphaTween : MonoBehaviour {

	private tk2dBaseSprite sprite;

	private int bw = 1;//1 white 0 black
	public float minAlpha = 0, maxAlpha = 1;
	public float transitionSpeed = 0.1f, threshold = 0.99f;
	public bool 
		isVisible = false,			//visible/invisible for snow or maxAlpha for shadows - for lerp direction fade-in or fade-out
		inTransition = false;
	private float startTime, startAlpha, endAlpha, transitionLength = 1;

	void Start () 
	{		
		GetSpriteType ();		
	}

	private void GetSpriteType()
	{
		if (GetComponent<tk2dClippedSprite>()!=null)
			sprite = GetComponent<tk2dClippedSprite> ();
		else if (GetComponent<tk2dTiledSprite>()!=null)
			sprite = GetComponent<tk2dTiledSprite> ();
		else //if (GetComponent<tk2dSprite>()!=null)
			sprite = GetComponent<tk2dSprite> ();
	}

	private IEnumerator RepeatDelayFade(bool visible, int blackOrWhite)
	{
		yield return new WaitForSeconds (2.0f);
		FadeAlpha (visible, blackOrWhite);
	}

	public void FadeAlpha(bool visible, int blackOrWhite)//visible means maximum alpha - 1 for snow, 0.35 for shadows
	{	
		if (visible == isVisible)//for newly built structures with no snow on them OR same command repeated - next iteration catches up
			return;
		
		if (inTransition)
			StartCoroutine (RepeatDelayFade (visible, blackOrWhite));

		inTransition = true;

		bw = blackOrWhite;

		if (isVisible) 
		{ 
			startAlpha = maxAlpha;
			endAlpha = minAlpha;
		}
		else 
		{//startAlpha = maxAlpha; endAlpha = minAlpha;
			startAlpha = minAlpha;
			endAlpha = maxAlpha;	
		}

		startTime = Time.time;		
		InvokeRepeating ("RunTransition", 0.1f, 0.1f);
	}

	private void RunTransition()
	{
		float transitionCovered = (Time.time - startTime) * transitionSpeed;
		float transitionFraction = transitionCovered / transitionLength;
		float alpha = Mathf.Lerp (startAlpha, endAlpha, transitionFraction);//lerp is clamped to 0,1
		Color currentColor = new Color (bw, bw, bw, alpha);

		sprite.color = currentColor;

		if (transitionFraction >= threshold)
		{				
			CancelTransition ();
			//print ("AlphaTween invoke canceled");//checked OK
			//always verify if these invokes get canceled; otherwise they will run permanently if some condition is never met
		}
	}

	public void CancelTransition()
	{
		CancelInvoke ("RunTransition");
		isVisible = !isVisible;
		inTransition = false;
	}

}


