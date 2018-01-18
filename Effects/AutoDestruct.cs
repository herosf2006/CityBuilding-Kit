using UnityEngine;
using System.Collections;

public class AutoDestruct : MonoBehaviour
{
    public float DestructTime = 2.0f;

    void Start()
    {
		transform.parent = GameObject.Find("GroupEffects").transform;
        Destroy(gameObject, DestructTime);
    }
}
