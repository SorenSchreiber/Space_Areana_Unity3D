using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIShips : MonoBehaviour {

    public GameControler GC;                        //game controler script
    private bool FSMrunning;                        //is State machine running

    public enum State                               //collection of states
    {
        PATROL,                                     //patrol
        ENGAGE                                      //engage
    }

    public State state;                             //state of AI
    public bool alive;                              //is alive
    public MeshRenderer scanPerimiter;              //mesh for scan perimiter
    public GameObject[] patrolPoints;               //all patrol points on map
    private int waypointCounter;                    //current waypoint
    public AIScanner scan;                          //scanner script

    private GameObject target;                      //taret gaemobject
    private Vector3 destination;                    //current destination
    private Transform obstacle;                     //obstacle transform
    private Vector3 currentWaypoint;                //current waypoint position
    private float distError;                        //distance to target before changing
    private bool buffer;                            //position buffered
    private bool ovDestination;                     //overwritten destination
    public List<Vector3> escapeDir;                 //list of escape directions
    private PlayerControl PC;                       //player control script

    public Transform Ship;                          //ship transform
    public ParticleSystem leftEngine;               //left engine particle system
    public ParticleSystem rightEngine;              //right engine particle system
    private float setStartSize;                     //particle start size
    private float setLifeTime;                      //particle start lifetime
    public float maxSpeed;                          //max speed
    public float currentSpeed;                      //current speed
    public float turnSpeed;                         //turn speed
    public float wingspan;                          //wingspan of ship

    // Use this for initialization
    void Start()
    {
        alive = true;                                                                       //object is alive
        FSMrunning = false;                                                                 //state machine is not running
        GC = GameObject.FindGameObjectWithTag("GC").GetComponent<GameControler>();          //get gamecontroller
        PC = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();      //get player controler

        maxSpeed = maxSpeed / (GC.readDifficulty()/2);                                      //set max speed
        scanPerimiter.enabled = false;                                                      //disable scan mesh
        distError =Random.Range(0, 200);                                                    //set random dist error
        waypointCounter =Random.Range(0, patrolPoints.Length);                              //set current waypoint to random
        currentSpeed = 0;                                                                   //set current speed 0
        buffer = false;                                                                     //set buffer
        ovDestination = false;                                                              //set override
        setStartSize = 12f;                                                                 //set particle start size
        setLifeTime = 1f;                                                                   //set particle life time

        state = State.PATROL;                                                               //set state to patrol
       
        StartCoroutine("finiteStateMachine");                                               //start state machine
    }

    //Finite state machine
    IEnumerator finiteStateMachine()
    {
        while(alive)                        //while object alive
        {      
            switch (state)                  //get state
            {
                case State.PATROL:          //patrol system
                    patrol();
                    break;
                case State.ENGAGE:          //engage player
                    engage();
                    break;
            }
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (patrolPoints.Length > 0 && !FSMrunning)     //if has patrol points and FSM is off
        {
            FSMrunning = true;                                                      //set fsm running 
            destination = patrolPoints[waypointCounter].transform.position;         //set destination
            StartCoroutine("finiteStateMachine");                                   //start fsm
        }
    }

    //trun towards destination
    void turn()
    {
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(destination - this.transform.position), turnSpeed * Time.deltaTime);
    }

    //move towrds destination
    void throttle(float speed)
    {
        currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime);                                         //set current speed

        this.transform.position += this.transform.forward * currentSpeed;                                           //move AI forward

        leftEngine.startLifetime = Mathf.Lerp(0.0f, setLifeTime, currentSpeed/maxSpeed);                            //lerp left engine lifetime
        leftEngine.startSize = Mathf.Lerp(setStartSize * .3f, setStartSize, (currentSpeed / maxSpeed));             //lerp left engine start size

        rightEngine.startLifetime = Mathf.Lerp(0.0f, setLifeTime, (currentSpeed / maxSpeed));                       //lerp right engine lifetime
        rightEngine.startSize = Mathf.Lerp(setStartSize * .3f, setStartSize, (currentSpeed / maxSpeed));            //lerp right engine start size
    }

    //scan for player
    void scanForEnemy()
    {
        if(scan.getTargetFound())               //if target found
        {
            state = State.ENGAGE;               //switch to engage state
            target = scan.getTarget();          //set target to player
        }
        else
        {
            state = State.PATROL;               //switch to partol state
            target = null;                      //set target null
        }
    }

    //patrol system
    void patrol()
    {
        if (!GC.paused)                                                                             //if game is not paused
        {
            if (ovDestination)                                                                      //if destination was overwritten
            {
                if (Vector3.Distance(this.transform.position, destination) < 170)                   //distance to destination is less than
                {
                    if (buffer)                                                                     //if buffer has destination set to buffered destination
                    {
                        destination = currentWaypoint;
                        buffer = false;
                    }

                    ovDestination = false;                                                          //set overwrite 
                    escapeDir.Clear();                                                              //clear escape diractions
                }
                else
                {
                    avoidObstacle(transform.forward, 0);                                            //scan for obstacles
                    scanForEnemy();                                                                 //scan for player
                    turn();                                                                         //turn towards destination
                    throttle(maxSpeed / 2);                                                         //move towrds destination
                }
            }
            else if (Vector3.Distance(this.transform.position, destination) >= distError)           //if AI is not close enough to way point
            {
                avoidObstacle(transform.forward, 0);
                scanForEnemy();
                turn();
                throttle(maxSpeed / 2);
            }
            else if (Vector3.Distance(this.transform.position, destination) < distError)            //if close enough to waypoint
            {
                waypointCounter = Random.Range(0, patrolPoints.Length);                             //pick random new waypoint
                destination = patrolPoints[waypointCounter].transform.position;                     //set new destination
                distError = Random.Range(1, 200);                                                   //set new distance from waypoint
            }
        }
    }

    //engage player
    void engage()
    {
        if (!GC.paused)                                                                             //if game is not paused
        {
            if (target == null)                                                                     //if target invalid switch to patrol state
            {
                state = State.PATROL;
            }

            if (!ovDestination)                                                                     //if no override
            {
                destination = target.transform.position;                                            //destination is target
            }
            if (ovDestination)                                                                      //if override
            {
                if (Vector3.Distance(this.transform.position, destination) < 350)                   //is distance shoter than
                {
                    if (buffer)                                                                     //has buffer
                    {
                        destination = target.transform.position;                                    //set destination
                        buffer = false;                                                             //turn off buffer
                    }

                    ovDestination = false;                                                          //turn of override
                    escapeDir.Clear();                                                              //clear escape diractions
                }
                else
                {
                    avoidObstacle(transform.forward, 0);                                            //avoid obstacles
                    scanForEnemy();                                                                 //scan for player
                    turn();                                                                         //turn towards destination
                    throttle(maxSpeed / 2);                                                         //move towards destination
                }
            }
            else if (Vector3.Distance(this.transform.localPosition, destination) < 350)             //if range to target is less
            {
                int r = Random.Range(0, 3);
                if (r == 2)                                                                         //chance to veer off
                {
                    Debug.Log(r);
                    veerAway();                                                                     //veer off for new approach
                }                                                                                   
            }
            else if (Vector3.Distance(this.transform.position, destination) < 550)                  //if distance is less
            {
                int r = Random.Range(0,20);
                if (r == 5)                                                                         //chance to veer off
                {
                    Debug.Log(r);
                    veerAway();                                                                     //veer off for new approach
                }

                avoidObstacle(transform.forward, 0);                                                //avoid obstacles
                scanForEnemy();                                                                     //scan for enemy
                turn();                                                                             //turn towards target
                throttle(PC.getSpeed());                                                            //move towards target
            }
            else if (Vector3.Distance(this.transform.position, destination) >= 550)                 //if distance is greater
            {
                avoidObstacle(transform.forward, 0);                                                //avoid obstacles
                scanForEnemy();                                                                     //scan for player
                turn();                                                                             //turn towards destination
                throttle(maxSpeed);                                                                 //move towards destination
            }
        }
    }

    //move around obstacle
    void avoidObstacle(Vector3 direction, float offSet)
    {
        escapeDir.Clear();                                                                              //clear escape directions
        RaycastHit[] hits = Rays(direction, offSet);                                                    //cast rays

        foreach(RaycastHit hit in hits)                                                                 //for each hit
        {
            if (hit.transform.root.gameObject != this.gameObject &&hit.transform.root.tag!="Player")    //if hit.root is not this or player
            {
                if(!buffer)                                                                             //if no destination is buffered
                {
                    currentWaypoint = destination;                                                      //save vector
                    obstacle = hit.transform;                                                           //set obstacle
                    buffer = true;                                                                      //set buffer true
                }

                navigateAroundObject(hit.collider);                                                     //find way around object
            } 
        }

        if (escapeDir.Count > 0)                                                                        //if has escape directions
        {
            ovDestination = false;                                                                      //set override false
            if(!ovDestination)                                                                          
            {
                destination = findShortest();                                                           //set new destination
                ovDestination = true;                                                                   //set override true
            }
        }
    }

    //check for ray cast hits
    RaycastHit[] Rays(Vector3 direction, float offSet)
    {
        Ray ray = new Ray(this.transform.position + new Vector3(offSet, 0, 0), direction);      //new ray from nose of ship

        float lookAhead = maxSpeed * 500;                                                       //check ahead distance
        RaycastHit[] hits = Physics.SphereCastAll(ray, 5, lookAhead);                           //cast rays

        return hits;
    }

    //find escape directions
    void navigateAroundObject(Collider obj)
    {
        RaycastHit HitUp;
        if (!Physics.Raycast(obj.transform.position, obj.transform.up, out HitUp, obj.bounds.extents.y * 5 + wingspan))         //check abouve object
        {
            Vector3 dir = obj.transform.position + new Vector3(0, obj.bounds.extents.y * 2 + wingspan, 0);                      //new position abouve object
            dir.x = this.transform.position.x;                                                                                  //set new x position to this.x position

            if (!escapeDir.Contains(dir))                                                                                       //if not in escape dir add it
                escapeDir.Add(dir);
        }

        RaycastHit HitDown;
        if (!Physics.Raycast(obj.transform.position, -obj.transform.up, out HitDown, obj.bounds.extents.y * 5+ wingspan))       //check abouve object
        {
            Vector3 dir = obj.transform.position + new Vector3(0, -obj.bounds.extents.y * 2 - wingspan, 0);                      //new position abouve object
            dir.x = this.transform.position.x;                                                                                  //set new x position to this.x position

            if (!escapeDir.Contains(dir))                                                                                       //if not in escape dir add it
                escapeDir.Add(dir);
        }

        RaycastHit HitRight;
        if (!Physics.Raycast(obj.transform.position, obj.transform.right, out HitRight, obj.bounds.extents.x * 5 + wingspan))   //check abouve object
        {
            Vector3 dir = obj.transform.position + new Vector3(obj.bounds.extents.x * 2 + wingspan, 0, 0);                      //new position abouve object            
            dir.y = this.transform.position.y;                                                                                  //set new x position to this.x position

            if (!escapeDir.Contains(dir))                                                                                       //if not in escape dir add it
                escapeDir.Add(dir);
        }

        RaycastHit HitLeft;
        if (!Physics.Raycast(obj.transform.position, -obj.transform.right, out HitLeft, obj.bounds.extents.x * 5 + wingspan))   //check abouve object
        {
            Vector3 dir = obj.transform.position + new Vector3(-obj.bounds.extents.x * 2 - wingspan, 0, 0);                      //new position abouve object
            dir.y = this.transform.position.y;                                                                                  //set new x position to this.x position

            if (!escapeDir.Contains(dir))                                                                                       //if not in escape dir add it
                escapeDir.Add(dir);
        }
    }

    //select shortest way
    Vector3 findShortest()
    {
        Vector3 found = escapeDir[0];                                                       //select first point as shortest
        float distance = Vector3.Distance(this.transform.position, escapeDir[0]);           //get distance from ship to point

        foreach(Vector3 escapePath in escapeDir)                                            //for each found direction
        {
            float tempDistance = Vector3.Distance(this.transform.position, escapePath);     //get distance
            if (tempDistance<distance)                                                      //if shorter set new shortest
            {
                distance = tempDistance;
                found = escapePath;
            }
        }

        return found;
    }

    //get system patrol points
    public void setPatrolPoints(GameObject[]pPoints)
    {
        patrolPoints = pPoints;
    }

    //move away from target
    private void veerAway()
    {
        ovDestination = true;                                                                   //set override true
        buffer = true;                                                                          //set buffer true

        destination = target.transform.position + new Vector3(Random.Range(500,800), Random.Range(500, 800), Random.Range(500, 800));   //set veer off destination
    }
}
