using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIWeaponControl : MonoBehaviour {
    public GameControler GC;                                //game controller script
    public AIScanner scanner;                               //Scanner script for ai
    private Vector3 leadIndicator;                          //position of lead indicator

    private float targetSpeed;                              //target speed
    private GameObject target;                              //target gameobject

    public GameObject rocket;                               //rocket prefab
    public GameObject cannon;                               //blaster prefab

    public GameObject[] rocketPods;                         //rocket pods
    public GameObject[] cannonPods;                         //cannon pods
    public GameObject[] beamEmitters;                       //beam emitters

    private Rockets currentRocketScript;                    //rocket script
    private BeamWeapons currentBeamScript;                  //beam script
    private CannonShotsScript currentCannonScript;          //cannon script

    private int selectedRocketPod;                          //current rocket pod
    private int selectedCannonPod;                          //current cannon pod
    private int selectedBeamEmitter;                        //current beam emitter

    public float rocket_reload;                             //reload timer for rockets

    public List<GameObject> rocketTarget;                   //rocket target list
    private GameObject lockedTarget;                        //locked rocket target
    private int currentTarget;                              //current target
    private float weaponRange;                              //weapon range

    private bool readyToFire;                               //is ship ready to fire
    private float lastShotTime;                             //time of last fire event
    private float delayBetweenShots;                        //time between shots
    private float maxDelayBetweenShots;                     //max delay before shooting
    private float minDelayBetweenShots;                     //min delay before shooting

    private float rocketWeaponRange;                        //max rocket range
    private float cannonWeaponRange;                        //max cannon range
    private float beamWeaponRange;                          //max beam range
    private int weaponToFire;                               //current selected weapon
    private float timeSinceSwitch;                          //time since last weapon switch
    private bool preSet;                                    //is preset

    // Use this for initialization
    void Start () {
        GC = GameObject.FindGameObjectWithTag("GC").GetComponent<GameControler>();                      //get game controller
        selectedRocketPod = 0;                                                                          //set rocket pod
        selectedCannonPod = 0;                                                                          //set cannon pod
        selectedBeamEmitter = 0;                                                                        //set beam emitter
        preSet = false;                                                                                 //set preset
        rocket_reload = 15;                                                                             //set rocket reload timer to 15 secounds
        readyToFire = true;                                                                             //set ready to fire
        weaponToFire = 0;                                                                               //preselect cannon 
        timeSinceSwitch = Time.realtimeSinceStartup;                                                    //set last switch

        beamWeaponRange = beamEmitters[0].GetComponent<BeamWeapons>().getMaxFireDistance();             //get beam max distance
        rocketWeaponRange = rocketPods[0].GetComponentInChildren<Rockets>().getMaxFireRange();          //get rocket max distance
        cannonWeaponRange = cannon.GetComponent<CannonShotsScript>().getMaxFireRange();                 //get cannon max distance
    }
	
	// Update is called once per frame
	void Update () {
        if (!GC.paused)                                                                                 //if game is not paused
        {
            if (!preSet && GC.readDifficulty() != 0)                                                    //if not set and difficutly is not 0
            {
                preSet = true;                                                                          //preset
                maxDelayBetweenShots = 2f * (float)GC.readDifficulty();                                 //set max delay
                minDelayBetweenShots = 1f * (float)GC.readDifficulty() / 3f;                            //set min delay
            }
            if (readyToFire)                                                                            //if ready to fire
            {
                delayBetweenShots = Random.Range(minDelayBetweenShots, maxDelayBetweenShots);           //set delay between shots
                readyToFire = false;                                                                    //ready to fire false

                target = scanner.getTarget();                                                           //get target

                if (target != null)                                                                     //if has target
                {
                    targetSpeed = target.GetComponent<PlayerControl>().getSpeed();                      //read target speed
                    leadIndicator = getLeadPoint();                                                     //calculate lead

                    if (Random.Range(0, 1 + (3 / GC.readDifficulty())) == 0)                            //random chance to miss
                    {
                        leadIndicator = addVariance(leadIndicator);                                     //add random variance to lead
                    }

                    if (Random.Range(0, 2) == 1 && timeSinceSwitch + 15 < Time.realtimeSinceStartup)    //if switch and last switch is 15 second old
                    {
                        timeSinceSwitch = Time.realtimeSinceStartup;                                    //set last switch
                        weaponToFire = decideOnWeapon();                                                //decide on weapon
                    }

                    switch (weaponToFire)                                                               //read weapon to fire
                    {
                        case 0:                                                                         //if lead is within range and fire angle => shoot cannon
                            if (getDistanceToTargetPoint(leadIndicator, this.transform.position) < cannonWeaponRange && isTargetInWeaponAngle(40, leadIndicator))
                            {
                                fireCannon(leadIndicator, cannonPods[selectedCannonPod]);
                            }
                            break;
                        case 1:                                                                         //if lead is within range and fire angle => shoot beam
                            if (getDistanceToTargetPoint(leadIndicator, this.transform.position) < beamWeaponRange && isTargetInWeaponAngle(90, leadIndicator))
                            {
                                fireBeam(beamEmitters[selectedBeamEmitter], leadIndicator);
                            }
                            break;
                        case 2:                                                                         //if lead is within range and fire angle => shoot rocket
                            if (getDistanceToTargetPoint(leadIndicator, this.transform.position) < rocketWeaponRange && isTargetInWeaponAngle(90, leadIndicator))
                            {
                                fireRocket(rocketPods[selectedRocketPod], target);
                            }
                            break;
                    }
                    lastShotTime = Time.realtimeSinceStartup;                                           //set last shot
                }
            }
            else if (Time.realtimeSinceStartup >= (lastShotTime + delayBetweenShots))                   //set ready to fire on run out of firing delay
            {
                readyToFire = true;
            }
        }
    }

    //calculate lead point
    Vector3 getLeadPoint()
    {
        Vector3 leadPoint=target.transform.position+(target.transform.forward*targetSpeed*50);  //get lead point
        leadPoint = leadPoint + new Vector3(0, 20, 0);                                          //elevate to hit ship
        return leadPoint;
    }

    //get distance to target
    float getDistanceToTargetPoint(Vector3 target, Vector3 position)                           
    {
        float distance = Vector3.Distance(target, position);                //calc distance

        return distance;
    }

    //fire cannon
    void fireCannon(Vector3 target, GameObject pod)
    {
        GameObject cannonShot = Instantiate(cannon) as GameObject;                  //spawn new blaster
        currentCannonScript = cannonShot.GetComponent<CannonShotsScript>();         //get cannon script
        cannonShot.transform.position = pod.transform.position;                     //set position to pod position

        currentCannonScript.onFire(target, this.transform.root.gameObject);         //fire shot

        selectedCannonPod += 1;                                                     //select next pod
        if (selectedCannonPod >= cannonPods.Length)                                 //on overflow reset to firt pod
        {
            selectedCannonPod = 0;
        }
    }

    //fire beam
    void fireBeam(GameObject emitter, Vector3 target)
    {
        currentBeamScript = emitter.GetComponent<BeamWeapons>();                    //get beam script

        currentBeamScript.onFire(target);                                           //fire beam

        selectedBeamEmitter += 1;                                                   //select next beam emitter
        if (selectedBeamEmitter >= beamEmitters.Length)                             //on overflow select first beam emitter
        {
            selectedBeamEmitter = 0;
        }
    }

    //fire rocket
    void fireRocket(GameObject pod, GameObject target)
    {
        if (pod.transform.childCount == 1)                                          //if pod has rocket
        {
            currentRocketScript = pod.GetComponentInChildren<Rockets>();            //get rocket script
            currentRocketScript.fireRocket(target);                                 //fire rocket

           StartCoroutine(spawnRocket(rocketPods[selectedRocketPod]));              //reload pod

            selectedRocketPod += 1;                                                 //select next pod
            if (selectedRocketPod >= rocketPods.Length)                             //on overflow select first pod
            {
                selectedRocketPod = 0;
            }
        }
    }

    //spawn new rocket in pod
    IEnumerator spawnRocket(GameObject rocketpod)
    {
        yield return new WaitForSeconds(rocket_reload);                             //delay spawn for reload time

        GameObject newRocket = Instantiate(rocket) as GameObject;                   //create new rocket
        newRocket.transform.parent = rocketpod.transform;                           //set parent to rocket pod
        newRocket.transform.position = rocketpod.transform.position;                //set position to rocket pod
        newRocket.transform.rotation = rocketpod.transform.rotation;                //set rotation to rocket pod
    }

    //pick weapon to fire
    int decideOnWeapon()
    {
        int weapon;                                                                 //selected weapon
        int pick = Random.Range(0, 3*GC.readDifficulty());                          //random number to pick weapon
        switch(pick)
        {
            case 0:                                                                 //select cannon
                weapon = 0;
                break;
            case 1:                                                                 //select beam
                weapon = 1;
                break;
            case 2:                                                                 //select rocket
                weapon = 2;
                break;
            case 3:                                                                 //select cannon
                weapon = 0;
                break;
            case 4:                                                                 //select beam
                weapon = 1;
                break;
            case 5:                                                                 //select cannon
                weapon = 0;
                break;
            case 6:                                                                 //select beam
                weapon = 1;
                break;
            case 7:                                                                 //select cannon
                weapon = 0;
                break;
            case 8:                                                                 //select cannon
                weapon = 0;
                break;
            default:                                                                //select cannon
                weapon = 0;
                break;
        }

        return weapon;
    }

    //read angle to target
    bool isTargetInWeaponAngle(float targetingAngle, Vector3 target)
    {
        float angle = Vector3.Angle(this.transform.forward, target - this.transform.position);  //calc angle between ship and target

        if (angle <= targetingAngle)                                                            //if with in targetign angle
            return true;
        else
            return false;
    }

    //add variance to vector
    Vector3 addVariance(Vector3 originalLead)
    {
        int variance = 5*GC.readDifficulty();                                           //set variance depending on difficulty
        float maxRandomRange = 3 * GC.readDifficulty();                                 //set max range depending on difficulty
        Vector3 newLeadVector = originalLead + new Vector3(variance*Random.Range(0,maxRandomRange), variance * Random.Range(0, maxRandomRange), variance * Random.Range(0, maxRandomRange)); //add random variance to target lead

        return newLeadVector;
    }
}
