using UnityEngine;
using System.Collections;

public class Dissolve : MonoBehaviour {

	public UISprite 
		blackDissolve;

	public TweenAlpha 
		tweenDissolve;

	private bool flipping = true; 	//flips the dissolve mask, to give the impression of static

	public bool	halfFade;			//also used as flag for garbled communications					

	private float 
		flippTimer, 
		flippTime = 0.2f;

	private int flipIndex = -1;

	public void FadeFullIn()
	{
		flipping = true;
		((TweenAlpha)tweenDissolve).PlayForward ();
	}

	public void FadeHalfOut()
	{
		StopCoroutine ("FadeBackIn");

		((TweenAlpha)tweenDissolve).PlayReverse ();
		flipping = true;
		halfFade = true;
	}

	void Update () {
	if (flipping)
		{
			flippTimer += Time.deltaTime;

			if(flippTimer>flippTime)
			{
				flipIndex++;

				if(flipIndex == 4) flipIndex = 0;

				switch (flipIndex) 
				{
				case 0:
					((UISprite)blackDissolve).flip = UIBasicSprite.Flip.Horizontally;				
					break;
				case 1:
					((UISprite)blackDissolve).flip = UIBasicSprite.Flip.Vertically;
					break;
				case 2:
					((UISprite)blackDissolve).flip = UIBasicSprite.Flip.Both;	
					break;
				case 3:
					((UISprite)blackDissolve).flip = UIBasicSprite.Flip.Nothing;
					break;
				}
											
				if(halfFade)
				{
					if (((TweenAlpha)tweenDissolve).value >= 0.9f)
					{
						StartCoroutine("FadeBackIn");
						halfFade = false;
					}				
				}

				else if (((TweenAlpha)tweenDissolve).value == 0)
				{
					flipping = false;						//stop the flipping static
				}

				flippTimer = 0;
			}
		}
	}

	private IEnumerator FadeBackIn()
	{
		yield return new WaitForSeconds (1.0f);
		FadeFullIn();
	}

}
