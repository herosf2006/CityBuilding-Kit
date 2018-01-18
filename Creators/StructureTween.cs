using UnityEngine;
using System.Collections;

public class StructureTween : MonoBehaviour {		//briefly scales up and down the spites parent object when the building is selected

	private GameObject sprites;

	public float 
		tweenSpeed = 0.02f,
		size = 1,			
		initSize = 1,  
		maxSize =  1.1f;

	private bool 		
		scaleUpb;

	void Start () {

		sprites = transform.Find ("Sprites").gameObject; 	//find the sprites parent
	}

	public void Tween()
	{		
		scaleUpb = true;
		InvokeRepeating ("TweenScale", 0.02f, 0.02f);
	}


	void TweenScale()
	{
		
		if(scaleUpb)
		{
			size+= tweenSpeed;					//scale up
		}
		else
		{
			size-= tweenSpeed; 					//scale back down
		}

		if(size>maxSize) scaleUpb = false; 		//maximum size reached, time to scale down

		else if(size<initSize) 					//reached a size smaller than the initial size
		{									
			size = initSize; 					//reset the size to 1
			CancelInvoke("TweenScale");			//end the scale sequence 
		}

		sprites.transform.localScale = new Vector3(size,size,1);	//pass the scale values to the sprites parent

	}
}