using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class DeepMotionContent : Content
{
    [SerializeField] protected Transform fbx;
    [SerializeField] protected Transform image;

    public override void Show(ARTrackedImage trackedImage)
    {
       // image.localScale = new Vector3(trackedImage.size.x, trackedImage.size.y, 1f);
        gameObject.SetActive(true);
        Globals.UI.SetActive(true);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
    }

    public override void Play()
    {
    }

    public override void Pause()
    {
    }
}
