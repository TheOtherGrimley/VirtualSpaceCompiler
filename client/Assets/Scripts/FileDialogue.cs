using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FileDialogue : MonoBehaviour {
    public Transform buttonParent;
    public GameObject buttonPrefab;
    public Text selectedFileLabel;
    public Text API_URL;
    public GameObject loadingCube;

    
    private ImageLoader imgldr; // Used to refer to the image loader in the menu scripts
    private Menu _menuController;
    List<SelectedFile> foundFiles = new List<SelectedFile>();

    // Counter variables used to format the custom file explorer
    int movCount = 0;
    int xOffset = -200;

    

    private void Start()
    {
        _menuController = this.GetComponent<Menu>();
        
        imgldr = GameObject.FindGameObjectWithTag("MenuCtrl").GetComponent<ImageLoader>();
        _initFileExplorer();
    }

    private void _initFileButton(string file)
    {
        SelectedFile temp = new SelectedFile();
        temp.button = Instantiate(buttonPrefab, buttonParent);
        temp.filepath = file;
        temp.filename = file.Split('\\')[file.Split('\\').Length - 1];
        temp.button.GetComponentInChildren<Text>().text = temp.filename;
        temp.button.name = "btn" + temp.filename;
        temp.button.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset, 100 - 20 * movCount);
        temp.button.GetComponent<Button>().onClick.AddListener(delegate { btnClick(temp); });
        temp.button.GetComponent<Button>().onClick.AddListener(delegate { imgldr.LoadImageToUI(); });
        foundFiles.Add(temp);
    }
    private void _initFileExplorer()
    {
        string sourceDirectory = @"C:\\Users\\AdamG\\Desktop";
        try
        {
            string[] txtFiles = Directory.GetFiles(sourceDirectory, "*.jpg");

            foreach (string currentFile in txtFiles)
            {
                _initFileButton(currentFile);
                if (movCount == 10)
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
    void btnClick(SelectedFile arg)
    {
        arg.button.gameObject.transform.parent.gameObject.SetActive(false);
        _menuController.ActiveFile = arg;
        selectedFileLabel.text = arg.filename.Replace(".jpg", "");
    }
}
public struct SelectedFile
{
    public GameObject button;
    public string filepath;
    public string filename;
}
