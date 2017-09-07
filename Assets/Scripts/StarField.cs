using UnityEngine;
using System.Collections;

public class StarField : MonoBehaviour {

    private ParticleSystem.Particle[] stars;                                                    //particles in starfield
    private ParticleSystem Starfield;                                                           //particle system

    private int maxStars;                                                                       //max stars
    private float starSize;                                                                     //star size
    private float Distance;                                                                     //perimiter for starfield

	// Use this for initialization
	void Start () {
        Starfield = this.GetComponent<ParticleSystem>();                                        //get particle system
        maxStars = 1000;                                                                        //set max stars
        starSize = 0.5f;                                                                        //set star size
        Distance = 100;                                                                         //set distance

        createStarField();                                                                      //create starfield
    }
	
	// Update is called once per frame
	void Update () {
	    if(stars==null)                                                                         //if no stars exist
        {
            createStarField();                                                                  //create stars
        }

        Starfield.SetParticles(stars, stars.Length);                                            //display stars in particle emitter
	}


    //create star field
    private void createStarField()                                  
    {
        stars = new ParticleSystem.Particle[maxStars];                                          //set stars array

        for(int x=0; x<maxStars; x++)                                                           //run for max stars
        {
            stars[x].position = Random.insideUnitSphere * Distance + this.transform.position;   //set position to random within sphere
            stars[x].color = new Color(1, 1, 1, 1);                                             //set color to white
            stars[x].size = starSize;                                                           //set star size
        }
    }
}
