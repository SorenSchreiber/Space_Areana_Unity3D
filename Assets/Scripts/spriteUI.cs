using UnityEngine;
using System.Collections;

public class spriteUI : MonoBehaviour {

    private Canvas canvas;
	// Use this for initialization
	void Start () {
        canvas=GetComponent<Canvas>();
        canvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.LookAt(Camera.main.transform.position);
	}
}
