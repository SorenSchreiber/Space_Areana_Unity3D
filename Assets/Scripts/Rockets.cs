using UnityEngine;
using System.Collections;

public class Rockets : MonoBehaviour {
    public AudioSource sound;                           //audio source

    public float detonator;                             //max flight time
    private float damageHull;                           //damage to hull
    private float damageShields;                        //damage to shields
    public float speed;                                 //flying speed
    public float rotSpeed;                              //turn speed
    public bool fired;                                  //is fired

    private GameControler GC;                           //game controller 
    private bool detached;                              //is deteched form carrier
    private bool homing;                                //is homing on target
    private float range;                                //distance to player
    private float maxRange;                             //max fire range
    private Transform target;                           //transform of locked target
    private Transform player;                           //transform of carrier
    public ParticleSystem engine;                       //particle system for rocket engine    
    public ParticleSystem glow;                         //particle system for engine glow
    private ShipEventController targetEventControler;   //target event controller script

    // Use this for initialization
    void Start () {
        GC = GameObject.FindGameObjectWithTag("GC").GetComponent<GameControler>();  //get game controller
        sound = GetComponent<AudioSource>();                                        //get audio source
        player = this.transform.root.transform;                                     //set player to this root object

        fired=false;                                                                //set firing false
        homing = false;                                                             //set homing false
        detached = false;                                                           //set detached false
        range = 400;                                                                //set distance to player 
        maxRange = 2000;                                                            //set max fire range

        damageHull = 65;                                                            //set damage to hull
        damageShields = 10;                                                         //set damage to shields
	}
	
	// Update is called once per frame
	void Update () {
        if (!GC.paused)                                     //if game is not paused
        {
            if (fired)                                      //if rocket is fired
            {
                if (!detached)                              //if rocket is not detached
                {
                    detach();
                }
                else
                {
                    Destroy(this.gameObject, detonator);    //max fly time exceded =>destroy
                    if (homing)                             //if rocket is homing
                    {
                        homingOnTarget();
                    }
                    else
                    {
                        moveAwayFromPlayer();
                    }
                }
            }
        }
    }

    //on hit with taret
    void OnHit(GameObject obj)
    {
        targetEventControler.onHitByWeapon(damageShields, damageHull);  //apply damage to target
        Destroy(this.gameObject);                                       //destroy
    }

    //on fire event
    public void fireRocket(GameObject targetObj)
    {
        if (targetObj != null)                                                      //if target object is valid
        {
            target = targetObj.transform;                                           //set taret
            targetEventControler = targetObj.GetComponent<ShipEventController>();   //get target event controller
            fired = true;                                                           //set fire true
            sound.Play();                                                           //play sound
        }
    }

    //detachting form carrier
    void detach()
    {
        if (this.transform.parent != null)                                      //if linked to parent
        {
            engine.Play();                                                      //start engine particles
            glow.Play();                                                        //start glow paricles
            this.transform.parent = null;                                       //remove from parent
        }

        transform.position -= this.transform.up * (speed / 4) * Time.deltaTime; //move rocket away from carrier(falling away)

        if (Vector3.Distance(player.position, this.transform.position) > 50)    //if 50 units away
        {
            detached = true;                                                    //set detached true
        }
    }

    //rocket homin gon target
    void homingOnTarget()
    {
        if (target == null)                                                                                                 //if target lost
            Destroy(this.gameObject);                                                                                       //destroy

        Quaternion targetRotation = Quaternion.LookRotation(target.position - this.transform.position);                     //get rotation towards target
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, speed * Time.smoothDeltaTime);  //turn rocket to face target

        transform.position += this.transform.forward * speed * Time.deltaTime;                                              //move rocket forward

        if ((target.position - this.transform.position).magnitude < 1)                                                      //if within 1 unit of target
        {
            OnHit(target.gameObject);   //hit event
        }
    }

    //moving away from carrier
    void moveAwayFromPlayer()
    {
        transform.position += this.transform.forward * speed * Time.deltaTime;  //move rocket forward from carrier

        if (Vector3.Distance(player.position, this.transform.position) > range) //if rocket is far enough away
            homing = true;                                                      //set homing true
    }

    //read max fire range
    public float getMaxFireRange()
    {
        return maxRange;
    }
}
