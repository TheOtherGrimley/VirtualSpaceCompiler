using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour {
    private bool _metricsEnabled = false;
    public bool MetricsEnabled
    {
        set
        {
            _metricsEnabled = value;
        }
    }
    public Text timerText;
    public Text imageSize;
    public GameObject metricsDirectory;

    private void Start()
    {
        metricsDirectory.GetComponent<InputField>().onEndEdit.AddListener(delegate { checkDirectory(metricsDirectory.GetComponent<InputField>().text); });
    }

    void Update () {
        if (_metricsEnabled)
        {
            timerText.text = Metrics.Instance.reqTime.ToString("F1") + "s";
            imageSize.text = Metrics.Instance.ImageSize.ToString("F2") + "Mb";
        }
	}

    void checkDirectory(string dir)
    {
        if (Directory.Exists(dir))
            Metrics.Instance.metricsDirectory = dir;
        else
            metricsDirectory.GetComponent<InputField>().text = "Directory does not exist";
    }
}
