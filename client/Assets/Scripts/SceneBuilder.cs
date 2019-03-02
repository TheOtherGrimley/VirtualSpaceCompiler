using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBuilder : MonoBehaviour {
    ParsedData _sceneToBuild;
    Camera _cam; 
    List<objConfig> configs = new List<objConfig>();

    private void Awake()
    {
        _cam = Camera.main.GetComponent<Camera>();
        loadSceneDataToBuild();
        loadObjectConfig();

        GameObject g = Instantiate(Resources.Load(configs[0].objName) as GameObject);
        g.transform.position = _cam.ScreenToWorldPoint(new Vector3((_sceneToBuild.centre[2] / 100) * _cam.pixelWidth, (_sceneToBuild.centre[3] / 100) * _cam.pixelHeight, _cam.nearClipPlane)) + (_cam.transform.forward * 1.1f);
        g.transform.rotation = Quaternion.Euler(new Vector3(35, 193, 8)); //hardcoded rotation of cup
        GameObject floor = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Plane), g.transform);
        floor.transform.localPosition = new Vector3(0, configs[0].rel_base, 0);
        floor.transform.rotation = g.transform.rotation;
    }

    private void loadObjectConfig()
    {

        TextAsset t = (Resources.Load("ObjConfig") as TextAsset);

        JsonData s = JsonMapper.ToObject(t.text);
        Debug.Log(s);

        for (int i = 0; i < s[0].Count; i++)
        {
            objConfig temp = new objConfig();
            temp.objId = (int)s[0][i][0];
            temp.objName = (string)s[0][i][1];
            // Jsondata has a bug where you can't direct cast to float. Too much effort to fix, use this instead:
            temp.rel_base = float.Parse(s[0][i][2].ToString());
            configs.Add(temp);
        }
        
    }

    private void loadSceneDataToBuild()
    {
        try
        {
            _sceneToBuild = GameObject.FindGameObjectWithTag("Global").GetComponent<SceneData>().data;
            if (_sceneToBuild.centre == null)
            {
                Debug.LogError("No scene data found, loading main menu.");
                SceneManager.LoadScene(0);
            }
        }
        catch(System.NullReferenceException e)
        {
            Debug.LogError("No global data found, loading main menu.");
            SceneManager.LoadScene(0);
        }
    }

    struct objConfig
    {
        public int objId;
        public string objName;
        public float rel_base;
    }
}
