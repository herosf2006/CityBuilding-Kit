using UnityEngine;
using System.Collections;

public class Scale : MonoBehaviour {		//scales some of the particle effects

	public ParticleEmitter[] emitters;
	public float scale = 1;

	private float[] minSize,  maxSize;
	private Vector3[] worldVelocity, localVelocity, rndVelocity, scaleBackUp;

	void Start () {

		ScaleEmitters ();

	}

	private void ScaleEmitters()
	{
		int length = emitters.Length;
		
		minSize = new float[length];
		maxSize = new float[length];
		worldVelocity = new Vector3[length];
		localVelocity = new Vector3[length];
		rndVelocity = new Vector3[length];
		scaleBackUp = new Vector3[length];
		
		for (int i = 0; i < length; i++) 
		{ 
			minSize[i] = emitters[i].minSize;
			maxSize[i] = emitters[i].maxSize;
			worldVelocity[i] = emitters[i].worldVelocity;
			localVelocity[i] = emitters[i].localVelocity;
			rndVelocity[i] = emitters[i].rndVelocity;
			scaleBackUp[i] = emitters[i].transform.localScale;
			
			emitters[i].minSize = minSize[i] * scale;
			emitters[i].maxSize = maxSize[i] * scale;
			emitters[i].worldVelocity = worldVelocity[i] * scale;
			emitters[i].localVelocity = localVelocity[i] * scale;
			emitters[i].rndVelocity = rndVelocity[i] * scale;
			emitters[i].transform.localScale = scaleBackUp[i] * scale;			
		}
	}
}