using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Barracuda;
using UnityEngine.Profiling;

[RequireComponent(typeof(OnGUICanvasRelativeDrawer))]
public class WebCamDetector : MonoBehaviour
{
    public NNModel modelFile;
    public TextAsset classesFile;
    public RawImage imageRenderer;

    public float MinBoxConfidence = 0.3f; // threshold to discard bounding boxes
    
    NNHandler nn;
    YOLOHandler yolo;

    WebCamTexture camTexture;
    Texture2D displayingTex;

    TextureScaler textureScaler;

    string[] classesNames;
    OnGUICanvasRelativeDrawer relativeDrawer;

    Color[] colorArray = new Color[] { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow };

    private int frameSkipCount = 10; 
    public static int frameCounter = 0;
    public static int actualFrameCounter = 0;

    void Start()
    {
        var dev = SelectCameraDevice();
        camTexture = new WebCamTexture(dev);
        camTexture.Play();

        nn = new NNHandler(modelFile);
        yolo = new YOLOHandler(nn);

        textureScaler = new TextureScaler(nn.model.inputs[0].shape[1], nn.model.inputs[0].shape[2]);

        relativeDrawer = GetComponent<OnGUICanvasRelativeDrawer>();
        relativeDrawer.relativeObject = imageRenderer.GetComponent<RectTransform>();

        classesNames = classesFile.text.Split(',');
    }

    void Update()
    {
        // frame skip logic
        frameCounter++;
        actualFrameCounter++;

        print("Frame: " + actualFrameCounter);

        if (frameCounter % frameSkipCount == 0)
        {
            // do not skip frame -- perform object detection

            Debug.Log("<-------------------------- LOGS: Update() | WebCamTexture Dimensions Before CaptureAndPrepareTexture: <-------------------------->");
            Debug.Log("############## CamTexture Before calling CaptureAndPrepare #########################");
            Debug.Log("WebCamTexture width before: " + camTexture.width);
            Debug.Log("WebCamTexture height before: " + camTexture.height);
            Debug.Log("<-------------------------- LOGS: Update() | WebCamTexture Dimensions Before CaptureAndPrepareTexture: <-------------------------->");

            Debug.Log("Processing Frame No.: " + actualFrameCounter);

            CaptureAndPrepareTexture(camTexture, ref displayingTex); // WebCamTexture to cropped Texture2D AND scaled Texture2D to 416 X 416 

            Debug.Log("<-------------------------- LOGS: Update() | WebCamTexture Dimensions After CaptureAndPrepareTexture: <-------------------------->");
            Debug.Log("############## CamTexture After calling CaptureAndPrepare #########################");
            Debug.Log("WebCamTexture width after: " + camTexture.width);
            Debug.Log("WebCamTexture height after: " + camTexture.height);
            Debug.Log("<-------------------------- LOGS: Update() | WebCamTexture Dimensions After CaptureAndPrepareTexture: <-------------------------->");

            var boxes = yolo.Run(displayingTex); // gets the YOLO output -- List of ResultBox
            DrawResults(boxes, displayingTex);
            imageRenderer.texture = displayingTex;

            frameCounter = 0; // reset counter 
        }
        else
        {
            Debug.Log("Skipping Frame No.: " + actualFrameCounter);
            // skip frame -- do nothing
        }
    }

    private void OnDestroy()
    {
        nn.Dispose();
        yolo.Dispose();
        textureScaler.Dispose();

        camTexture.Stop();
    }

    private void CaptureAndPrepareTexture(WebCamTexture camTexture, ref Texture2D tex)
    {
        Profiler.BeginSample("Texture processing");
        TextureCropTools.CropToSquare(camTexture, ref tex); // cropped to satisfy YOLO requirement

        Debug.Log("<-------------------------- LOGS: CaptureAndPrepareTexture() | Texture2D Dimensions, Format Before CaptureAndPrepareTexture: <-------------------------->");
        Debug.Log("############## Texture2D Before Scaling #########################");
        Debug.Log("Texture2D width before: " + displayingTex.width);
        Debug.Log("Texture2D height before: " + displayingTex.height);
        Debug.Log("Texture2D format: " + displayingTex.graphicsFormat.ToString());
        Debug.Log("<-------------------------- LOGS: CaptureAndPrepareTexture() | Texture2D Dimensions, Format Before CaptureAndPrepareTexture: <-------------------------->");

        textureScaler.Scale(tex); // scale displayingTexture to 416X416 

        Debug.Log("<-------------------------- LOGS: CaptureAndPrepareTexture() | Texture2D Dimensions, Format After CaptureAndPrepareTexture: <-------------------------->");
        Debug.Log("############## Texture2D After Scaling #########################");
        Debug.Log("Texture2D width after: " + displayingTex.width);
        Debug.Log("Texture2D height after: " + displayingTex.height);
        Debug.Log("Texture2D format: " + displayingTex.graphicsFormat.ToString());
        Debug.Log("<-------------------------- LOGS: CaptureAndPrepareTexture() | Texture2D Dimensions, Format After CaptureAndPrepareTexture: <-------------------------->");

        Profiler.EndSample();
    }

    private void DrawResults(IEnumerable<YOLOHandler.ResultBox> results, Texture2D img)
    {
        relativeDrawer.Clear();
        results.ForEach(box => DrawBox(box, displayingTex));
    }

    private void DrawBox(YOLOHandler.ResultBox box, Texture2D img)
    {
        if (box.classes[box.bestClassIdx] < MinBoxConfidence)
            return;

        TextureDrawingUtils.DrawRect(img, box.rect, colorArray[box.bestClassIdx % colorArray.Length],
                                    (int)(box.classes[box.bestClassIdx] / MinBoxConfidence), true, true);
        relativeDrawer.DrawLabel(classesNames[box.bestClassIdx], box.rect.position);
    }

    string SelectCameraDevice()
    {
        if (WebCamTexture.devices.Length == 0)
            throw new Exception("No camera available.");

        foreach (var cam in WebCamTexture.devices)
        {
            if (!cam.isFrontFacing)
                return cam.name;
        }
        return WebCamTexture.devices[0].name;
    }
}
