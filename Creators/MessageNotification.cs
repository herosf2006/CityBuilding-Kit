using UnityEngine;
using System.Collections;

public class MessageNotification: MonoBehaviour{

	private const int elementsNo = 5;
	public int structureIndex;

	public GameObject notificationObj;
	public tk2dSprite[] sprites= new tk2dSprite[elementsNo];
	public UILabel[] labels = new UILabel[elementsNo];

	public float 
	alpha = 0, 
	deltaAlpha = 0.05f,
	ySpeed = 1.3f;

	private float currentDeltaAlpha;

	public bool isReady = false;

	void Start () {
		
	}
	public void SetLabel(int i, string s)
	{
		//0 harvest
		labels [i].text = s;
	}
	public void FadeIn()
	{
		currentDeltaAlpha = deltaAlpha;
		InvokeRepeating ("In", 0.02f, 0.02f);
	}

	private void In()
	{
		if (alpha < 1) 
		{			
			ChangeAlpha ();
		} 
		else
			CancelInvoke ("In");
	}

	public void FadeOut()
	{		
		currentDeltaAlpha = - deltaAlpha;
		InvokeRepeating ("Out", 0.02f, 0.02f);
	}

	private void Out()
	{
		if (alpha > 0) 
		{			
			ChangeAlpha ();
			notificationObj.transform.localPosition += new Vector3 (0, ySpeed,0);
		} 
		else 
		{
			CancelInvoke ("Out");
			notificationObj.transform.localPosition = Vector3.zero;
			//alpha = 1;
			//SetSpritesAlpha();
		}
	}

	private void ChangeAlpha()
	{		
		alpha += currentDeltaAlpha;
		alpha = Mathf.Clamp (alpha, 0, 1);
		SetSpritesAlpha();
	}

	private void SetSpritesAlpha()
	{
		for (int i = 0; i < sprites.Length; i++) 
		{
			sprites[i].color = new Color (1, 1, 1, alpha);			
		}
		for (int i = 0; i < labels.Length; i++) 
		{
			labels[i].color = new Color (1, 1, 1, alpha);
		}
	}


}
