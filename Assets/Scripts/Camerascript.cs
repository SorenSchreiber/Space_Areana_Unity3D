using UnityEngine;
using System.Collections;

public class Camerascript : MonoBehaviour {
    private GameObject Player;      //Player game object
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Player == null) //if curently no connected player
        {
            Player = GameObject.FindGameObjectWithTag("Player"); //find player
        }

        this.transform.position = Player.transform.position;    //set position to player position
        this.transform.rotation = Player.transform.rotation;    //set rotation to player rotation
	}
}
