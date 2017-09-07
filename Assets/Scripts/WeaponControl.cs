using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;

public class WeaponControl : MonoBehaviour {

    public GameObject rocket;                           //rocket prefab
    public GameObject cannon;                           //blaster prefab
    public GameObject crossHair;                        //crosshiar Gameobject
    private GameControler GC;                           //Game controller

    public GameObject[] rocketPods;                     //rocket pod array
    public GameObject[] cannonPods;                     //blaster gun array
    public GameObject[] beamEmitters;                   //beam weapon array

    public GameObject GameUI;                           //GameUI
    public Image button;                                //image for weapon swith
    public Image fireButton;                            //image for fire weapon
    public Image cycleTarget;                           //image for cycle target

    public Sprite rocketMat;                            //sprite for rocket weapon
    public Sprite beamMat;                              //sprite for beam weapon 
    public Sprite cannonMat;                            //sprite for cannon weapon

    private Rockets currentRocketScript;                //rocket script
    private BeamWeapons currentBeamScript;              //beam script
    private CannonShotsScript currentCannonScript;      //cannon script

    private int selectedRocketPod;                      //current rocket pod
    private int selectedCannonPod;                      //current cannon pod
    private int selectedBeamEmitter;                    //current beam emitter

    public float rocket_reload;                         //delay for rocket reload
                            
    private float timeOfFire;                           //last weapon fire
    public List<GameObject> rocketTarget;               //list of AI ships in rocket range
    private GameObject lockedTarget;                    //locked target for rocket
    private int currentTarget;                          //target counter
    public int weapon_select;                           //selected weapon
    private float weaponRange;                          //Weapon range


	// Use this for initialization
	void Start () {
        GC = GameObject.FindGameObjectWithTag("GC").GetComponent<GameControler>();          //get game controller
        button = GameObject.FindGameObjectWithTag("WSB").GetComponent<Image>();             //get weapon select button
        fireButton = GameObject.FindGameObjectWithTag("FB").GetComponent<Image>();          //get fire button
        cycleTarget = GameObject.FindGameObjectWithTag("TC").GetComponent<Image>();         //get target cycling button

        selectedRocketPod = 0;                                                              //set current rocket pod
        selectedCannonPod = 0;                                                              //set current cannon pod
        selectedBeamEmitter = 0;                                                            //set current beam emitter

        currentTarget=0;                                                                    //set current target
        weapon_select = 0;                                                                  //select cannon weapon by default
        rocket_reload = 15;                                                                 //set reload for rockets to 15 seconds
        button.sprite = cannonMat;                                                          //show cannon icon
        cycleTarget.enabled = false;                                                        //diable cycling targets
    }
	
	// Update is called once per frame
	void Update () {
        if (!GC.paused)                                                                     //if game is not paused
        {
            if (CrossPlatformInputManager.GetButtonDown("SelectWeapon"))                    //on select weapon press
            {
                weapon_select += 1;                                                         //increase current weapon
                if (weapon_select >= 3)                                                     //cycle back on overflow
                {
                    weapon_select = 0;
                }

                switch (weapon_select)                                                      //decide on weapon
                {
                    case 0:                                                                 //if blaster
                        setWeaponRange();                                                           //set weapon range
                        hideButton();                                                               //hide cycle button
                        button.sprite = cannonMat;                                                  //set icon to blaster
                        break;
                    case 1:                                                                 //if beam
                        setWeaponRange();                                                           //set weapon range
                        hideButton();                                                               //hide cycle button
                        button.sprite = beamMat;                                                    //set icon to beam
                        break;
                    case 2:                                                                 //if rocket
                        setWeaponRange();                                                           //set weapon range
                        showButton();                                                               //show cycle button
                        button.sprite = rocketMat;                                                  //set icon to rocket
                        break;
                }
            }

            if (CrossPlatformInputManager.GetButtonDown("Fire"))                            //on fire button
            {
                switch (weapon_select)                                                      //select weapon
                {
                    case 0:                                                                 //fire cannon
                        timeOfFire = Time.realtimeSinceStartup;                                 //set time of fire
                        fireCannon(cannonPods[selectedCannonPod]);                              //fire shot
                        break;
                    case 1:                                                                 //fire beam
                        fireBeam(beamEmitters[selectedBeamEmitter]);                            //fire beam
                        break;
                    case 2:                                                                 //fire rocket
                        if (rocketTarget.Count != 0)                                            //if rocket pod contains rocket
                        {
                            timeOfFire = Time.realtimeSinceStartup;                             //set time of fire
                            fireRocket(rocketPods[selectedRocketPod], lockedTarget);            //shoot rocket
                        }
                        break;
                }
            }

            if (CrossPlatformInputManager.GetButtonDown("cycleTarget"))                         //on cycle target
            {
                cycleRocketTarget();
            }

            if (rocketTarget.Count == 0 || weapon_select != 2)                                  //if no target in range or wepon is not rocket hid cycle target button
            {
                hideButton();
            }
            else
            {
                showButton();
            }

            if (!fireButton.enabled && weapon_select != 2)                                  //if fire button is hidden and ewapon is not rocket show fire button
            {
                fireButton.enabled = true;
            }
            else if (weapon_select == 2)                                                    //if current weapon is rocket
            {
                if (fireButton.enabled)                                                     //if fire button enabled
                {
                    if (rocketTarget.Count == 0 || lockedTarget == null || !isTargetInWeaponAngle(90, lockedTarget.transform.position)) //if no target in range, no valid target selected or target not in fire angle turn of fire button
                    {
                        fireButton.enabled = false;
                    }
                }
                else if (!fireButton.enabled)                                               //if fire button disabled
                {
                    if (lockedTarget != null && isTargetInWeaponAngle(90, lockedTarget.transform.position))                         //if valid target locked and target is in fire angle show fire button
                    {
                        fireButton.enabled = true;
                    }
                }

            }

            if (rocketTarget.Count == 0 && lockedTarget != null)                                                                    //if target count is 0 and current target is not invalid set target to null
            {
                lockedTarget = null;
            }

            if (rocketTarget.Count != 0 && lockedTarget == null)                                                                    //if target is null and rocket count is not empty remove invalid target rom list
            {
                rocketTarget.Remove(lockedTarget);
            }
        }
    }

    //spawn new rocket in pod
    IEnumerator spawnRocket(GameObject rocketpod)                                                   
    {
        yield return new WaitForSeconds(rocket_reload);                         //wait for reload timer
        
        GameObject newRocket = Instantiate(rocket) as GameObject;               //create new rocket instance
        newRocket.transform.parent = rocketpod.transform;                       //set parent to pod
        newRocket.transform.position = rocketpod.transform.position;            //set position to pod position
        newRocket.transform.rotation = rocketpod.transform.rotation;            //set rotaion to pod rotation
    }

    //firing beam weapon
    void fireBeam(GameObject emitter)                                           
    {
        currentBeamScript = emitter.GetComponent<BeamWeapons>();                //get beam script from current emitter

        currentBeamScript.onFire(crossHair.transform.position);                 //fire beam towards crosshair

        selectedBeamEmitter += 1;                                               //select next emitter
        if (selectedBeamEmitter >= beamEmitters.Length)                         //on overflow reset to first emitter
        {
            selectedBeamEmitter = 0;
        }
    }

    //firing cannon
    void fireCannon(GameObject pod)
    {
        GameObject cannonShot = Instantiate(cannon) as GameObject;                                  //create cannon shot
        currentCannonScript = cannonShot.GetComponent<CannonShotsScript>();                         //get cannon script
        cannonShot.transform.position = pod.transform.position;                                     //set position to pod position

        currentCannonScript.onFire(crossHair.transform.position, this.transform.root.gameObject);   //fire shot

        selectedCannonPod += 1;                                                                     //select next pod
        if (selectedCannonPod >= cannonPods.Length)                                                 //on overflow reset to first pod
        {
            selectedCannonPod = 0;
        }
    }

    //firing rocket
    void fireRocket(GameObject pod, GameObject target)
    {
        if (pod.transform.childCount == 1)                                                          //if selected pod has rocket
        {
            currentRocketScript = pod.GetComponentInChildren<Rockets>();                            //get rocket script
            currentRocketScript.fireRocket(target);                                                 //fire rocket at target

            StartCoroutine(spawnRocket(rocketPods[selectedRocketPod]));                             //spawn new rocket
 
            selectedRocketPod += 1;                                                                 //select next rocket pod
            if (selectedRocketPod >= rocketPods.Length)                                             //on overflow select first pod
            {
                selectedRocketPod = 0;
            }
        }
    }

    //cycle trhough targets for rocket
    void cycleRocketTarget()
    {
        if(lockedTarget!=null)                                                                      //if current target is not null
            lockedTarget.GetComponent<ShipEventController>().rocketLock();                              //turn of target lock

        currentTarget += 1;                                                                         //select next target
        if(currentTarget>=rocketTarget.Count)                                                       //on overflow select first target
        {
            currentTarget = 0;
        }

        lockedTarget = rocketTarget[currentTarget];                                                 //set locked target
        lockedTarget.GetComponent<ShipEventController>().rocketLock();                              //turn on target lock
    }

    //show cycle target button
    void showButton()
    {
        cycleTarget.enabled = true;
    }

    //hide cycle target button
    void hideButton()
    {
        cycleTarget.enabled = false;
    }

    //set weapon range
    void setWeaponRange()
    {
        if(weapon_select==2)                                                //if rocket selected
        {
            crossHair.GetComponent<SpriteRenderer>().enabled = false;       //turn off croshair
        }
        else if(!crossHair.GetComponent<SpriteRenderer>().enabled)          //if crosshair disabled
        {
            crossHair.GetComponent<SpriteRenderer>().enabled = true;        //turn on crosshair
        }
    }

    ////reload weapon
    //bool reload(float duration)
    //{
    //    Debug.Log(Time.realtimeSinceStartup + ";;" + (timeOfFire + duration));
    //    if (Time.realtimeSinceStartup >= (timeOfFire + duration))
    //    {
    //        return false;
    //    }
    //    else
    //    {
    //        return true;
    //    }
    //}


        //check if target is within weapons angle
    bool isTargetInWeaponAngle(float targetingAngle, Vector3 target)
    {
        float angle = Vector3.Angle(this.transform.forward, target - this.transform.position);      //get angle between ship and target

        if (angle <= targetingAngle)                                                                //if inside area return true
            return true;
        else
            return false;
    }
}
