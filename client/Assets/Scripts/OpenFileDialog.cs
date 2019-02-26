using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class OpenFileDialog : MonoBehaviour {
    public Transform buttonParent;
    public GameObject buttonPrefab;
    public Text selectedFileLabel;

    SelectedFile activeFile;
    List<SelectedFile> foundFiles = new List<SelectedFile>();
    int movCount = 0;
    int xOffset=-200;

    private void Start()
    {
        DontDestroyOnLoad(this);

        string sourceDirectory = @"C:\\Users\\AdamG\\Desktop";

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
    }

    void btnClick(SelectedFile arg)
    {
        arg.button.gameObject.transform.parent.gameObject.SetActive(false);
        activeFile = arg;
        selectedFileLabel.text = arg.filename.Replace(".jpg", "");
    }

    struct SelectedFile
    {
        public GameObject button;
        public string filepath;
        public string filename;
    }
    
}
