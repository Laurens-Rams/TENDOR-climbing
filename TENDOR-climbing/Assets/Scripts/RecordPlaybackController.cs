using UnityEngine;

public class RecordPlaybackController : MonoBehaviour
{
    [SerializeField] private SessionRecorder recorder;
    [SerializeField] private LatestPlaybackManager playback;

    private bool recording = false;
    private bool playing = false;

    void Start()
    {
        if (playback)
            playback.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (recording)
                StopRecording();
            else
                StartRecording();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (playing)
                StopPlayback();
            else
                StartPlayback();
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

    public void StopPlayback()
    {
        if (!playback)
            return;
        playback.Clear();
        playback.gameObject.SetActive(false);
        playing = false;
    }
}
