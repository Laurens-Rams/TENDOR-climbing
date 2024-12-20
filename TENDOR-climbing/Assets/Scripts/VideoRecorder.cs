using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.VideoioModule;

public class VideoRecorder : MonoBehaviour
{
    [SerializeField]
    private string path = "/video.avi";

    public Texture2D tex;
    public Mat mat;

    private XRCpuImage cpuImage;
    private VideoWriter writer;

    void OnEnable()
    {
        Init();
        Globals.CameraManager.frameReceived += RecordFrame;
    }

    void OnDisable()
    {
        Globals.CameraManager.frameReceived -= RecordFrame;

        writer.release();
        cpuImage.Dispose();
        mat.Dispose();
    }

    private void Init()
    {
        int h = (int)Globals.CameraManager.currentConfiguration?.height;
        int w = (int)Globals.CameraManager.currentConfiguration?.width;

        tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        mat = new Mat(h, w, CvType.CV_8UC3);

        writer = new VideoWriter();
        writer.open(Application.persistentDataPath + path, Videoio.CAP_OPENCV_MJPEG, VideoWriter.fourcc('M', 'J', 'P', 'G'), 30, new Size(w, h));
    }

    private void RecordFrame(ARCameraFrameEventArgs args)
    {
        if (!Globals.CameraManager.TryAcquireLatestCpuImage(out cpuImage))
            return;

        var conversionParams = new XRCpuImage.ConversionParams(cpuImage, TextureFormat.RGB24, XRCpuImage.Transformation.MirrorY);
        var data = tex.GetRawTextureData<byte>();
        cpuImage.Convert(conversionParams, data);

        tex.Apply();
        Utils.texture2DToMat(tex, mat);
        writer.write(mat);
        cpuImage.Dispose();
    }
}
