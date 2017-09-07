using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Nebula : MonoBehaviour {
    private type nebulaType;                                //type of nebula

    private enum type                                       //collection of nebula types
    {
        corosion,                                           //hull damage
        energy,                                             //shield damage
        plasma                                              //full damage
    }

    private float dmgHull;                                  //damage to hull
    private float dmgShields;                               //damage to shields
    private float delay = 3;                                //tick timer
    private float lastTrigger;                              //last tick time

    private List<GameObject> ships=new List<GameObject>();  //Gameobjects of ships inside nebula

    // Use this for initialization
    void Start()
    {

        switch (this.tag)                                   //read nabula type
        {
            case "corosion":                                //hull damage
                nebulaType = type.corosion;                     //set type
                dmgHull = 5;                                    //set damage hull
                dmgShields = 0;                                 //set damage shields
                break;
            case "energy":                                  //shield damage
                nebulaType = type.energy;                       //set type
                dmgShields = 10;                                //set damage shields
                dmgHull = 0;                                    //set damage hull
                break;
            case "plasma":                                  //full damage
                nebulaType = type.plasma;                       //set type
                dmgShields = 20;                                //set damage shields
                dmgHull = 10;                                   //set damage hull
                break;
            default:                                        //default
                nebulaType = type.energy;                       //set type
                dmgShields = 10;                                //set damage shields
                dmgHull = 0;                                    //set damage hull
                break;
        }
    }

    // Update is called once per frame
    void Update()
    { 
        if (ships.Count > 0 && lastTrigger+delay<Time.realtimeSinceStartup)                 //if next tick
        {
            applyNebulaEffects();                                                           //apply damage to ships inside
        }
    }

    //on trigger enter event
    private void OnTriggerEnter(Collider obj)
    {
        if (obj.tag != "ignore")                                                                    //if object is not scann perimiter
        {
            GameObject ship = obj.transform.root.gameObject;                                        //get root object
                                                                      
            if (!ships.Contains(ship) && ship != null)                                              //if ship list does not contain root object and root is not null
            {
                ships.Add(ship.gameObject);                                                         //add root to list
            }
        }
    }

    //on trigger exit event
    private void OnTriggerExit(Collider obj)
    {
        GameObject ship = obj.transform.root.gameObject;                                            //read root object

        if (ships.Contains(ship) && ship!=null)                                                     //if list contains root object and root is valid
        {
            ships.Remove(ship);                                                                     //remove object
        }
    }

    //apply effect
    private void applyNebulaEffects()
    {
        foreach(GameObject ship in ships)                                                           //cycle trhough ships inside effect area
        {
           ship.GetComponent<ShipEventController>().onHitByWeapon(dmgShields, dmgHull);             //apply damage to ship
        }
        lastTrigger = Time.realtimeSinceStartup;                                                    //after cycle set last tick time
    }

    //recurssive algorithm to find ship parent object
    //private GameObject findWantedParent(GameObject obj)
    //{
    //    GameObject parent=null;
    //    if(obj.tag == "Player" || obj.tag == "AIShips")
    //    {
    //        return obj;
    //    }
    //    else if(obj.transform.parent.tag=="Player" || obj.transform.parent.tag == "AIShips")
    //    {
    //        parent = obj.transform.parent.gameObject;
    //    }
    //    else if(obj.transform.parent!=null)
    //    {
    //        parent=findWantedParent(obj.transform.parent.gameObject);
    //    }
    //    return parent;
    //}
}
