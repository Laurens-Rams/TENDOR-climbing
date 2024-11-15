using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class DeepMotionContent : Content
{
    [SerializeField] private Transform fbx;

    public override void Show()
    {
        fbx?.gameObject.SetActive(true);
    }

    public override void Hide()
    {
        fbx?.gameObject.SetActive(false);
    }

    public override void Play()
    {
    }

    public override void Pause()
    {
    }
}
