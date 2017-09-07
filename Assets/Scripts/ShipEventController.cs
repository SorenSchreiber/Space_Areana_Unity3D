using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShipEventController : MonoBehaviour {

    public GameControler GC;                                                                    //Game controller script
    public GameObject GameUI;                                                                   //UI during play
    public GameObject healthPowerUp;                                                            //health regen prefab
    public GameObject shieldPowerUp;                                                            //shield regen prefab
    public GameObject livesPowerUp;                                                             //lives add prefab

    public float shieldStrength;                                                                //ship shield strength
    private float currentShieldStrength;                                                        //ships current shield strength
    public float hullStrength;                                                                  //ship hull strength
    private float currentHullStrength;                                                          //ship current hull strength
    public MeshRenderer Shields;                                                                //mesh for shield effect
    public GameObject explosion;                                                                //prefab for explosion

    private float hitTime;                                                                      //time of last damage
    private bool shields;                                                                       //are shields active
    private float regenDelay;                                                                   //delay before shield regen

    public Slider healthBar;                                                                    //slider element for health display
    public Slider shieldsBar;                                                                   //slider element for shields display

    public Image rocketlock;                                                                    //image elment for rocket lock
    // Use this for initialization
    void Start () {
        GC = GameObject.FindGameObjectWithTag("GC").GetComponent<GameControler>();              //find Game controller
        GameUI = GameObject.FindGameObjectWithTag("GameUI");                                    //find Game Ui

        if (this.tag == "AIShips")                                                              //if this is AI ship
        {
            currentHullStrength = hullStrength;                                                 //set current health
            currentShieldStrength = shieldStrength;                                             //set current shields
            foreach (Slider slider in this.GetComponentsInChildren<Slider>())                   //read slider componets
            {
                if (slider.name == "EnemyHealth")                                               //set slider for health
                    healthBar = slider;
                else if (slider.name == "EnemyShields")                                         //set slider for shields
                    shieldsBar = slider;
            }
        }
        if (this.tag == "Player")                                                               //if this is player
        {
            hullStrength = hullStrength * GC.readDifficulty();                                  //set health in accordance to difficulty
            shieldStrength = shieldStrength * GC.readDifficulty();                              //set shields in accordance to difficulty
            currentHullStrength = hullStrength;                                                 //set current health
            currentShieldStrength = shieldStrength;                                             //set current shields
            foreach (Slider slider in GameUI.GetComponentsInChildren<Slider>())                 //read slider elements
            {
                if (slider.name == "PlayerHealth")                                              //set health bar
                    healthBar = slider;
                else if (slider.name == "PlayerShields")                                        //set shields bar
                    shieldsBar = slider;
            }
        }

        healthBar.value = 1;                                                                    //set bar to max
        shieldsBar.value = 1;                                                                   //set bar to max
        regenDelay = 20;                                                                        //set shield regen delay
	}
	
	// Update is called once per frame
	void Update () {
        if (!GC.paused)                                                                                                         //if game is not paused
        {
            if (currentShieldStrength > 0)                                                                                      //if ship has shield energy left
                triggerShield(2);                                                                                               //trigger shields for 2 seconds
            else
                Shields.enabled = false;                                                                                        //disable shields

            
            if ((hitTime + regenDelay) < Time.realtimeSinceStartup && currentShieldStrength < shieldStrength)                   //if regen timer has expired
            {
                regenShields();                                                                                                 //regen shields
            }
        }
    }

    //is hit by weapon
    public void onHitByWeapon(float dmgShields, float dmgHull)
    {
        hitTime = Time.realtimeSinceStartup;                                                                                    //set hit time
        if (currentShieldStrength > 0 && dmgShields>0)                                                                          //if there is damage to shields and shields are not depleted
        {
            shields = true;                                                                                                     //set shields active true
            Shields.enabled = true;                                                                                             //enable shields mesh
        }

        dmgCalculation(dmgShields, dmgHull);                                                                                    //calculate damage

        adjustBars();                                                                                                           //adjust bar indicators

        if (currentHullStrength<=0)                                                                                             //if hull falls below 0
        {
            if (transform.root.tag == "Player")                                                                                 //if this is player
            {
                GC.deathPlayer(this.transform.root.position, this.transform.root.rotation);                                     //call death plaer from game controller
            }
            if(transform.root.tag=="AIShips")                                                                                   //if this is AI
            {
                GC.killedEnemy();                                                                                               //call enemy killed
                GC.addToScore(12);                                                                                              //add to player score
            }
            int spawn = Random.Range(0, 6);                                                                                     //random spawn seed for powerups
            int select = Random.Range(0, 7);                                                                                    //random select seed for powerups
            selfDestroy(spawn, select);                                                                                         //destroy object
        }
    }

    //shield triggering
    void triggerShield(float duration)
    {
        if(shields)                                                                                                             //if shields are active
        {
            if(Time.realtimeSinceStartup>=(hitTime+duration))                                                                   //if active time has expired
            {
                Shields.enabled = false;                                                                                        //turn off shield mesh
                shields = false;                                                                                                //set shields to off
            }
        }
           
    }

    //damage calculation
    void dmgCalculation(float dmgShields, float dmgHull)
    {
        if (currentShieldStrength > 0)                                                                                          //if shields are not depleated
        {
            if(dmgShields>=currentShieldStrength)                                                                               //if damage is greater than shield strength
            {
                float dmgleft = (dmgShields - currentShieldStrength) / dmgShields;                                              //calculate dmaage left
                currentShieldStrength = 0;                                                                                      //set shields strength 0
                shields = false;                                                                                                //turn off shields

                currentHullStrength -= (dmgHull * dmgleft);                                                                     //apply damage left to hull
            }
            else
            {
                currentShieldStrength -= dmgShields;                                                                            //adjust shield strength
            }
            
        }
        else if (currentShieldStrength <= 0)                                                                                    //if no shileds are left
        {
            currentHullStrength -= dmgHull;                                                                                     //apply hull damage
        }
    }

    //rocket target lock
    public void rocketLock()
    {
        rocketlock.enabled = !rocketlock.enabled;                                                                               //enable rocket lock spirte on target
    }

    //read ship information
    public List<float> getShipInformation()
    {
        List<float> information = new List<float>();                                                                            //create list of information
        if (this.gameObject.transform.root.tag == "Player")                                                                     //if player set first value 0
            information.Add(0);
        else                                                                                                                    //if AI set first value 1
            information.Add(1);

        information.Add(currentHullStrength/hullStrength);                                                                      //add current health bar value
        information.Add(currentShieldStrength/shieldStrength);                                                                  //add current shield bar value

        return information; 
    }

    //set bar values
    private void adjustBars()
    {
        healthBar.value = currentHullStrength / hullStrength;                                                                   //set new health value for bar
        shieldsBar.value = currentShieldStrength / shieldStrength;                                                              //set new shields avlue for bar
    }

    //regenerate shields
    private void regenShields()
    {
        currentShieldStrength += 25*Time.deltaTime;                                                                             //increase shields
        shieldsBar.value = currentShieldStrength / shieldStrength;                                                              //adjust shields bar to match
    }

    //heal the hull on power up pickup
    public void healHull(float value)
    {
        currentHullStrength += value;                                                                                           //add health increase to current health
        if (currentHullStrength > hullStrength)                                                                                 //check if max hull is reached
            currentHullStrength = hullStrength;                                                                                 //set current to max

        healthBar.value = currentHullStrength / hullStrength;                                                                   //adjust bar
    }

    //heal the shields on power up pickup
    public void healShields(float value)
    {
        currentShieldStrength += value;                                                                                         //add shield increase to current shield
        if (currentShieldStrength > shieldStrength)                                                                             //check if max shields are reached
            currentShieldStrength = shieldStrength;                                                                             //set current to max

        shieldsBar.value = currentShieldStrength / shieldStrength;                                                              //adjust bar
    }

    //add a life on power up pickup
    public void addLife()
    {
        GC.addLifes();                                                                                                          //call ad life 
    }

    //destroy gameobject
    private void selfDestroy(int spawn, int select)
    {
        if(spawn==3)                                                                                                            //if spawn
        {
            switch(select)                                                                                                      //select powerups
            {
                case 0:                                                                                                         //spawn health
                    Instantiate(healthPowerUp, spawnpoint(), new Quaternion(0,0,0,0));
                    break;
                case 1:                                                                                                         //spawn schield
                    Instantiate(shieldPowerUp, spawnpoint(), new Quaternion(0, 0, 0, 0));
                    break;
                case 2:                                                                                                         //spawn life
                    Instantiate(livesPowerUp, spawnpoint(), new Quaternion(0, 0, 0, 0));
                    break;
                case 3:                                                                                                         //spawn shield
                    Instantiate(shieldPowerUp, spawnpoint(), new Quaternion(0, 0, 0, 0));
                    break;
                case 4:                                                                                                         //spawn health
                    Instantiate(healthPowerUp, spawnpoint(), new Quaternion(0, 0, 0, 0));
                    break;
                case 5:                                                                                                         //spawn health+shield
                    Instantiate(shieldPowerUp, spawnpoint(), new Quaternion(0, 0, 0, 0));
                    Instantiate(healthPowerUp, spawnpoint(), new Quaternion(0, 0, 0, 0));
                    break;
                case 6:                                                                                                         //spawn health+shield
                    Instantiate(healthPowerUp, spawnpoint(), new Quaternion(0, 0, 0, 0));
                    Instantiate(shieldPowerUp, spawnpoint(), new Quaternion(0, 0, 0, 0));
                    break;
            }
        }
        Instantiate(explosion, this.transform.position, this.transform.rotation);                                               //spawn explosion prefab
        Destroy(transform.root.gameObject);                                                                                     //destroy
    }

    //get spawn point
    private Vector3 spawnpoint()                                                                                                
    {
        int variant = 20;                                                                                                       //variance
        float x = this.transform.position.x;                                                                                    //parent object x position
        float y = this.transform.position.y;                                                                                    //parent object y position
        float z = this.transform.position.z;                                                                                    //parent object z position

        Vector3 spawnpoint = new Vector3(Random.Range(x-variant, x+variant), Random.Range(y - variant, y + variant), Random.Range(z - variant, z + variant)); //set spawn point at parent location +- variance

        return spawnpoint;
    }

    //get ship information for save purposes
    public float[] getShipInformationToSave()
    {
        float[] info = { currentHullStrength, currentShieldStrength };                          //set float array to health and shield value

        return info;
    }

    //set ship information on load
    public void setShipInformation(string hull, string shields)
    {
        currentHullStrength = float.Parse(hull);                                                //set ship hull strength
        currentShieldStrength = float.Parse(shields);                                           //set ship shield strength
    }

    //read target lock
    public bool readTargetLock()
    {
        return rocketlock.enabled;                                                              //read target lock
    }
}
