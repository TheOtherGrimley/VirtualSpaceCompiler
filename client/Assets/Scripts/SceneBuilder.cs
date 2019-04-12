using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneBuilder : MonoBehaviour {
    public bool InDebug = false;
    public Text MetricsText;
    public GameObject MetricsPanel;

    ParsedData _sceneToBuild;
    Camera _cam; 
    List<ObjConfig> configs = new List<ObjConfig>();
    bool _firstItem = true;
    GameObject table;

    private void Start()
    {
        _cam = Camera.main.GetComponent<Camera>();
        Debug.Log(_cam.gameObject.name);
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

        resetWorldOrientation();

        RaycastHit hit;
        if(Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, 10)){
            _cam.GetComponent<Orbit>().centrePoint = hit.point;
        }

        if (Metrics.Instance.MetricsEnabled)
        {
            MetricsPanel.SetActive(false);
            MetricsText.text = Metrics.Instance.FullResponse;
        }
    }

    private void loadBowl(CropData c)
    {
        GameObject g = Instantiate(Resources.Load(configs[1].objName) as GameObject);
        g.transform.position = _cam.ScreenToWorldPoint(new Vector3(((100-c.centre[3]) / 100) * _cam.pixelWidth, ((100-c.centre[2]) / 100) * _cam.pixelHeight, 2f));
        g.transform.rotation = Quaternion.Euler(new Vector3(35, 193, 8)); //hardcoded rotation of cup
        if (_firstItem)
        {
            table = Instantiate(Resources.Load("tables\\standard") as GameObject, g.transform);
            table.transform.localPosition = new Vector3(0, configs[0].rel_base, 0);
            table.transform.rotation = g.transform.rotation;
            _firstItem = !_firstItem;
        }
    }

    private void loadCup(CropData c)
    {
        GameObject g = Instantiate(Resources.Load(configs[0].objName) as GameObject);
        g.transform.position = _cam.ScreenToWorldPoint(new Vector3(((c.centre[3]) / 100) * _cam.pixelWidth, ((100-c.centre[2]) / 100) * _cam.pixelHeight, 1.5f));
        g.transform.rotation = Quaternion.Euler(new Vector3(35, 193, 8)); //hardcoded rotation of cup
        if (_firstItem)
        {
            table = Instantiate(Resources.Load("tables\\standard") as GameObject, g.transform);
            table.transform.localPosition = new Vector3(0, configs[0].rel_base, 0);
            table.transform.rotation = g.transform.rotation;
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
                ObjConfig temp = new ObjConfig();
                temp.objId = (int)s[j][i][0];
                temp.objName = (string)s[j][i][1];
                // Jsondata has a bug where you can't direct cast to float. Too much effort to fix for this project plan, use this instead:
                temp.rel_base = float.Parse(s[j][i][2].ToString());
                temp.style = (string)s[j][i][3];
                configs.Add(temp);
            }
    }

    private void resetWorldOrientation()
    {
        foreach(GameObject g in GameObject.FindObjectsOfType<GameObject>())
        {
            if (g != table && (g.tag == "Untagged" || g.tag == "MainCamera"))
            {
                g.transform.parent = table.transform;
            }
        }
        table.transform.up = Vector3.up;
    }

    private void loadSceneDataToBuild()
    {
        try
        {
            _sceneToBuild = GameObject.FindGameObjectWithTag("Global").GetComponent<SceneData>().Data;
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

    public void SaveScene()
    {
        ObjectsToSave objects = new ObjectsToSave();
        objects.ObjectList = new List<SaveObject>();

        foreach (GameObject g in FindObjectsOfType<GameObject>())
        {
            SaveObject temp = new SaveObject();
            temp.name = g.name;
            temp.position = g.transform.position;
            temp.Rotation = g.transform.rotation.eulerAngles;
            objects.ObjectList.Add(temp);
        }

        if (Metrics.Instance.MetricsDirectory == null)
        {
            if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "//VSCLogs"))
                Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "//VSCLogs");
            Metrics.Instance.MetricsDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "//VSCLogs";
        }
        File.WriteAllText(Metrics.Instance.MetricsDirectory + "\\scene-" + DateTime.Now.ToFileTime()+".json", JsonUtility.ToJson(objects, true));
    }

    [System.Serializable]
    struct SaveObject
    {
        public string name;
        public Vector3 position;
        public Vector3 Rotation;
    }

    [System.Serializable]
    struct ObjectsToSave
    {
        public List<SaveObject> ObjectList;
    }

    struct ObjConfig
    {
        public int objId;
        public string objName;
        public float rel_base;
        public string style;
    }
}
