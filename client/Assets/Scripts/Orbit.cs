using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public float speed = 1;
    private Vector3 _centrePoint;

    public Vector3 CentrePoint
    {
        get
        {
            return _centrePoint;
        }
        set
        {
            _centrePoint = value;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            //Capture the cursor when orbiting
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            //read mouse input and multiply values by the speed value
            float h = speed * Input.GetAxis("Mouse X");
            float v = speed * Input.GetAxis("Mouse Y");

            //Rotate around the raycast hit point using mouse input
            this.transform.RotateAround(_centrePoint, ((Vector3.up * h) + (Vector3.right * v)), 1);
            //Clamp z rotation value
            this.transform.rotation = Quaternion.Euler(new Vector3(this.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y, 0f));
           
        }
        else
        {
            //Unlock cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
