using UnityEngine;
using System.Collections;

public class SpriteFader : MonoBehaviour { //the gold coin / blue mana vial going up when the building is attacked

	public float alpha, fadeOutAfter, scale, deltaAlpha, deltaScale, ySpeed;

	private tk2dSprite sprite;
	private	tk2dTextMesh textMesh;

	private bool fadeOut = false;
	private float fadeOutCounter = 0;
	private int gainValue = 0;

	void Start () 
	{
		transform.parent = GameObject.Find("GroupEffects").transform;
		sprite = gameObject.GetComponentInChildren<tk2dSprite> ();
		textMesh = gameObject.GetComponentInChildren<tk2dTextMesh> ();
	}

	public void SetValue(int v)
	{
		gainValue = v;
		StartCoroutine ("SetVal");		
	}

	private IEnumerator SetVal()
	{
		yield return new WaitForSeconds (0.2f);
		textMesh.text = "+ " + gainValue.ToString ();
	}

	void FixedUpdate () {

		if (!fadeOut) 
		{
			fadeOutCounter += Time.deltaTime;
			if(fadeOutCounter > fadeOutAfter)
			{
				fadeOut=true;
			}
		}

		if (alpha > 0)
		{
			scale += deltaScale;	

			transform.position += new Vector3 (0, ySpeed,0);
			transform.localScale = new Vector3(scale,scale,1);

			if(fadeOut)
			{
				alpha += deltaAlpha;
				Color currentSpriteCollor = sprite.color;
				sprite.color = new Color(currentSpriteCollor.r, currentSpriteCollor.g, currentSpriteCollor.b, alpha);

				Color currentTextCollor = textMesh.color;
				textMesh.color = new Color(currentTextCollor.r, currentTextCollor.g, currentTextCollor.b, alpha);
			}

		}
		else
			Destroy(gameObject);			                                                        
	}
}