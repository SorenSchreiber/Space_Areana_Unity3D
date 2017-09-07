using UnityEngine;
using System.Collections;

public class PowerUps : MonoBehaviour {
    public GameControler GC;            //Game controller script
    public enum Type                    //collection of possible types
    {
        health,                         //increase health
        shields,                        //increase shields
        lifes                           //increase lives
    }

    private Type type;                  //type of power up
    private float shieldValue=60;       //heal value shields
    private float hullValue=40;         //heal value hull

	// Use this for initialization
	void Start () {
        //Assign power up type
        if (this.gameObject.tag == "healthPuP")
            type = Type.health;
        else if (this.gameObject.tag == "shieldsPuP")
            type = Type.shields;
        else
            type = Type.lifes;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //on trigger enter event
    void OnTriggerEnter(Collider obj)
    {
        if (obj.tag!="ignore"&&(obj.transform.root.tag == "Player" || obj.transform.root.tag == "AIShips")) //if collider is part of player or AI
        {
            switch(type)    //read type of powerup
            {
                case Type.health:
                    obj.transform.root.GetComponent<ShipEventController>().healHull(hullValue);     //heal ship hull
                    break;
                case Type.shields:
                    obj.transform.root.GetComponent<ShipEventController>().healShields(shieldValue);//heal ship shields
                    break;
                case Type.lifes:
                    if (obj.transform.root.tag == "Player")                                         //if player
                    {
                        obj.transform.root.GetComponent<ShipEventController>().addLife();           //add 1 life
                    }
                    break;
            }
            if(obj.transform.root.tag=="Player")        //if object is player
            {
                GC.addToScore(6);                       //increase player score
            }

            Destroy(this.gameObject);                   //destroy
        }
    }
}
