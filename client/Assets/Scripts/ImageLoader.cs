﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImageLoader : MonoBehaviour {
    public Image img;

	public void LoadImageToUI()
    {
        if (!img.gameObject.activeInHierarchy)
            img.gameObject.SetActive(true);
        img.sprite = _loadPngFile();
        
    }

    private Sprite _loadPngFile()
    {
        Texture2D tex = null;
        byte[] fileData;

        fileData = File.ReadAllBytes(this.GetComponent<Menu>().ActiveFile.filepath);
        tex = new Texture2D(2, 2);
        tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.0f), 1.0f);

        return sprite;
    }
}
