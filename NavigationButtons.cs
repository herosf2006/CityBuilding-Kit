using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationButtons:MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{

		if (Application.platform == RuntimePlatform.Android)
		{
			gameObject.SetActive(false);	
		}
	}
}
