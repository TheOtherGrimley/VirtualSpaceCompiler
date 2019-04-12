using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;

public class SceneData : MonoBehaviour {
    ParsedData _data;
    public ParsedData Data {
        get { return _data; }
        set { _data = value; }
    }
    private string _style;
    public string Style
    {
        get { return _style; }
        set { _style = value; }
    }

    string _rawRequestData;
        
    public void ParseData(string json)
    {
        _rawRequestData = json;
        if (Metrics.Instance.MetricsEnabled)
            Metrics.Instance.FullResponse = cleanRequestData();
        _data = JsonUtility.FromJson<ParsedData>(cleanRequestData());

    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private string cleanRequestData()
    {
        return _rawRequestData.TrimStart('[', '"').Replace("\"]", "").Replace("\\", "").Replace("}\"", "}");
    }
}


[System.Serializable]
public class ParsedData
{
    public CropData[] crops;
    public Keypoints[] keypoints;
}

[System.Serializable]
public struct CropData
{
    public float[] centre;
    public string filename;
    public string object_type;
}

[System.Serializable]
public struct Keypoints
{
    public float averageDepth;
    public float[] orientRet;
    public float[] points;
}

struct objConfig
{
    public int objId;
    public string objName;
    public float rel_base;
}