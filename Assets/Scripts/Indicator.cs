using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class Indicator : MonoBehaviour {
    public GameObject targetSquare;
    private  List<GameObject> targets=new List<GameObject>();
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        GameObject[] indi = GameObject.FindGameObjectsWithTag("indicator");
        foreach (GameObject ind in indi)
        {
            Destroy(ind);
        }

        if (targets.Count == 0)
        {
            GameObject[] t = GameObject.FindGameObjectsWithTag("AIShips");
            foreach (GameObject AI in t)
            {
                targets.Add(AI);
            }
        }

        foreach (GameObject t in targets)
        {
            if (t == null)
            {
                targets.Remove(t);
            }
            else
            {
                displayIndicator(t);
            }
        }

    }

    private void displayIndicator(GameObject target)
    {
        Vector3 screenpos = Camera.main.WorldToScreenPoint(target.transform.position);

        if (screenpos.z > 0 &&
            screenpos.x > 0 && screenpos.x < Screen.width &&
            screenpos.y > 0 && screenpos.y < Screen.height)
        {
            Debug.Log("onscreen");
            GameObject indicator = createIndicator(targetSquare);
            indicator.transform.position = screenpos;
        }            
    }

    private GameObject createIndicator(GameObject indicator)
    {
        GameObject ind;

        ind = Instantiate(indicator) as GameObject;
        ind.transform.parent = transform;

        return ind;
    }
}
