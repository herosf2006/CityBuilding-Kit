using UnityEngine;
using System.Collections;

public class SetLayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingLayerName = "Foreground";
	}
}
