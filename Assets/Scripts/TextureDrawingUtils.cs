﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public static class TextureDrawingUtils
{
    /// <summary>
    /// Draw rectange outline on texture
    /// </summary>
    /// <param name="width">Width of outline</param>
    /// <param name="rectIsNormalized">Are rect values normalized?</param>
    /// <param name="revertY">Pass true if y axis has opposite direction than texture axis</param>
    /// 

    public static void DrawRect(Texture2D tex, Rect rect, Color color, int width = 1, bool rectIsNormalized = true, bool revertY = false)
    {

        Debug.Log("Drawing bounding box for Frame No.: " + WebCamDetector.actualFrameCounter);
        
        Debug.Log("<-------------------------- LOGS: DrawRect() | Texture2D Dimensions, Rect Dimensions Before: <-------------------------->");

        Debug.Log("#################### Texture dimensions: ####################");
        Debug.Log("Tex width: " + tex.width);
        Debug.Log("Tex height: " + tex.height);

        Debug.Log("#################### Rect dimensions before: ####################");
        Debug.Log("Rect x: " + rect.x);
        Debug.Log("Rect y: " + rect.y);
        Debug.Log("Rect width: " + rect.width);
        Debug.Log("Rect height: " + rect.height);

        Debug.Log("<-------------------------- LOGS: DrawRect() | Texture2D Dimensions, Rect Dimensions Before: <-------------------------->");

        if (rectIsNormalized)
        {
            rect.x *= tex.width;
            rect.y *= tex.height;
            rect.width *= tex.width;
            rect.height *= tex.height;
        }

        // rect coordinates are scaled wrt 416X416 texture
        // can we enter our coordinate extraction logic here? 

        Debug.Log("<-------------------------- LOGS: DrawRect() | Texture2D Dimensions, Rect Dimensions After: <-------------------------->");
        Debug.Log("#################### Rect dimensions after: ####################");
        Debug.Log("Rect x: " + rect.x);
        Debug.Log("Rect y: " + rect.y);
        Debug.Log("Rect width: " + rect.width);
        Debug.Log("Rect height: " + rect.height);
        Debug.Log("<-------------------------- LOGS: DrawRect() | Texture2D Dimensions, Rect Dimensions After: <-------------------------->");

        if (revertY)
            rect.y = rect.y * -1 + tex.height - rect.height;

        if (rect.width <= 0 || rect.height <= 0)
            return;

        _draw_line(rect.x, rect.y, rect.width + width, width, color, tex);
        _draw_line(rect.x, rect.y + rect.height, rect.width + width, width, color, tex);

        _draw_line(rect.x, rect.y, width, rect.height + width, color, tex);
        _draw_line(rect.x + rect.width, rect.y, width, rect.height + width, color, tex);
        tex.Apply();
    }

    static void _draw_line(float x, float y, float width, float height, Color col, Texture2D tex)
    {
        if (x > tex.width
            || y > tex.height)
            return;

        if (x < 0)
        {
            width += x;
            x = 0;
        }
        if (y < 0)
        {
            height += y;
            y = 0;
        }

        if (width < 0 || height < 0)
            return;

        width = x + width > tex.width ? tex.width - x : width;
        height = y + height > tex.height ? tex.height - y : height;

        int len = (int)width * (int)height;
        Color[] c = new Color[len];
        for (int i = 0; i < len; i++)
            c[i] = col;

        tex.SetPixels((int)x, (int)y, (int)width, (int)height, c);
    }


}
