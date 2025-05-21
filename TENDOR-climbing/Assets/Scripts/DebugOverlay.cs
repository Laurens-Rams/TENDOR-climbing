using UnityEngine;

public class DebugOverlay : MonoBehaviour
{
    [SerializeField] private GameObject skeleton;
    [SerializeField] private GameObject avatar;

    public void SetVisible(bool visible)
    {
        if (skeleton) skeleton.SetActive(visible);
        if (avatar) avatar.SetActive(visible);
    }
}
