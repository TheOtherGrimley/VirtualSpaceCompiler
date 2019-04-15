using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
    public GameObject LoadingCube;
    public Text ApiUrl;
    private SelectedFile _activeFile; // Current file actively selected property.
    private List<Button> _menuButtons = new List<Button>();
    private List<string> _objectsToDetect = new List<string>();

    public SelectedFile ActiveFile
    {
        get
        {
            return _activeFile;
        }

        set
        {
            _activeFile = value;
        }
    }

    private void Start()
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Button"))
            _menuButtons.Add(g.GetComponent<Button>());
    }

    public void Submit()
    {
        if (ActiveFile.filename == null)
            Debug.LogError("No file selected");

        else if (_objectsToDetect.Count <= 0)
            Debug.LogError("No objects selected");

        else if (ApiUrl.text == "")
            Debug.LogError("No url entered");

        else
        {
            _buttonInteractChange(false);
            byte[] bytes = File.ReadAllBytes(ActiveFile.filepath);
            string img = System.Convert.ToBase64String(bytes);

            Debug.Log("Submitting");
            StartCoroutine(SendRequest(img));
        }
    }


    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ObjListChange(string obj, bool onoff)
    {
        if (onoff)
            _objectsToDetect.Add(obj);
        else
            _objectsToDetect.Remove(obj);
    }

    private void _buttonInteractChange(bool canInteract)
    {
        foreach (Button b in _menuButtons)
            b.interactable = canInteract;
    }

    IEnumerator SendRequest(string img)
    {
        LoadingCube.SetActive(true);
        reqData rawData = new reqData();
        rawData.objects = _objectsToDetect.ToArray();
        rawData.image = img;

        string data = JsonUtility.ToJson(rawData);

        UnityWebRequest req = UnityWebRequest.Put(ApiUrl.text, data);
        req.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Request sent");
        Metrics.Instance.TimerActiveSwitch();
        yield return req.SendWebRequest();

        if (req.isNetworkError || req.isHttpError)
        {
            LoadingCube.SetActive(false);
            Debug.Log(req.error);
            _buttonInteractChange(true);
            Metrics.Instance.TimerActiveSwitch();
        }
        else
        {
            LoadingCube.SetActive(false);
            _buttonInteractChange(true); //For debug, change in final
            Debug.Log("Upload complete!");
            GameObject.FindGameObjectWithTag("Global").GetComponent<SceneData>().ParseData(req.downloadHandler.text);
            Metrics.Instance.TimerActiveSwitch();
            SceneManager.LoadScene(1);
        }
    }

    struct reqData
    {
        public string[] objects;
        public string image;
    }
}
