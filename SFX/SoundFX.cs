using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundFX : MonoBehaviour {		//attached to the camera, generates sounds

	private const int audioSourcesNo = 2;
	private int maxSounds = 0, currentSwitchingAudioSource = 0, nextSwitchingAudioSource = 1;

	private float musicVoume = 0.3f, maxMusicVoume = 0.3f, maxSoundVoume = 0.3f, currentDelta = -0.04f, nextDelta = 0.02f,
	thresholdUp = 0.99f, thresholdDown = 0.01f, transitionSpeed = 0.2f;

	private float[] switchingAudioSourceVolume = new float[audioSourcesNo]{0,0}; //day,night	

	private bool//make private 
	firstRunMusic = true, firstRunSound = true, firstRunAmbient = true,
	soundsTimerb = true, fadeIn = false, fadeOut = false, busy = false, isPlaying = true,
	inTransition = false, halfDone = false;

	//[HideInInspector]
	public bool musicOn = false, soundOn = true, ambientOn = true, weatherOn = false, dayTime = true, isWinter = false;
	//soundOn refers to sound effects  - shooting, explosions, and not ambient

	public AudioClip 
		buildingFinished, move, click, close, bip, end,
		soldierFire, soldierHit, soldierExplosion, 
		buildingExplosion, fireBurning,
		summerDayAmbient, summerNightAmbient, rainAmbient, snowAmbient, winterDayAmbient, winterNightAmbient;								

	private AudioSource soundSource, musicSource;
	private AudioSource[] switchingAudioSource = new AudioSource[audioSourcesNo];//one fades in, the other out 

	public AudioClip[] 
		hit = new AudioClip[10],
		die = new AudioClip[5],
		cannonFire = new AudioClip[2],
		copFire = new AudioClip[3],
		vortex = new AudioClip[2],
		typeWriter = new AudioClip[7];

	// Use this for initialization
	void Start () {

		AudioSource[] sources = GetComponents <AudioSource>();

		soundSource = sources [0];//shooting, explosions
		musicSource = sources [1];//music

		switchingAudioSource[0] = sources [2];//switching ambient forest birds daytime
		switchingAudioSource[1] = sources [3];//switching ambient night crickets

		for (int i = 1; i < sources.Length; i++) 
		{
			sources[i].ignoreListenerVolume = true;	
		}

		StartCoroutine ("LateInitializeAmbient");
	}
	private IEnumerator LateInitializeAmbient()
	{
		yield return new WaitForSeconds(10f);
		InitializeAmbient();
		//MusicOn ();
	}
	public void BattleMapSpecific()
	{
		InvokeRepeating ("SoundsTimer", 0.5f, 0.5f);
	}

	private void SoundsTimer()
	{
		soundsTimerb = !soundsTimerb;
	}

	public void MusicOn()//button called
	{
		if (!fadeIn && !busy) {
			fadeIn = true;
			busy = true;
			
			if (!isPlaying) {	
				isPlaying = true;
				musicSource.Play ();
			}
			musicOn = true;
			InvokeRepeating ("FadeInMusic", 0.2f, 0.2f);
		} 
		else 
		{		
			//StopAllCoroutines ();	
			StopCoroutine ("LateMusicOff");
			StartCoroutine ("LateMusicOn");	
		}

	}
	private IEnumerator LateMusicOn()
	{
		yield return new WaitForSeconds (2);
		MusicOn ();	
		//fadeIn = false;
	}


	public void MusicOff()//button called
	{		
		if (!fadeOut && !busy) {
			fadeOut = true;
			busy = true;
			musicOn = false;
			InvokeRepeating ("FadeOutMusic", 0.2f, 0.2f);
		} else 
		{
			//StopAllCoroutines ();	
			StopCoroutine ("LateMusicOn");
			StartCoroutine ("LateMusicOff");		
		}
	}

	private IEnumerator LateMusicOff()
	{
		yield return new WaitForSeconds (2);
		MusicOff ();	
		//fadeOut = false;
	}

	public void SoundOn() //button called
	{ 
		soundOn = true;
		soundSource.volume = maxSoundVoume;
		AudioListener.volume = maxSoundVoume;
	}
	
	public void SoundOff()//button called
	{
		soundOn = false;
		soundSource.volume = 0;
		AudioListener.volume = 0;
	}

	private void InitializeAmbient()
	{	
		switchingAudioSource [0].Play ();
		switchingAudioSource [1].Play ();	

		if (ambientOn) 
		{				
			InvokeRepeating ("StartTheDay", 0.2f, 0.2f);
		}
	}
	private void StartTheDay()//invoke repeating
	{
		SetSwitchingVolume (0, nextDelta);
		if (switchingAudioSourceVolume [0] >= 0.95f) 
		{
			CancelInvoke ("StartTheDay");
			//print ("StartTheDay invoke canceled");
		}
	}

	private void SetSwitchingVolume(int index, float delta)
	{
		switchingAudioSourceVolume[index] += delta;
		switchingAudioSourceVolume [index] = Mathf.Clamp (switchingAudioSourceVolume [index], 0, 1);
		switchingAudioSource [index].volume = switchingAudioSourceVolume [index];
	}

	private void SoundTransition()
	{	
		if (!halfDone) 
		{
			SetSwitchingVolume (currentSwitchingAudioSource, currentDelta);
			if (switchingAudioSourceVolume [currentSwitchingAudioSource] < thresholdDown) 
			{				
				switchingAudioSource[currentSwitchingAudioSource].Pause();
				halfDone = true;
			}
		} 
		else 
		{
			SetSwitchingVolume(nextSwitchingAudioSource, nextDelta);	
			if (switchingAudioSourceVolume [nextSwitchingAudioSource] > thresholdUp) 
			{	
				currentSwitchingAudioSource = nextSwitchingAudioSource;
				halfDone = false;
				inTransition = false;
				CancelInvoke ("SoundTransition");
				//print ("SoundTransition invoke canceled");
			}
		}
	}

	private IEnumerator DelayedSwitchSound(string soundClip)
	{
		yield return new WaitForSeconds (1.0f);
		SwitchSound (soundClip);
	}

	public void SwitchSound(string soundClip)
	{	
		if (!ambientOn) 
		{
			SwitchSoundClip(soundClip,currentSwitchingAudioSource);
			return;
		}
		
		if (inTransition) 
		{
			//accelerate running transition
			CancelInvoke ("SoundTransition");
			transitionSpeed = 0.05f;//finish running transition 4x faster
			InvokeRepeating("SoundTransition",transitionSpeed, transitionSpeed);
			transitionSpeed = 0.1f;//run next transition 2x faster
			StartCoroutine (DelayedSwitchSound(soundClip));
			return;
		}

		if (currentSwitchingAudioSource == 0)
			nextSwitchingAudioSource = 1;
		else
			nextSwitchingAudioSource = 0;


		//dayAmbient, nightAmbient, rainAmbient, snowAmbient;	
		SwitchSoundClip(soundClip,nextSwitchingAudioSource);
		/*

		switch (soundClip) 
		{
		case "summerday":
			switchingAudioSource [nextSwitchingAudioSource].clip = summerDayAmbient;
			break;
		case "summernight":
			switchingAudioSource [nextSwitchingAudioSource].clip = summerNightAmbient;
			break;
		case "winterday":
			switchingAudioSource [nextSwitchingAudioSource].clip = winterDayAmbient;
			break;
		case "winternight":
			switchingAudioSource [nextSwitchingAudioSource].clip = winterNightAmbient;
			break;
		case "rain":
			switchingAudioSource [nextSwitchingAudioSource].clip = rainAmbient;
			break;
		case "snow":
			switchingAudioSource [nextSwitchingAudioSource].clip = snowAmbient;
			break;
		}
		*/
		if (inTransition) 
		{
			//CancelInvoke ("SoundTransition");
			//halfDone = false;
		} 

		switchingAudioSource[nextSwitchingAudioSource].Play();

		halfDone = false;
		inTransition = true;
		InvokeRepeating("SoundTransition",transitionSpeed, transitionSpeed);
		transitionSpeed = 0.2f;		
	}

	private void SwitchSoundClip(string soundClip, int switchingAudioSourceIndex)
	{
		switch (soundClip) 
		{
		case "summerday":
			switchingAudioSource [switchingAudioSourceIndex].clip = summerDayAmbient;
			break;
		case "summernight":
			switchingAudioSource [switchingAudioSourceIndex].clip = summerNightAmbient;
			break;
		case "winterday":
			switchingAudioSource [switchingAudioSourceIndex].clip = winterDayAmbient;
			break;
		case "winternight":
			switchingAudioSource [switchingAudioSourceIndex].clip = winterNightAmbient;
			break;
		case "rain":
			switchingAudioSource [switchingAudioSourceIndex].clip = rainAmbient;
			break;
		case "snow":
			switchingAudioSource [switchingAudioSourceIndex].clip = snowAmbient;
			break;
		}
	}

	public void ToggleAmbient()
	{	
		if (firstRunAmbient) 
		{
			firstRunAmbient = false; 
			//InvokeRepeating ("StartTheDay", 0.2f, 0.2f); 
			return;
		}	

		ambientOn = !ambientOn;
		if (!ambientOn) 
		{
			if (IsInvoking ("SoundTransition")) 
			{
				CancelInvoke ("SoundTransition");
				halfDone = false;
				inTransition = false;
			}
			switchingAudioSource [currentSwitchingAudioSource].Pause ();
		} 
		else 
		{			
			if(!switchingAudioSource [currentSwitchingAudioSource].isPlaying)
			switchingAudioSource [currentSwitchingAudioSource].Play ();
			if(switchingAudioSource [currentSwitchingAudioSource].volume==0)				
				InvokeRepeating ("StartTheDay", 0.2f, 0.2f);
		}
	}

	public void ToggleMusic()
	{
		if (firstRunMusic) { firstRunMusic = false; return; }
		musicOn = !musicOn;
		ChangeMusic (musicOn);
	}

	public void ChangeMusic(bool b)//called from StatsBattle SaveLoadMap
	{
		if (b) {
			if (IsInvoking ("FadeOutMusic")) 
			{
				CancelInvoke ("FadeOutMusic");
				fadeOut = false;
				busy = false;
			}
			MusicOn ();
		} 
		else 
		{
			if (IsInvoking ("FadeInMusic")) 
			{
				CancelInvoke ("FadeInMusic");
				fadeIn = false;
				busy = false;
			}
			MusicOff ();
		}
	}

	public void ToggleSound()
	{
		if (firstRunSound) {firstRunSound = false; return;}

		soundOn = !soundOn;
		ChangeSound (soundOn);
	}

	public void ChangeSound(bool b)
	{		
		if (b) SoundOn();
		else SoundOff(); 
	}

	private void FadeInMusic()//invoke repeating
	{
		if(fadeIn)
		{
			if(musicVoume<maxMusicVoume)
			{
				musicVoume += 0.05f;
				musicVoume = Mathf.Clamp(musicVoume,0,maxMusicVoume);
				musicSource.volume = musicVoume;
			}
			else
			{
				fadeIn = false;
				busy = false;
				CancelInvoke ("FadeInMusic");
				//print ("FadeInMusic canceled");
			}			
		}
	}

	private void FadeOutMusic()//invoke repeating
	{
		if(fadeOut)
		{
			if(musicVoume>0)
			{
				musicVoume -= 0.05f;
				musicVoume = Mathf.Clamp(musicVoume,0,maxMusicVoume);
				musicSource.volume = musicVoume;
			}
			else
			{
				fadeOut = false;
				busy = false;
				musicSource.Pause();
				isPlaying = false;
				CancelInvoke ("FadeOutMusic");
				//print ("FadeOutMusic canceled");
			}			
		}
	}

	//called by buttons pressed in various 2dToolkit interfaces and played near the camera
	public void BuildingFinished()	{ soundSource.PlayOneShot(buildingFinished); }

	public void Move() { soundSource.PlayOneShot(move); }

	public void Click()	{ soundSource.PlayOneShot(click);	}

	public void Close()	{ soundSource.PlayOneShot(close);	}

	public void End()	{ soundSource.PlayOneShot(end);	}
	//Battle sounds

	public void CannonFire()	
	{
		int rand = Random.Range (0, 2);
		soundSource.PlayOneShot(cannonFire[rand]);	
	}

	public void BuildingBurn()
	{
		soundSource.loop = true; soundSource.clip = fireBurning; soundSource.Play();
		soundSource.PlayOneShot (fireBurning);
	}
	public void BuildingExplode() { soundSource.PlayOneShot(buildingExplosion);}

	public void SoldierPlace() { soundSource.PlayOneShot(bip);}

	public void SoldierFire()//this must be limited, otherwise the sound fades 	
	{ 
		if(soundsTimerb) 
		{
			soundSource.PlayOneShot (soldierFire);
			CountFireSounds();
		}
	}

	public void CopFire()
	{
		if(soundsTimerb) 
		{
			int rand = Random.Range (0, 3);
			soundSource.PlayOneShot(copFire[rand]);	
			CountFireSounds();
		}
	}

	private void CountFireSounds()
	{
		maxSounds++;
		if(maxSounds > 5)//allow 6 sounds total - coming from both dobbit/cop soldiers - otherwise fades
		{
			maxSounds = 0;
			soundsTimerb = false;
		}
	}

	public void SoldierHit()	
	{ 
		soundSource.PlayOneShot(soldierHit);
		int rand = Random.Range (0, 10);
		soundSource.PlayOneShot(hit[rand]);
	}

	public void SoldierExplode() { soundSource.PlayOneShot(soldierExplosion);}

	public void SoldierDie()
	{
		int rand = Random.Range (0, 5);
		soundSource.PlayOneShot (die[rand]);
		soundSource.PlayOneShot (vortex[rand%2]);
	}
	public void TypeWriter()
	{
		int rand = Random.Range (0, 7);
		soundSource.PlayOneShot (typeWriter[rand]);
	}


	/*
	void OnGUI()
	{
		if(GUI.Button(new Rect(15,Screen.height/2 -75,45,25),"Rain"))
		{
			

		}
		if(GUI.Button(new Rect(15,Screen.height/2 -45,45,25),"Snow"))
		{
			
		}
	}
	*/
}
