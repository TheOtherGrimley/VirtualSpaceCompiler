using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneData : MonoBehaviour {
    ParsedData data;

    string _rawRequestData;
    
    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void ParseData(string json)
    {
        _rawRequestData = json;
        data = JsonUtility.FromJson<ParsedData>(cleanRequestData());
    }

    private string cleanRequestData()
    {
        return _rawRequestData.TrimStart('[', '"').Replace("\"]", "").Replace("\\", "");
    }
}

[System.Serializable]
public struct ParsedData
{
    public float[] centre;
    public string filename;
}
