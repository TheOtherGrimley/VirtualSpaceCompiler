using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class Metrics : MonoBehaviour {
    public static Metrics Instance;

    public string MetricsDirectory
    {
        get { return _metricsDirectory; }
        set { _metricsDirectory = value; }
    }
    public float reqTime {
        get { return _reqTime; }
    }
    public float ImageSize
    {
        get { return _imgSize; }
        set { _imgSize = value; }
    }
    public bool DumpMetrics
    {
        set { _dumpMetrics = value; }
    }

    public string FullResponse
    {
        get
        {
            return _fullResponse;
        }

        set
        {
            _fullResponse = JsonUtility.ToJson(value, true);
        }
    }
    public bool MetricsEnabled
    {
        get
        {
            return _metricsEnabled;
        }
        set
        {
            _metricsEnabled = value;
        }
    }

    private float _reqTime;
    private float _imgSize;
    private string _metricsDirectory;
    private bool _dumpMetrics;
    private float imageSize;
    private bool _timerActive;
    private bool _metricsEnabled = false;

    private string _fullResponse;

    public void TimerActiveSwitch()
    {
        _timerActive = !_timerActive;
    }

	void Start () {
        if (Metrics.Instance == null)
            Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
        if (_timerActive)
            _reqTime += Time.deltaTime;
	}

    private void OnApplicationQuit()
    {
        if (_dumpMetrics)
        {
            SavedMetrics s = new SavedMetrics();
            s.RequestTime = reqTime + "s";
            s.FileSize = ImageSize + "Mb";
            s.keypoints = GameObject.FindGameObjectWithTag("Global").GetComponent<SceneData>().Data.keypoints;

            if (_metricsDirectory == null)
                _metricsDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            File.WriteAllText(_metricsDirectory + "\\log-" + DateTime.Now.ToFileTime(), JsonUtility.ToJson(s, true));
        }

    }

    [System.Serializable]
    private struct SavedMetrics
    {
        public string RequestTime;
        public string FileSize;
        public Keypoints[] keypoints;
    }
}
