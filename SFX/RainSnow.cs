using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
using System;

public class RainSnow : MonoBehaviour {

	public bool transitionOn = true;
	private bool clearWeather = true, isWinter = false;

	private int 
	winterSwitchCounter = 0, //after 5 rain/snow cycles switches to winter/summer
	slowSnowFadeIndex = 0,
	slowShadowFadeIndex = 0;

	public float transitionSpeed = 0.1f, transitionInterval = 0.5f, snowOpacity = 1; //transition interval in minutes
	private float threshold = 0.99f, startTimeVignette, startTimeSnowTerrain, transitionLength = 1, startAlpha, endAlpha, startSnowAlpha, endSnowAlpha;


	public UISprite rainVignete;
	public GameObject terrainSnow;
	public EllipsoidParticleEmitter rainDrops, rainSplash, snowFlakes;

	private Color color;
	private MeshRenderer[] terrainMeshes;
	private Material[] terrainSnowMaterials;


	private GameObject[] snowSprites, shadowSprites;

	public SoundFX soundFX;

	void Start () 
	{
		if(transitionOn)
		{
			color = rainVignete.color;			
			GetSnowTerrainMaterials();

			SetSnowColor(new Color(1.0f, 1.0f, 1.0f, 0.0f));

			InvokeRepeating("Transition",transitionInterval*60,transitionInterval*60);//convert seconds
			terrainSnow.SetActive(false);
		}
	}

	private void GetSnowTerrainMaterials()
	{
		terrainMeshes = terrainSnow.GetComponentsInChildren<MeshRenderer> ();
		terrainSnowMaterials = new Material[terrainMeshes.Length];

		for (int i = 0; i < terrainMeshes.Length; i++) 
		{
			terrainSnowMaterials [i] = terrainMeshes [i].sharedMaterial;
		}
	}

	private void Transition()
	{
		SwitchClearRain ();
	}

	private IEnumerator TestTransition()
	{
		yield return new WaitForSeconds (10);
		SwitchClearRain ();
	}

	public void SwitchClearRain()
	{
		clearWeather = ! clearWeather;
		if (!clearWeather) 
		{			
			rainVignete.gameObject.SetActive (true);
			//you can just erase this so the winter doesn't cycle
			winterSwitchCounter++;
			if (winterSwitchCounter == 3) //rains 3 times, snows 3 times
			{
				terrainSnow.SetActive(true);
				isWinter = !isWinter;
				soundFX.isWinter = isWinter;
				winterSwitchCounter = 0;
				if(!isWinter)
					StartCoroutine("FadeSnowOnSprites");//turn winter off	
				ToggleTerrainSnow();
			}
			if(isWinter)
			{
				StartCoroutine("FadeSnowOnSprites");//turn/update winter on
				//embarassing, but I can't think of a way to avoid/simplify this duplicate
			}
			else
			{
				terrainSnow.SetActive(false);
			}
		}
		FadeShadows ();
		startTimeVignette = Time.time;
		ReadyParameters ();
		//transitionLength = 1;
		InvokeRepeating ("RunTransition", 0.02f, 0.02f);
	}
	private void ReadyParameters()
	{
		if (clearWeather) 
		{	
			ResetSound();
			startAlpha = 1; endAlpha = 0;
			if (rainDrops.emit) 
			{
				rainDrops.emit = false;
				rainSplash.emit = false;
			}
			else if(snowFlakes.emit)
				snowFlakes.emit = false;
		} 
		else 
		{	
			soundFX.weatherOn = true;
			startAlpha = 0; endAlpha = 1;
		}
	}

	private void RunTransition()
	{
		float transitionCovered = (Time.time - startTimeVignette) * transitionSpeed;
		float transitionFraction = transitionCovered / transitionLength;
		float alpha = Mathf.Lerp (startAlpha, endAlpha, transitionFraction);//lerp is clamped to 0,1
		Color currentColor = new Color (color.r, color.g, color.b, alpha);
		rainVignete.color = currentColor;

		if (transitionFraction >= threshold)
		{
			if (clearWeather) {
				rainVignete.gameObject.SetActive (false);
			} 
			else 
			{		
				if(isWinter)
					soundFX.SwitchSound ("snow");
				else
					soundFX.SwitchSound ("rain");

				StartCoroutine ("StartRain");
			}
			CancelInvoke ("RunTransition");
			//print ("RunTransition vignete invoke canceled");//always verify if these invokes get canceled; otherwise they will run permanently
		}
	}

	private IEnumerator StartRain()
	{
		yield return new WaitForSeconds (5.0f);
		if (isWinter)
			snowFlakes.emit = true;
		else 
		{
			rainDrops.emit = true;
			rainSplash.emit = true;
		}
	}
	private void ResetSound()
	{

		soundFX.weatherOn = false;
		if (soundFX.dayTime) 
		{
			if (isWinter) {
				soundFX.SwitchSound ("winterday"); //print ("winterday");
			} else {
				soundFX.SwitchSound ("summerday"); //print ("summerday");
			}
		} 
		else 
		{
			if (isWinter) {
				soundFX.SwitchSound ("winternight"); //print ("winternight");
			} else {
				soundFX.SwitchSound ("summernight"); //print ("summernight");
			}
		}

	}

	private IEnumerator FadeSnowOnSprites()
	{
		yield return new WaitForSeconds (20);	
		snowSprites = GameObject.FindGameObjectsWithTag ("Snow");
		SortSnowSprites ();
		//if(IsInvoking("FadeSnowSlowSequence"))
		//print("already running");
		InvokeRepeating ("FadeSnowSlowSequence", 0.2f, 0.2f);
	}

	private void FadeShadows()
	{
		shadowSprites = GameObject.FindGameObjectsWithTag ("Shadow");
		SortShadowSprites ();
		//if(IsInvoking("FadeShadowSlowSequence"))
		//print("already running");
		InvokeRepeating ("FadeShadowSlowSequence", 0.2f, 0.2f);
	}

	private void SortSnowSprites()
	{			
		Array.Sort (snowSprites, delegate(GameObject gsn1, GameObject gsn2) {
			return gsn1.transform.position.x.CompareTo (gsn2.transform.position.x);			
		});
	}
	private void SortShadowSprites()
	{	
		Array.Sort (shadowSprites, delegate(GameObject gsh1, GameObject gsh2) {
			return gsh1.transform.position.x.CompareTo (gsh2.transform.position.x);			
		});
	}

	private void ToggleTerrainSnow()
	{
		if (isWinter) 
		{
			startSnowAlpha = 0;
			endSnowAlpha = snowOpacity;
		} 
		else 
		{
			startSnowAlpha = snowOpacity;
			endSnowAlpha = 0;
		}
		StartCoroutine ("LateFadeSnowTerrain");
	}


	private IEnumerator LateFadeSnowTerrain()
	{
		yield return new WaitForSeconds (40);
		startTimeSnowTerrain = Time.time;

		//if(IsInvoking("FadeSnowTerrain"))
		//print("already running");

		InvokeRepeating("FadeSnowTerrain", 0.3f, 0.3f);
	}
	private void FadeSnowTerrain()
	{
		float transitionCovered = (Time.time - startTimeSnowTerrain) * transitionSpeed;
		float transitionFraction = transitionCovered / transitionLength;
		float alpha = Mathf.Lerp (startSnowAlpha, endSnowAlpha, transitionFraction);//lerp is clamped to 0,1
		Color currentColor = new Color (1, 1, 1, alpha);

		SetSnowColor(currentColor);

		if (transitionFraction >= threshold)
		{				
			CancelInvoke ("FadeSnowTerrain");
			//print ("terrain snow fade invoke canceled");//checked OK?

		}
	}

	private void SetSnowColor(Color col)
	{
		for (int i = 0; i < terrainSnowMaterials.Length; i++) 
		{
			terrainSnowMaterials [i].color = col;	
		}
	}

	private void FadeSnowSlowSequence()
	{		
		if (slowSnowFadeIndex < snowSprites.Length) 
		{
			if (snowSprites [slowSnowFadeIndex] != null) //verify if for some reason the object no longer exists - destroyed on battle map and replaced with ruins?
			{		
				snowSprites [slowSnowFadeIndex].GetComponent<AlphaTween> ().FadeAlpha (isWinter, 1);
				slowSnowFadeIndex++;
			}
		} 
		else 
		{
			CancelInvoke ("FadeSnowSlowSequence");
			slowSnowFadeIndex = 0;
			//print ("slowSnowFadeIndex invoke canceled"); //checked OK
		}
	}
	private void FadeShadowSlowSequence()
	{
		if (slowShadowFadeIndex < shadowSprites.Length) 
		{
			if (shadowSprites [slowShadowFadeIndex] != null) //verify if for some reason the object no longer exists - destroyed on battle map and replaced with ruins?
			{		
				shadowSprites [slowShadowFadeIndex].GetComponent<AlphaTween> ().FadeAlpha (clearWeather, 0);
				slowShadowFadeIndex++;
			}
		} 
		else 
		{
			CancelInvoke ("FadeShadowSlowSequence");
			slowShadowFadeIndex = 0;
			//print ("slowShadowFadeIndex invoke canceled"); //checked OK
		}
	}

}
