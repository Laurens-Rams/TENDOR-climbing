using UnityEngine;

public class RecordPlaybackUI : MonoBehaviour
{
    [SerializeField] private RecordPlaybackController controller;
    [SerializeField] private Vector2 offset = new Vector2(20, 20);
    [SerializeField] private float buttonWidth = 140f;
    [SerializeField] private float buttonHeight = 60f;

    void OnGUI()
    {
        if (!controller)
            return;

        string recordLabel = controller.IsRecording ? "Stop Recording" : "Record";
        if (GUI.Button(new Rect(offset.x, offset.y, buttonWidth, buttonHeight), recordLabel))
        {
            controller.ToggleRecording();
        }

        string playLabel = controller.IsPlaying ? "Stop Playback" : "Playback";
        if (GUI.Button(new Rect(offset.x + buttonWidth + 10f, offset.y, buttonWidth, buttonHeight), playLabel))
        {
            controller.TogglePlayback();
        }
    }
}
