using UnityEngine;

public class ModeSwitcher : MonoBehaviour
{
    [SerializeField] private SessionRecorder recorder;
    [SerializeField] private PlaybackManager playback;
    [SerializeField] private GameObject recordUI;
    [SerializeField] private GameObject playbackUI;

    bool recording = false;

    public void ToggleMode()
    {
        if (!recording)
        {
            recorder.StartRecording();
            recordUI.SetActive(true);
            playbackUI.SetActive(false);
        }
        else
        {
            recorder.StopRecording();
            recordUI.SetActive(false);
            playbackUI.SetActive(true);
        }
        recording = !recording;
    }

    public void PlayLatest()
    {
        var folder = Application.persistentDataPath + "/Captures";
        var files = System.IO.Directory.GetFiles(folder, "*.body");
        if (files.Length > 0)
            playback.Load(files[files.Length - 1]);
    }
}
