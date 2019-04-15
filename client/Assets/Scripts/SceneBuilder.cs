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
    int _style = 0;
    Camera _cam; 
    List<ObjConfig> configs = new List<ObjConfig>();
    bool _firstItem = true;
    GameObject table;

    private void Start()
    {
        _cam = Camera.main.GetComponent<Camera>();
        Debug.Log(_cam.gameObject.name);
        _loadSceneDataToBuild();
        _loadObjectConfig();
        
        foreach(CropData crop in _sceneToBuild.crops)
        {
            switch (crop.object_type)
            {
                case "cup":
                    _loadCup(crop);
                    break;
                case "bowl":
                    _loadBowl(crop);
                    break;
                default:
                    Debug.LogError("Object type invalid");
                    break;
            }
        }

        _resetWorldOrientation();

        RaycastHit hit;
        if(Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, 10)){
            _cam.GetComponent<Orbit>().centrePoint = hit.point;
        }

        if (Metrics.Instance.MetricsEnabled)
        {
            MetricsPanel.SetActive(true);
            MetricsText.text = Metrics.Instance.FullResponse;
        }
        else
            MetricsPanel.SetActive(true);
    }

    private void _loadBowl(CropData c)
    {
        List<ObjConfig> bowls = new List<ObjConfig>();
        for (int i = 0; i < configs.Count; i++)
            if (configs[i].type == "bowl")
                bowls.Add(configs[i]);
        ObjConfig bowlToUse = bowls[UnityEngine.Random.Range(0, bowls.Count - 1)];
        GameObject g = Instantiate(Resources.Load(bowlToUse.objName) as GameObject);
        g.transform.position = _cam.ScreenToWorldPoint(new Vector3(((c.centre[3]) / 100) * _cam.pixelWidth, ((100-c.centre[2]) / 100) * _cam.pixelHeight, 1.75f));
        g.transform.position += g.transform.up * -0.25f;
        g.transform.rotation = Quaternion.Euler(new Vector3(-64 + 0.646f, 5- 4.331f, -4 - 8.968f)); //hardcoded rotation of bowl
        _initTable(g, bowlToUse);
    }

    private void _loadCup(CropData c)
    {
        List<ObjConfig> cups = new List<ObjConfig>();
        for (int i = 0; i < configs.Count; i++)
            if (configs[i].type == "cup")
                cups.Add(configs[i]);
        ObjConfig cupToUse = cups[UnityEngine.Random.Range(0, cups.Count - 1)];
        GameObject g = Instantiate(Resources.Load(cupToUse.objName) as GameObject);
        g.transform.position = _cam.ScreenToWorldPoint(new Vector3(((c.centre[3]) / 100) * _cam.pixelWidth, ((100-c.centre[2]) / 100) * _cam.pixelHeight, 1.5f));
        g.transform.rotation = Quaternion.Euler(new Vector3(-64, -5, -4)); //hardcoded rotation of cup
        _initTable(g, cupToUse);
    }

    private void _initTable(GameObject g, ObjConfig obj)
    {
        if (_firstItem)
        {
            switch (_style)
            {
                case 0:
                    table = Instantiate(Resources.Load("tables\\Plane") as GameObject, g.transform);
                    break;
                case 1:
                    table = Instantiate(Resources.Load("tables\\standard") as GameObject, g.transform);
                    break;
            }
            table.transform.localPosition = new Vector3(0, obj.rel_base, 0);
            table.transform.rotation = g.transform.rotation;
            _firstItem = !_firstItem;
        }
    }

    private void _loadObjectConfig()
    {

        TextAsset t = (Resources.Load("ObjConfig") as TextAsset);

        JsonData s = JsonMapper.ToObject(t.text);
        Debug.Log(s);

        for(int j = 0; j < s.Count; j++)
            for (int i = 0; i < s[j].Count; i++)
                if((int)s[j][i][3] == _style)
                {
                    ObjConfig temp = new ObjConfig();
                    temp.objId = (int)s[j][i][0];
                    temp.objName = (string)s[j][i][1];
                    // Jsondata has a bug where you can't direct cast to float. Too much effort to fix for this project plan, use this instead:
                    temp.rel_base = float.Parse(s[j][i][2].ToString());
                    switch (j)
                    {
                        case 0:
                            temp.type = "cup";
                            break;
                        case 1:
                            temp.type = "bowl";
                            break;
                    }
                    configs.Add(temp);
                }
    }

    private void _resetWorldOrientation()
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

    private void _loadSceneDataToBuild()
    {
        try
        {
            _sceneToBuild = GameObject.FindGameObjectWithTag("Global").GetComponent<SceneData>().Data;
            _style = GameObject.FindGameObjectWithTag("Global").GetComponent<SceneData>().Style;
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
        public int style;
        public string type;
    }
}
