using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

public class TargetControl : MonoBehaviour
{

    public GameObject player;       //player object
    public GameObject target;       //target object
    public GameObject camera;       //camera object
    private GameControler GC;       //game controller script
    public float targetAngle;       //max target angle

    private float targetHor;        //value input player
    private float targetVer;        //value input player

    // Use this for initialization
    void Start()
    {
        GC = GameObject.FindGameObjectWithTag("GC").GetComponent<GameControler>();  //find game controller
        camera = GameObject.FindGameObjectWithTag("MainCamera");                    //find main camera
    }

    // Update is called once per frame
    void Update()
    {
        if (!GC.paused) //if game is not paused
        {
            targetHor = CrossPlatformInputManager.GetAxis("TargetHorizontal");  //read horizontal input
            targetVer = -CrossPlatformInputManager.GetAxis("TargetVertical");   //read vertical input

            transform.position = player.transform.position;                     //set transform to player transform
            Vector3 rotation = transform.localEulerAngles;                      //read local euler angles

            if ((targetHor < 0 && (rotation.y > (360 - targetAngle) || rotation.y < targetAngle + 10)) || (targetHor > 0 && (rotation.y < targetAngle || rotation.y > (360 - targetAngle - 10)))) //if user input and crosshair within limits
            {
                transform.RotateAround(transform.position, player.transform.up, targetHor / 4);     //rotate around players y axis
            }

            if ((targetVer < 0 && (rotation.x > (360 - targetAngle / 2) || rotation.x < targetAngle / 2 + 10)) || (targetVer > 0 && (rotation.x < targetAngle / 2 || rotation.x > (360 - targetAngle / 2 - 10)))) //if user input and crosshair within limits
            {
                transform.RotateAround(transform.position, transform.right, targetVer / 4);         //rotate around players x axis
            }
            target.transform.LookAt(camera.transform.position);     //crosshiar look at camera
        }
    }
}
