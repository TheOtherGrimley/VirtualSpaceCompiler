using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour {
    public Text timerText;
    public Text imageSize;
    public GameObject metricsDirectory;

    private void Start()
    {
        metricsDirectory.GetComponent<InputField>().onEndEdit.AddListener(delegate { _checkDirectory(metricsDirectory.GetComponent<InputField>().text); });
    }

    void Update () {
        if (Metrics.Instance.MetricsEnabled)
        {
            timerText.text = Metrics.Instance.ReqTime.ToString("F1") + "s";
            imageSize.text = Metrics.Instance.ImageSize.ToString("F2") + "Mb";
        }
	}

    public void SetMetrics(bool show)
    {
        Metrics.Instance.MetricsEnabled = show;
    }

    private void _checkDirectory(string dir)
    {
        if (Directory.Exists(dir))
            Metrics.Instance.MetricsDirectory = dir;
        else
            metricsDirectory.GetComponent<InputField>().text = "Directory does not exist";
    }
}
