using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleInit : MonoBehaviour {
	// Use this for initialization of toggle buttons
    // Essentially links the toggle buttons to the menu as the application begins.

	void Start () {
        Toggle thisTog = this.gameObject.GetComponent<Toggle>();
        Menu diag = GameObject.FindGameObjectWithTag("MenuCtrl").GetComponent<Menu>();

        thisTog.onValueChanged.AddListener(delegate { diag.ObjListChange(this.GetComponentInChildren<Text>().text, thisTog.isOn); });
    }
}
