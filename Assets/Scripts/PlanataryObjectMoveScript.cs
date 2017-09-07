using UnityEngine;
using System.Collections;

public class PlanataryObjectMoveScript : MonoBehaviour
{
    public GameControler GC;    //game controller script
    public float rotSpeed;      //rotation speed

    // Use this for initialization
    void Start()
    {
        GC = GameObject.FindGameObjectWithTag("GC").GetComponent<GameControler>();      //find game controller
    }

    // Update is called once per frame
    void Update()
    {
        if (!GC.paused) //if game is not paused
        {
            transform.Rotate(0f, rotSpeed * Time.deltaTime, 0f);    //rotate around Y axis
        }
    }
}
