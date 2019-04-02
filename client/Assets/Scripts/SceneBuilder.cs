using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBuilder : MonoBehaviour {
    public bool InDebug = false;

    ParsedData _sceneToBuild;
    Camera _cam; 
    List<objConfig> configs = new List<objConfig>();
    bool _firstItem = true;

    private void Awake()
    {
        _cam = Camera.main.GetComponent<Camera>();
        loadSceneDataToBuild();
        loadObjectConfig();
        
        foreach(CropData crop in _sceneToBuild.crops)
        {
            switch (crop.object_type)
            {
                case "cup":
                    loadCup(crop);
                    break;
                case "bowl":
                    loadBowl(crop);
                    break;
                default:
                    Debug.LogError("Object type invalid");
                    break;
            }
        }

        RaycastHit hit;
        if(Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, 10)){
            _cam.GetComponent<Orbit>().centrePoint = hit.point;
        }
    }

    private void loadBowl(CropData c)
    {
        GameObject g = Instantiate(Resources.Load(configs[1].objName) as GameObject);
        g.transform.position = _cam.ScreenToWorldPoint(new Vector3(((100-c.centre[3]) / 100) * _cam.pixelWidth, ((100-c.centre[2]) / 100) * _cam.pixelHeight, 2f));
        g.transform.rotation = Quaternion.Euler(new Vector3(35, 193, 8)); //hardcoded rotation of cup
        if (_firstItem)
        {
            GameObject floor = Instantiate(Resources.Load("tables\\standard") as GameObject, g.transform);
            floor.transform.localPosition = new Vector3(0, configs[0].rel_base, 0);
            floor.transform.rotation = g.transform.rotation;
            _firstItem = !_firstItem;
        }
    }

    private void loadCup(CropData c)
    {
        GameObject g = Instantiate(Resources.Load(configs[0].objName) as GameObject);
        g.transform.position = _cam.ScreenToWorldPoint(new Vector3((c.centre[2] / 100) * _cam.pixelWidth, (c.centre[3] / 100) * _cam.pixelHeight, _cam.nearClipPlane)) + (_cam.transform.forward * 1.5f);
        g.transform.rotation = Quaternion.Euler(new Vector3(35, 193, 8)); //hardcoded rotation of cup
        if (_firstItem)
        {
            GameObject floor = Instantiate(Resources.Load("tables\\standard") as GameObject, g.transform);
            floor.transform.localPosition = new Vector3(0, configs[0].rel_base, 0);
            floor.transform.rotation = g.transform.rotation;
            _firstItem = !_firstItem;
        }
    }

    private void loadObjectConfig()
    {

        TextAsset t = (Resources.Load("ObjConfig") as TextAsset);

        JsonData s = JsonMapper.ToObject(t.text);
        Debug.Log(s);

        for(int j = 0; j < s.Count; j++)
            for (int i = 0; i < s[0].Count; i++)
            {
                objConfig temp = new objConfig();
                temp.objId = (int)s[j][i][0];
                temp.objName = (string)s[j][i][1];
                // Jsondata has a bug where you can't direct cast to float. Too much effort to fix, use this instead:
                temp.rel_base = float.Parse(s[j][i][2].ToString());
                configs.Add(temp);
            }
        
    }

    private void loadSceneDataToBuild()
    {
        try
        {
            _sceneToBuild = GameObject.FindGameObjectWithTag("Global").GetComponent<SceneData>().data;
            if (_sceneToBuild.crops == null && !InDebug)
            {
                Debug.LogError("No scene data found, loading main menu.");
                SceneManager.LoadScene(0);
            }
        }
        catch(System.NullReferenceException e )
        {
            if (!InDebug) {
                Debug.LogError("No global data found, loading main menu.");
                SceneManager.LoadScene(0);
            }
        }
    }

    struct objConfig
    {
        public int objId;
        public string objName;
        public float rel_base;
    }
}
