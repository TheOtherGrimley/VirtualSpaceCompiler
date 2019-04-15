using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour {
    public float xIntensity;
    public float yIntensity;
    public float zIntensity;

	// Update is called once per frame
	void Update () {
        this.gameObject.transform.Rotate(new Vector3(xIntensity, yIntensity, zIntensity));
	}
}
