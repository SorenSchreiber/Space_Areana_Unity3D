using UnityEngine;
using System.Collections;

public class BeamWeapons : MonoBehaviour {

    public AudioSource sound;   //Audio Source variable

    public float speed;         //speed of the beam       
    public Transform gun;       //transform of gun object
    private Vector3 target;     //vector of target
    public float dist;          //float max distance
    public bool firing;         //is firing
    public bool removing;       //is removing
    private GameControler GC;   //Gamecontroller script

    private float hitlength;    //distance to hitpoint
    private float counter;      //step counter
    private float x;            //current point on beam
    private LineRenderer LR;    //Line renderer element
    private Vector3 pointOnLine;//point to draw on line
    private RaycastHit hit;     //raycast hit output

    private bool readytofire;   //is weapon ready to be fired

    private float damageHull;   //damage to the ship hull
    private float damageShields;// damage to ship shields

    // Use this for initialization
    void Start () {
        GC = GameObject.FindGameObjectWithTag("GC").GetComponent<GameControler>();  //find game controller
        sound=GetComponent<AudioSource>();  //get audio source
        LR = GetComponent<LineRenderer>();  //get line renderer
        readytofire = true;                 //set weapon ready to fire

        damageHull = 10;                    //set hull damage
        damageShields = 40;                 //set shield damage
	}
	
	// Update is called once per frame
	void Update () {
        if (!GC.paused)                     //if game is not paused
        {
            if (firing)                     //if firing
            {
                fire(target);
            }
            else if (removing)              //if removing
            {
                removeBeam();
            }
        }
    }

    //on fire event
    public void onFire(Vector3 targetSend)
    {
        if (readytofire)                        //if ready to fire
        {
            target = targetSend;                //set target
            readytofire = false;                //set weapon not ready
            LR.SetPosition(0, gun.position);    //set line start point to weapon position
            firing = true;                      //set firing true
            sound.Play();                       //play sound
        }
    }

    //on raycast hit
   void OnHit(GameObject obj)
    {
        obj.transform.root.GetComponent<ShipEventController>().onHitByWeapon(damageShields, damageHull);    //deal damage to hit object
        firing = false;                                                                                     //set firing false
        hitlength = x;                                                                                      //set distance to hitpoint to current point on line
        x = 0;                                                                                              //reset x
        counter = 0;                                                                                        //reset counter
        removing = true;                                                                                    //set removing state true                                                                     
    }

    //while firing
    void fire(Vector3 target)
    {
        transform.LookAt(target);                                                           //rotate to look at target
        if (x<dist)                                                                         //if not at max distance
        {
            counter += .1f / speed;                                                         //increase counter by 0.1f / beam speed

            x = Mathf.Lerp(0, dist, counter);                                               //lerp towards distance

            pointOnLine = x * Vector3.Normalize(target - gun.position) + gun.position;      //calculate point on line

            LR.SetPosition(1, pointOnLine);                                                 //set endpoin to calculated point
            if (Physics.Raycast(gun.position, gun.forward, out hit, x))                     //check for raycast hit
            {
                if (hit.transform.root != this.gameObject.transform.root)                   //if hit is not with this root object
                {
                    OnHit(hit.transform.root.gameObject);                                   //call hit function
                }
            }
        }
        else
        {
            firing = false;                                                                 //set firing false
            hitlength = dist;                                                               //set hitlength to max distance
            counter = 0;                                                                    //reset counter
            x = 0;                                                                          //reset x
            removing = true;                                                                //start removing
        }
    }

    //while removing
    void removeBeam()
    {
        //same procedure as firing
        //in this case the start point of the line is being moved
        if (x < hitlength)
        {
            counter += .1f / (speed);

            x = Mathf.Lerp(0, hitlength, counter);

            pointOnLine = x * Vector3.Normalize(target - gun.position) + gun.position;

            LR.SetPosition(0, pointOnLine);
        }
        else    //reset line renderer for new fire and variables
        {
            LR.SetPosition(0, gun.position);
            LR.SetPosition(1, gun.position);
            counter = 0;
            x = 0;
            removing = false;
            readytofire = true;
        }
    }

    //read max range
    public float getMaxFireDistance()
    {
        return dist;
    }
}
