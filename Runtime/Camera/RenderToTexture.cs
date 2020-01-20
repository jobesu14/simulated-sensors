using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class RenderToTexture : MonoBehaviour
{
    public SimulationSettings simulationSettings;
    public Camera renderCamera;

    private string outputFolder;

    void Awake()
    {
        string folder = "camera_" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");
        outputFolder = Path.Combine(simulationSettings.dataOutputDirectory, folder);
        DirectoryInfo di = Directory.CreateDirectory(outputFolder);
    }

    private void Start()
    {
        Application.targetFrameRate = simulationSettings.targetCameraFps;
    }

    void Update()
    {
        Stopwatch sw = new Stopwatch();

        sw.Start();
        
        ulong timestamp = (ulong)(Time.time * 1e9);

        string imagePath = Path.Combine(simulationSettings.dataOutputDirectory, outputFolder, "" + timestamp + ".png");
        ScreenCapture.CaptureScreenshot(imagePath);

        sw.Stop();
        UnityEngine.Debug.Log("image timestamps: " + timestamp/1e6);
        UnityEngine.Debug.Log("dt save image precise: " + (sw.Elapsed));

        //Texture2D image = RTImage(renderCamera);
        /*Task<bool> success = WriteImageThreaded(timestamp, image);

        /*if(!success)
        {
            // TODO stop simulation
        }*/
    }

    // Take a "screenshot" of a camera's Render Texture.
    Texture2D RTImage(Camera camera)
    {
        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;

        // Render the camera's view.
        camera.Render();

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        image.Apply();

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;
        return image;
    }

    private async /*Task<bool>*/ void WriteImageThreaded(ulong imageTimestamp, Texture2D image)
    {
        //try
        {
            byte[] bytes = image.EncodeToPNG();
            string imagePath = Path.Combine(simulationSettings.dataOutputDirectory, outputFolder, "" + imageTimestamp + ".png");
            using (FileStream SourceStream = File.Open(imagePath, FileMode.OpenOrCreate))
            {
                SourceStream.Seek(0, SeekOrigin.End);
                await SourceStream.WriteAsync(bytes, 0, bytes.Length);
                //return true;
            }
        }
        /*catch
        {
            return false;
        }*/
    }
}