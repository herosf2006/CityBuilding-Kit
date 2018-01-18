// Attach this to a GUIText to make a frames/second indicator.
//
// It calculates frames/second over each updateInterval,
// so the display does not keep changing wildly.
//
// It is also fairly accurate at very low FPS counts (<10).
// We do this not by simply counting frames per interval, but
// by accumulating FPS for each frame. This way we end up with
// correct overall FPS even if the interval renders something like
// 5.5 frames.
 
var updateInterval = 1.0;
private var accum = 0.0; 		// FPS accumulated over the interval
private var frames = 0; 		// Frames drawn over the interval
private var timeleft : float;	// Left time for current interval
private var fps = 15.0; 		// Current FPS
private var lastSample : double;
private var gotIntervals = 0;

private var hours : int; 
private var minutes : int;
private var seconds : int;

private var realTimeFloat : float;
private var realTimeText : String ;

function Start()
{
    timeleft = updateInterval;
    lastSample = Time.realtimeSinceStartup;
    InvokeRepeating("UpdateTimeCounter",1,1);
}

function GetFPS() : float { return fps; }
function HasFPS() : boolean { return gotIntervals > 2; }
 
function Update()
{
    ++frames;
    var newSample = Time.realtimeSinceStartup;
    var deltaTime = newSample - lastSample;
    lastSample = newSample;

    timeleft -= deltaTime;
    accum += 1.0 / deltaTime;
    
    // Interval ended - update GUI text and start new interval
    if( timeleft <= 0.0 )
    {
        // display two fractional digits (f2 format)
        fps = accum/frames;
		// guiText.text = fps.ToString("f2");
        timeleft = updateInterval;
        accum = 0.0;
        frames = 0;
        ++gotIntervals;
    }
}

function OnGUI()
{
	GUI.Box(new Rect(Screen.width/2 - 75, Screen.height -20, 70, 25), fps.ToString("f2") + " fps");// | QSetting: " + QualitySettings.currentLevel

	GUI.Box(new Rect(Screen.width/2 + 5, Screen.height -20, 70, 25), realTimeText);
}



function UpdateTimeCounter()				//calculate remaining time
	{
		realTimeFloat = Time.realtimeSinceStartup;

		hours = realTimeFloat/3600 ; 
		minutes = realTimeFloat/60;
		seconds= realTimeFloat%60;

		if (minutes == 60) minutes = 0;
		if (seconds == 60) seconds = 0;

		UpdateTimeLabel ();
	}

function UpdateTimeLabel()									//update the time labels on top
	{
		if(hours>0 && minutes >0 && seconds>=0 )
		{			

			realTimeText = 
				hours.ToString() +" h " +
					minutes.ToString() +" m " +
					seconds.ToString() +" s";			
		}
		else if(minutes > 0 && seconds >= 0)
		{
			realTimeText = 
				minutes.ToString() +" m " +
					seconds.ToString() +" s";
			
		}
		else if(seconds > 0 )
		{
			realTimeText  = 
				seconds.ToString() +" s";
		}

	}