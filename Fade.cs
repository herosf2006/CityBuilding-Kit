using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade:MonoBehaviour
{

	[SerializeField] 
	private float fadeSpeed = 0.0f;

	public static Fade Instance;

	void Awake()
	{
		Instance = this;
	}
	
	
	public IEnumerator FadeTo(float aValue, float aTime)
	{
		CanvasGroup cg = GetComponent<CanvasGroup>();
		float alpha = cg.alpha;
		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
		{
			cg.alpha = Mathf.Lerp(alpha,aValue,t);
			yield return null;
		}
	}
}
