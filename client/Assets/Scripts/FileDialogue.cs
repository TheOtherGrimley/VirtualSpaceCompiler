using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FileDialogue : MonoBehaviour {
    public Transform buttonParent;
    public GameObject filePrefab;
    public GameObject directoryPrefab;
    public GameObject parentDirectoryPrefab;
    public Text CurrentDirectory;
    public Text selectedFileLabel;
    public Text API_URL;
    public GameObject loadingCube;
    public int maxStringLength = 15;
    public int xOffsetDifference = 150;

    private string sourceDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
    private ImageLoader imgldr; // Used to refer to the image loader in the menu scripts
    private Menu _menuController;
    List<SelectedFile> foundFiles = new List<SelectedFile>();

    // Counter variables used to format the custom file explorer
    int movCount;
    int xOffset;

    

    private void Start()
    {
        _menuController = this.GetComponent<Menu>();
        
        imgldr = GameObject.FindGameObjectWithTag("MenuCtrl").GetComponent<ImageLoader>();
        _initFileExplorer();
    }

    private void _initFileButton(string file)
    {
        SelectedFile temp = new SelectedFile();
        temp.button = Instantiate(filePrefab, buttonParent);
        temp.filepath = file;
        temp.filename = file.Split('\\')[file.Split('\\').Length - 1];
        if(temp.filename.Length > 20)
            temp.button.GetComponentInChildren<Text>().text = temp.filename.Substring(0, maxStringLength);
        else
            temp.button.GetComponentInChildren<Text>().text = temp.filename;
        temp.button.name = "btn" + temp.filename;
        temp.button.GetComponent<Button>().onClick.AddListener(delegate { btnClick(temp); });
        temp.button.GetComponent<Button>().onClick.AddListener(delegate { imgldr.LoadImageToUI(); });
        foundFiles.Add(temp);
    }

    private void _initDirButton(string file)
    {
        GameObject g = Instantiate(directoryPrefab, buttonParent);
        string folderName = file.Split('\\')[file.Split('\\').Length - 1];
        if (folderName.Length > 20)
            g.GetComponentInChildren<Text>().text = folderName.Substring(0, maxStringLength);
        else
            g.GetComponentInChildren<Text>().text = folderName;
        g.name = "btn" + folderName;
        g.GetComponent<Button>().onClick.AddListener(delegate { _goTo(folderName); });
    }

    private void _initParentDirectoryButton()
    {
        GameObject g = Instantiate(parentDirectoryPrefab, buttonParent);
        g.GetComponent<Button>().onClick.AddListener(delegate { _goUp(); });
        movCount += 1; // Will always be the first loaded
    }

    private void _goTo(string folder)
    {
        sourceDirectory += "\\" + folder;
        _initFileExplorer();
    }

    private void _goUp()
    {
        sourceDirectory = Directory.GetParent(sourceDirectory).FullName;
        _initFileExplorer();
    }

    private void _resetVars()
    {
        for(int i = buttonParent.childCount-1; i > -1; i--)
            if (buttonParent.GetChild(i).gameObject.tag == "UI")
                Destroy(buttonParent.GetChild(i).gameObject);
        foundFiles.Clear();
        movCount = 0;
        xOffset = -350;
    }

    private void loadDirectories()
    {
        string[] directories = Directory.GetDirectories(sourceDirectory);
        foreach (string currentDir in directories)
        {
            _initDirButton(currentDir);
            if (movCount == 10)
            {
                movCount = 0;
                xOffset += xOffsetDifference;
            }
            movCount += 1;
        }
    }

    private void loadFiles()
    {
        string[] txtFiles = Directory.GetFiles(sourceDirectory, "*.jpg");

        foreach (string currentFile in txtFiles)
        {
            _initFileButton(currentFile);
            if (movCount == 10)
            {
                movCount = 0;
                xOffset += xOffsetDifference;
            }
            movCount += 1;
        }
    }

    private void _initFileExplorer()
    {
        try
        {
            CurrentDirectory.text = sourceDirectory;
            _resetVars();
            _initParentDirectoryButton();
            loadDirectories();
            loadFiles();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    void btnClick(SelectedFile arg)
    {
        arg.button.gameObject.transform.parent.parent.parent.parent.gameObject.SetActive(false);
        var fileInfo = new System.IO.FileInfo(arg.filepath);
        Metrics.Instance.ImageSize = (fileInfo.Length / 1e+6f); // Give size in mb
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
