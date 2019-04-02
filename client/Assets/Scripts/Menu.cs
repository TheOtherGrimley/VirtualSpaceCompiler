using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
    public GameObject loadingCube;
    public Text API_URL;
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

    public void ObjListChange(string obj, bool onoff)
    {
        if (onoff)
            _objectsToDetect.Add(obj);
        else
            _objectsToDetect.Remove(obj);
    }

    private void Start()
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Button"))
            _menuButtons.Add(g.GetComponent<Button>());
    }

    public void submit()
    {
        if (ActiveFile.filename == null)
            Debug.LogError("No file selected");

        else if (_objectsToDetect.Count <= 0)
            Debug.LogError("No objects selected");

        else if (API_URL.text == "")
            Debug.LogError("No url entered");

        else
        {
            ButtonInteractChange(false);
            byte[] bytes = File.ReadAllBytes(ActiveFile.filepath);
            string img = System.Convert.ToBase64String(bytes);

            Debug.Log("Submitting");
            StartCoroutine(sendRequest(img));
        }
    }

    void ButtonInteractChange(bool canInteract)
    {
        foreach (Button b in _menuButtons)
            b.interactable = canInteract;
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    IEnumerator sendRequest(string img)
    {
        loadingCube.SetActive(true);
        reqData rawData = new reqData();
        rawData.objects = _objectsToDetect.ToArray();
        rawData.image = img;

        string data = JsonUtility.ToJson(rawData);

        UnityWebRequest req = UnityWebRequest.Put(API_URL.text, data);
        req.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Request sent");
        yield return req.SendWebRequest();

        if (req.isNetworkError || req.isHttpError)
        {
            loadingCube.SetActive(false);
            Debug.Log(req.error);
            ButtonInteractChange(true);
        }
        else
        {
            loadingCube.SetActive(false);
            ButtonInteractChange(true); //For debug, change in final
            Debug.Log("Upload complete!");
            GameObject.FindGameObjectWithTag("Global").GetComponent<SceneData>().ParseData(req.downloadHandler.text);
            SceneManager.LoadScene(1);
        }
    }

    struct reqData
    {
        public string[] objects;
        public string image;
    }
}
