using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;

public class SceneData : MonoBehaviour {
    ParsedData _data;
    public ParsedData data {
        get { return _data; }
        set { _data = value; }
    }

    string _rawRequestData;
        
    public void ParseData(string json)
    {
        _rawRequestData = json;
        Debug.Log(json);
        Debug.Log(cleanRequestData());
        _data = JsonUtility.FromJson<ParsedData>(cleanRequestData());
    }

    
    private void Start()
    {
        DontDestroyOnLoad(this);

        // --------------------------------------
        // FOR DEBUG | DELETE IN FINAL
        // --------------------------------------
        

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
}

[System.Serializable]
public struct CropData
{
    public float[] centre;
    public string filename;
    public string object_type;
}

struct objConfig
{
    public int objId;
    public string objName;
    public float rel_base;
}