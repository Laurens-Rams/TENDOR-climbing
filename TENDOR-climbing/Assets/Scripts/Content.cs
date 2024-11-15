using UnityEngine;
using UnityEngine.XR.ARFoundation;

public abstract class Content : MonoBehaviour
{
    public abstract void Show();
    public abstract void Hide();
    public abstract void Play();
    public abstract void Pause();
}
