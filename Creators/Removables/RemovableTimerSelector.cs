using UnityEngine;
using System.Collections;

public class RemovableTimerSelector : RemovableBase {//attached to each building as an invisible 2dtoolkit button

	public UILabel TimeCounterLb;

	public GameObject 
		Price, 					//own child obj - has the price label
		ProgressBar, 			//own child obj
		GainCrystals,
		GainExperience;

	public bool 
		inRemovalB = false;		

	private int 
		hours, 
		minutes, 
		seconds,				//for time remaining label
		finishPrice;			//price displayed for "finish now" button. based on remaining time

	public int 
		crystalAward,
		xpAward,
		remainingTime = 1;
	public string removableName;

	public float progTime = 0.57f, progCounter = 0;		//for progress timer, one minute

	// Use this for initialization
	void Start () {

		//tween = GetComponent<StructureTween> ();
		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();
		relay = GameObject.Find("Relay").GetComponent<Relay>();
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();


		if(!battleMap)
		{
			removableCreator = GameObject.Find("RemovableCreator").GetComponent<RemovableCreator>();
			stats = GameObject.Find("Stats").GetComponent<Stats>();
		}

		//init price so user can't click fast on price 0
		remainingTime = removeTime - 1;
		UpdatePrice (remainingTime);
	}

	void FixedUpdate()
	{
		if(inRemovalB)
		{
			ProgressBarUpdate();
		}
	}

	private void ProgressBarUpdate()
	{		
		progCounter += Time.deltaTime*0.5f;
		if(progCounter > progTime)
		{
			progCounter = 0;
			
			((UISlider)ProgressBar.GetComponent("UISlider")).value += (float)(Time.deltaTime/removeTime);				//update progress bars values
			
			((UISlider)ProgressBar.GetComponent("UISlider")).value=
				Mathf.Clamp(((UISlider)ProgressBar.GetComponent("UISlider")).value,0,1);
			
			remainingTime = (int)(removeTime * (1 - ((UISlider)ProgressBar.GetComponent("UISlider")).value));
			
			UpdatePrice (remainingTime);
			UpdateTimeCounter(remainingTime);
			
			if(((UISlider)ProgressBar.GetComponent("UISlider")).value == 1)				//building finished - the progress bar has reached 1												
			{
				((SoundFX)soundFX).BuildingFinished();
				
				if(!battleMap)															//if this building is not finished on a battle map
				{
					((Stats)stats).occupiedDobbits--;									//the dobbit previously assigned becomes available

					if (crystalAward > 0) 
					{
						((Stats)stats).AddResources (0, 0, crystalAward);
						((Messenger)statusMsg).DisplayMessage("You found "+ crystalAward.ToString()+" crystals in " + removableName );
						GameObject gainCrystalIco = (GameObject)Instantiate(GainCrystals, transform.position+new Vector3(50,0,0), Quaternion.identity);
						gainCrystalIco.GetComponent<FadeOutGain> ().SetValue (crystalAward);//pass the value to the label
					}		
					((Stats)stats).experience += xpAward;	
					GameObject gainExperienceIco = (GameObject)Instantiate(GainExperience, transform.position+new Vector3(-50,0,0), Quaternion.identity);
					gainExperienceIco.GetComponent<FadeOutGain> ().SetValue (xpAward);//pass the value to 
					((Stats)stats).UpdateUI();
				}
				inRemovalB = false;				
				Destroy(this.transform.parent.gameObject);				
			}			
		}			
	}
	
	private void UpdateTimeCounter(int remainingTime)				//calculate remaining time
	{
		hours = (int)remainingTime/60; 
		minutes = (int)remainingTime%60;
		seconds = (int)(60 - (((UISlider)ProgressBar.GetComponent("UISlider")).value*removeTime*60)%60);	
		
		if (minutes == 60) minutes = 0;
		if (seconds == 60) seconds = 0;
		
		UpdateTimeLabel ();
	}
	
	private void UpdateTimeLabel()									//update the time labels on top
	{
		if(hours>0 && minutes >0 && seconds>=0 )
		{			
			
			((UILabel)TimeCounterLb).text = 
				hours.ToString() +" h " +
					minutes.ToString() +" m " +
					seconds.ToString() +" s ";			
		}
		else if(minutes > 0 && seconds >= 0)
		{
			((UILabel)TimeCounterLb).text = 
				minutes.ToString() +" m " +
					seconds.ToString() +" s ";
			
		}
		else if(seconds > 0 )
		{
			((UILabel)TimeCounterLb).text = 
				seconds.ToString() +" s ";
		}
		
	}
		
	private void UpdatePrice(int remainingTime)					//update the price label on the button, based on remaining time		
	{
		/*
		0		30		1
		30		60		3
		60		180		7
		180		600		15
		600		1440	30
		1440	2880	45
		2880	4320	70
		4320			150
		 */
		
		if (remainingTime >= 4320) { finishPrice = 150; }
		else if (remainingTime >= 2880) { finishPrice = 70; }
		else if (remainingTime >= 1440) { finishPrice = 45;	}
		else if (remainingTime >= 600)	{ finishPrice = 30;	}
		else if (remainingTime >= 180) { finishPrice = 15; }
		else if (remainingTime >= 60) { finishPrice = 7; }
		else if (remainingTime >= 30) {	finishPrice = 3; }
		else if(remainingTime >= 0) { finishPrice = 1; }
		
		((tk2dTextMesh)Price.GetComponent("tk2dTextMesh")).text = finishPrice.ToString();
	}
	
	public void Finish()									
	{
		if (!((Relay)relay).pauseInput && !((Relay)relay).delay)  //panels are open / buttons were just pressed 
		{
			((SoundFX)soundFX).Click();
			if (((Stats)stats).crystals >= finishPrice) 
			{
				((Stats)stats).crystals -= finishPrice;
				((Stats)stats).UpdateUI();
				((UISlider)ProgressBar.GetComponent ("UISlider")).value = 1;
			} 
			else 
			{				
				((Messenger)statusMsg).DisplayMessage("Insufficient crystals");
			}
		}
	}
}
