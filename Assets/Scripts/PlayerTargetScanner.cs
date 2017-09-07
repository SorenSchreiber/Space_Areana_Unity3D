using UnityEngine;
using System.Collections;

public class PlayerTargetScanner : MonoBehaviour {
    public WeaponControl wpc;       //weapon control script

	// Use this for initialization
	void Start () {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Raycast"), LayerMask.NameToLayer("Ignore Raycast"), true);   //set collision to ignore scanner perimiter
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    //on trigger enter event
    void OnTriggerEnter(Collider obj)
    {
        if(obj.transform.root.tag=="AIShips" && !wpc.rocketTarget.Contains(obj.transform.root.gameObject))  //if collider is part of AI and not in weapon control target list
            wpc.rocketTarget.Add(obj.transform.root.gameObject);    //add target to list
    }

    //on trigget stay event(AI psawns inside scanner range)
    void OnTriggerStay(Collider obj)
    {
        if (obj.transform.root.tag == "AIShips" && !wpc.rocketTarget.Contains(obj.transform.root.gameObject))  //if collider is part of AI and not in weapon control target list
            wpc.rocketTarget.Add(obj.transform.root.gameObject);    //add target to list
    }

    //on trigger exit event
    void OnTriggerExit(Collider obj)
    {
        StartCoroutine(exitTrigger(obj));//start coroutine
    }

    //using IEnumerator to enable delayed removal to smooth UI control
    IEnumerator exitTrigger(Collider obj)
    {
        if (obj.transform.root.tag == "AIShips" && wpc.rocketTarget.Contains(obj.transform.root.gameObject))    //if object is part of AI and is in target list
        {
            yield return new WaitForSeconds(2);                                                     //dealy code for 2 seconds
            obj.transform.root.GetComponent<ShipEventController>().rocketlock.enabled = false;      //remove possible targetlock on object
            wpc.rocketTarget.Remove(obj.transform.root.gameObject);                                 //remove object from target list
        }
    }
}
