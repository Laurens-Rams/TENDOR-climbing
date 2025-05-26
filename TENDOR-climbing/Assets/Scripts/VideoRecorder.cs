using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;
using RenderHeads.Media.AVProMovieCapture;
using System;
using System.Collections;

public class VideoRecorder : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI tmpFrames;

    [SerializeField]
    private RawImage image;

    [SerializeField]
    private CaptureFromTexture capture;

    public Texture2D tex;

    //private string fullPath;
    private XRCpuImage cpuImage;

    IEnumerator Start()
    {
#if (UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_EDITOR
    CaptureBase.PhotoLibraryAccessLevel photoLibraryAccessLevel = CaptureBase.PhotoLibraryAccessLevel.AddOnly;

    // If we're trying to write to the photo library, make sure we have permission
    if (capture.OutputFolder == CaptureBase.OutputPath.PhotoLibrary)
    {
        // Album creation (album name is taken from the output folder path) requires read write access.
        if (capture.OutputFolderPath != null && capture.OutputFolderPath.Length > 0)
            photoLibraryAccessLevel = CaptureBase.PhotoLibraryAccessLevel.ReadWrite;

        switch (CaptureBase.HasUserAuthorisationToAccessPhotos(photoLibraryAccessLevel))
        {
            case CaptureBase.PhotoLibraryAuthorisationStatus.Authorised:
                // All good, nothing to do
                break;

            case CaptureBase.PhotoLibraryAuthorisationStatus.Unavailable:
               UnityEngine.Debug.LogWarning("The photo library is unavailable, will use RelativeToPersistentData instead");
                capture.OutputFolder = CaptureBase.OutputPath.RelativeToPersistentData;
                break;

            case CaptureBase.PhotoLibraryAuthorisationStatus.Denied:
                // User has denied access, change output path
               UnityEngine.Debug.LogWarning("User has denied access to the photo library, will use RelativeToPersistentData instead");
                capture.OutputFolder = CaptureBase.OutputPath.RelativeToPersistentData;
                break;

            case CaptureBase.PhotoLibraryAuthorisationStatus.NotDetermined:
                // Need to ask permission
                yield return CaptureBase.RequestUserAuthorisationToAccessPhotos(photoLibraryAccessLevel);
                // Nested switch, everbodies favourite
                switch (CaptureBase.HasUserAuthorisationToAccessPhotos(photoLibraryAccessLevel))
                {
                    case CaptureBase.PhotoLibraryAuthorisationStatus.Authorised:
                        // All good, nothing to do
                        break;

                    case CaptureBase.PhotoLibraryAuthorisationStatus.Denied:
                        // User has denied access, change output path
                       UnityEngine.Debug.LogWarning("User has denied access to the photo library, will use RelativeToPersistentData instead");
                        capture.OutputFolder = CaptureBase.OutputPath.RelativeToPersistentData;
                        break;

                    case CaptureBase.PhotoLibraryAuthorisationStatus.NotDetermined:
                        // We were unable to request access for some reason, check the logs for any error information
                       UnityEngine.Debug.LogWarning("Authorisation to access the photo library is still undetermined, will use RelativeToPersistentData instead");
                        capture.OutputFolder = CaptureBase.OutputPath.RelativeToPersistentData;
                        break;
                }
                break;
        }
    }
#endif
        yield return null;
    }

    public void StartRecording()
    {
        tex = new Texture2D((int)Globals.CameraManager.currentConfiguration?.width, (int)Globals.CameraManager.currentConfiguration?.height, TextureFormat.RGBA32, false);
        image.texture = tex;
        capture.SetSourceTexture(tex);
        capture.StartCapture();

        tmpFrames.text = "0";
        Globals.CameraManager.frameReceived += RecordFrame;
    }

    public void StopRecording()
    {
        Globals.CameraManager.frameReceived -= RecordFrame;
        capture.StopCapture();

        //byte[] bytes = File.ReadAllBytes(fullPath);
        //Globals.FileUploader.StartUpload(bytes);
    }

    private void RecordFrame(ARCameraFrameEventArgs args)
    {
        if (!Globals.CameraManager.TryAcquireLatestCpuImage(out cpuImage))
            return;

        var conversionParams = new XRCpuImage.ConversionParams(cpuImage, TextureFormat.RGBA32, XRCpuImage.Transformation.MirrorY);
        var data = tex.GetRawTextureData<byte>();
        cpuImage.Convert(conversionParams, data);

        tex.Apply();
        capture.UpdateSourceTexture();
        capture.UpdateFrame();
        cpuImage.Dispose();

        int frames = Int32.Parse(tmpFrames.text);
        tmpFrames.text = (frames + 1).ToString();
    }
}
