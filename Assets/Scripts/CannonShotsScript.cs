using UnityEngine;
using System.Collections;

public class CannonShotsScript : MonoBehaviour {
    public float speed;                 //float weapon speed
    public Rigidbody weapon;            //Rigidbody of blaster

    public float range;                 //max range

    private Vector3 start;              //start position
    private GameObject firedFrom;       //gameobject of firing ship

    private float damageShields;        //damage to shields
    private float damageHull;           //damage to hull

	// Use this for initialization
	void Start () {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Raycast"), LayerMask.NameToLayer("Ignore Coll With Scan"), true);    //set collision to ignore Scan perimiter
        transform.LookAt(GameObject.FindGameObjectWithTag("crosshair").transform.position);                                             //rotate towards crosshair

        damageHull = 20;                                                                                                                //set damage to hull
        damageShields = 20;                                                                                                             //set damage to shields
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance(start, weapon.transform.position) > range)                                                                 //if max distance is reached
            Destroy(this.gameObject);                                                                                                   //destroy
    }

    //on fire call
    public void onFire(Vector3 target, GameObject Shooter)
    {
        firedFrom = Shooter;                                                    //set shooter
        transform.LookAt(target);                                               //look at target
        start = weapon.transform.position;                                      //set start
        weapon.AddForce(weapon.transform.forward*speed, ForceMode.Impulse);     //aplly force impulse to shoot projectile
    }

    //on trigger enter event
    void OnTriggerEnter(Collider obj)
    {
        if (obj.transform.root.gameObject != firedFrom)     //if collider is not part of shooter
        {
            obj.transform.root.GetComponent<ShipEventController>().onHitByWeapon(damageShields, damageHull);        //apply damage to ship
            Destroy(this.gameObject);                                                                               //destroy
        }
    }

    //read max range
    public float getMaxFireRange()
    {
        return range;
    }
}
