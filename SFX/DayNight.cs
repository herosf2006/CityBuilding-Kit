using UnityEngine;
using System.Collections;

public class DayNight : MonoBehaviour {

	public bool transitionOn = true;
	private bool daytime = true;//daytime

	public float transitionSpeed = 0.1f, transitionInterval = 0.5f;//transitionInterval in minutes switches between states
	private float startTime, transitionLength, threshold = 0.99f;

	public tk2dCamera nightCam;
	public Color dayColor, nightColor;

	private Vector3 startPoint, endPoint;

	public SoundFX soundFX;

	void Start () {	
		if(transitionOn)
		InvokeRepeating("Transition",transitionInterval*60,transitionInterval*60);//convert seconds
	}

	private void Transition()
	{
		SwitchDayNight ();
	}

	private IEnumerator TestTransition()
	{
		yield return new WaitForSeconds (10);
		SwitchDayNight ();
	}

	public void SwitchDayNight()
	{
		daytime = ! daytime;
		bool isWinter = soundFX.isWinter;

		string nightOrDay;
		if (daytime) 
		{
		if (isWinter)
			nightOrDay = "winterday";
		else
			nightOrDay = "summerday";
		} 
		else 
		{
		if(isWinter)
			nightOrDay = "winternight";
		else
			nightOrDay ="summernight";
		}
		startTime = Time.time;
		ReadyParameters ();
		transitionLength = Vector3.Distance (startPoint, endPoint);
		soundFX.dayTime = daytime;
		if (!soundFX.weatherOn) 
		{
			soundFX.SwitchSound (nightOrDay);
		}
		InvokeRepeating ("RunTransition", 0.02f, 0.02f);
	}
	private void ReadyParameters()
	{
		if (daytime) 
		{			
			startPoint = new Vector3 (nightColor.r, nightColor.g, nightColor.b);
			endPoint = new Vector3 (dayColor.r, dayColor.g, dayColor.b);
		} 
		else 
		{		
			startPoint = new Vector3 (dayColor.r, dayColor.g, dayColor.b);
			endPoint = new Vector3 (nightColor.r, nightColor.g, nightColor.b);	

		}
	}

	private void RunTransition()
	{
		float transitionCovered = (Time.time - startTime) * transitionSpeed;
		float transitionFraction = transitionCovered / transitionLength;
		Vector3 v = Vector3.Lerp (startPoint, endPoint, transitionFraction);//lerp is clamped to 0,1
		Color currentColor = new Color (v.x, v.y, v.z);
		((tk2dCamera)nightCam).ScreenCamera.backgroundColor = currentColor;
		if (transitionFraction >= threshold)
		{
			CancelInvoke ("RunTransition");
			//print ("RunTransition camera invoke canceled");
		}
	}
}
