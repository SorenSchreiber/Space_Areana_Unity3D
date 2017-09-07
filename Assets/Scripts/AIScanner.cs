using UnityEngine;
using System.Collections;

public class AIScanner : MonoBehaviour {
    private bool targetfound;       //bool is true when player inside AI scan perimiter
    private GameObject target;      //PLayer game object

	// Use this for initialization
	void Start () {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Raycast"), LayerMask.NameToLayer("Ignore Raycast"), true);       //set collision detection to ignore hits between Scan areas
        targetfound = false;                                                                                                        //set targetfound to false
	}
	
	// Update is called once per frame
	void Update () {
       
	}

    //triggered when object enteres trigger
    void OnTriggerEnter(Collider obj)
    {
        if (obj.transform.root.tag == "Player")         //if collider belongs to player
        {
            target = obj.transform.root.gameObject;     //set target to collider.root gameobject
            targetfound = true;                         //set targetfound true
        }
    }

    //on trigger stay event
    void OnTriggerStay(Collider obj)
    {
        if (obj.transform.root.tag == "Player"&& !targetfound)   //if collider belongs to player and no target found yet(player spawns inside collider)
        {
            target = obj.transform.root.gameObject;             //set target to collider.root gameobject
            targetfound = true;                                 //set targetfound true
        }
    }

    //triggered when object leave trigger
    void OnTriggerExit(Collider obj)
    {
        if (obj.transform.root.tag == "Player")         //if collider belongs to player
        {
            target = null;                              //set target NULL
            targetfound = false;                        //set targetfound false
        }
    }

    //read target found value
    public bool getTargetFound()
    {
        return targetfound;
    }

    //read target gameobject
    public GameObject getTarget()
    {
        return target;
    }
}
