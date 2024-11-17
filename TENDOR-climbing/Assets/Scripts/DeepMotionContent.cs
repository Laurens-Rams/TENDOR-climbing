using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class DeepMotionContent : Content
{
    [SerializeField] private Transform fbx;

    public override void Show()
    {
        gameObject.SetActive(true);
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
