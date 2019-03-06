using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleInit : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Toggle thisTog = this.gameObject.GetComponent<Toggle>();
        OpenFileDialog diag = GameObject.FindGameObjectWithTag("MenuCtrl").GetComponent<OpenFileDialog>();

        thisTog.onValueChanged.AddListener(delegate { diag.ObjListChange(this.GetComponentInChildren<Text>().text, thisTog.isOn); });
    }
	
}
