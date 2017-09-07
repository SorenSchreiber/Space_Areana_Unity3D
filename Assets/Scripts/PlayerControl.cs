using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

public class PlayerControl : MonoBehaviour {
    private GameControler GC;               //game controller script
    public MeshRenderer scanPerimiter;      //scan perimiter mesh

    public Transform Ship;                  //transform of player ship
    public ParticleSystem leftEngine;       //particles for left engine
    public ParticleSystem rightEngine;      //particles for right engine
    private float speed;                    //float speed
    public float maxSpeed;                  //float max speed
    public float turnSpeed;                 //float max turn speed

    private float setStartSize;             //particle start size
    private float setLifeTime;              //particle life time

    // Use this for initialization
	void Start () {
        GC = GameObject.FindGameObjectWithTag("GC").GetComponent<GameControler>();  //find game controler
        scanPerimiter.enabled = false;                                              //disable scanner mesh

        setStartSize =12;                                                           //set start size for engines
        setLifeTime=1;                                                              //set lifetime for engines
}
	
	// Update is called once per frame
	void Update () {
        if (!GC.paused) //if game is not paused
        {
            Ship.transform.position = this.transform.position;      //set ship transform to this transform

            float z = -CrossPlatformInputManager.GetAxis("PlayerHorizontal") * 40f; //calculate tilt angle for ship

            Vector3 euler = Ship.localEulerAngles;                                  //read local euler for ship

            if (Vector3.Dot(transform.up, Vector3.down)>0)                          //if ship is upside down invert z
                z = -z;

            euler.z = Mathf.LerpAngle(euler.z, z, 2.0f * Time.deltaTime);           //lerp tilt for ship
            Ship.localEulerAngles = euler;                                          //set euler angles for ship

            speed = maxSpeed * CrossPlatformInputManager.GetAxis("Throttle");       //read speed
            transform.Rotate(0, CrossPlatformInputManager.GetAxis("PlayerHorizontal") * (turnSpeed*(CrossPlatformInputManager.GetAxis("Throttle") + 0.5f)), 0, Space.World);//read rotation up down
            transform.Rotate(CrossPlatformInputManager.GetAxis("PlayerVertical") * (turnSpeed * (CrossPlatformInputManager.GetAxis("Throttle") + 0.5f)), 0, 0, Space.Self); //read rotation left right

            
            transform.Translate(0, 0, speed);   //move player forward
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -10000, 10000), Mathf.Clamp(transform.position.y, -10000, 10000), Mathf.Clamp(transform.position.z, -10000, 10000)); //lock player inside cube arena

            leftEngine.startLifetime = Mathf.Lerp(0.0f, setLifeTime, CrossPlatformInputManager.GetAxis("Throttle"));            //lerp engine lifetime relative to throttle input
            leftEngine.startSize = Mathf.Lerp(setStartSize * .3f, setStartSize, CrossPlatformInputManager.GetAxis("Throttle")); //lerp engine lifetime relative to throttle input

            rightEngine.startLifetime = Mathf.Lerp(0.0f, setLifeTime, CrossPlatformInputManager.GetAxis("Throttle"));           //lerp engine lifetime relative to throttle input
            rightEngine.startSize = Mathf.Lerp(setStartSize * .3f, setStartSize, CrossPlatformInputManager.GetAxis("Throttle"));//lerp engine lifetime relative to throttle input
        }
    }

    //read player speed
    public float getSpeed()
    {
        return speed;
    }
}
