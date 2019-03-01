using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientIn : MonoBehaviour {
    public List<Vector2> orientMatrices = new List<Vector2>();
    public GameObject a;
    public GameObject b;

    public GameObject cup;

	// Use this for initialization
	void Start () {
        Vector3 Yvec = a.transform.position - b.transform.position;
        cup.transform.forward = -Yvec;
        
    }
	
}
