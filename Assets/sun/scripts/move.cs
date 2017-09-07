using UnityEngine;
using System.Collections;

public class move : MonoBehaviour {

    public float rotSpeed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(0f, rotSpeed * Time.deltaTime, 0f);
	}
}
