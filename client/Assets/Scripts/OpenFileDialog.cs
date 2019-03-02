using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.SceneManagement;

public class OpenFileDialog : MonoBehaviour {
    public Transform buttonParent;
    public GameObject buttonPrefab;
    public Text selectedFileLabel;
    public Text API_URL;
    public GameObject loadingCube;

    List<Button> _menuButtons = new List<Button>();
    SelectedFile activeFile;
    List<SelectedFile> foundFiles = new List<SelectedFile>();
    int movCount = 0;
    int xOffset=-200;

    private void Start()
    {
        string sourceDirectory = @"C:\\Users\\AdamG\\Desktop";
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Button"))
            _menuButtons.Add(g.GetComponent<Button>());

        try
        {
            string[] txtFiles = Directory.GetFiles(sourceDirectory, "*.jpg");

            foreach (string currentFile in txtFiles)
            {
                SelectedFile temp = new SelectedFile();
                temp.button = Instantiate(buttonPrefab, buttonParent);
                temp.filepath = currentFile;
                temp.filename = currentFile.Split('\\')[currentFile.Split('\\').Length-1];
                temp.button.GetComponentInChildren<Text>().text = temp.filename;
                temp.button.name = "btn" + temp.filename;
                temp.button.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset, 100 - 20 * movCount);
                temp.button.GetComponent<Button>().onClick.AddListener(delegate { btnClick(temp); });
                foundFiles.Add(temp);
                Debug.Log(temp.filename);
                if(movCount == 10)
                {
                    movCount = 0;
                    xOffset += 50;
                }
                movCount += 1;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
        
    }

    public void submit()
    {
        if(activeFile.filename == null)
        {
            Debug.LogError("No file selected");
        }

        else if(API_URL.text == "")
        {
            Debug.LogError("No url entered");
        }

        else
        {
            ButtonInteractChange(false);
            byte[] bytes = File.ReadAllBytes(activeFile.filepath);
            string img = System.Convert.ToBase64String(bytes);

            Debug.Log("Submitting");
            StartCoroutine(sendRequest(img));
        }
    }

    void btnClick(SelectedFile arg)
    {
        arg.button.gameObject.transform.parent.gameObject.SetActive(false);
        activeFile = arg;
        selectedFileLabel.text = arg.filename.Replace(".jpg", "");
    }

    void ButtonInteractChange(bool canInteract)
    {
        foreach (Button b in _menuButtons)
            b.interactable = canInteract;
    }

    IEnumerator sendRequest(string img)
    {
        loadingCube.SetActive(true);
        reqData rawData = new reqData();
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

    struct SelectedFile
    {
        public GameObject button;
        public string filepath;
        public string filename;
    }

    struct reqData
    {
        public string image;
    }

}
