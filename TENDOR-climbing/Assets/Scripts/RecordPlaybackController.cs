using UnityEngine;

public class RecordPlaybackController : MonoBehaviour
{
    [SerializeField] private SessionRecorder recorder;
    [SerializeField] private LatestPlaybackManager playback;

    private bool recording = false;
    private bool playing = false;

    public bool IsRecording => recording;
    public bool IsPlaying => playing;

    void Start()
    {
        if (playback)
            playback.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleRecording();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            TogglePlayback();
        }
    }

    public void StartRecording()
    {
        if (!recorder)
            return;
        if (playback)
        {
            playback.Clear();
            playback.gameObject.SetActive(false);
            playing = false;
        }
        recorder.StartRecording();
        recording = true;
    }

    public void StopRecording()
    {
        if (!recorder)
            return;
        recorder.StopRecording();
        recording = false;
    }

    public void StartPlayback()
    {
        if (!playback)
            return;
        if (recording)
            StopRecording();
        playback.gameObject.SetActive(true);
        playback.LoadLatest();
        playing = true;
    }

    public void ToggleRecording()
    {
        if (recording)
            StopRecording();
        else
            StartRecording();
    }

    public void TogglePlayback()
    {
        if (playing)
            StopPlayback();
        else
            StartPlayback();
    }

    public void StopPlayback()
    {
        if (!playback)
            return;
        playback.Clear();
        playback.gameObject.SetActive(false);
        playing = false;
    }
}
