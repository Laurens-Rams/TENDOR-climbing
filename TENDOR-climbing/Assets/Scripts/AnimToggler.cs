using UnityEngine;

public class AnimToggler : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] string animName;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
        Restart();
    }

    public void Toggle()
    {
        animator.speed = 1f - animator.speed;
    }

    public void Restart()
    {
        transform.localPosition = startPos;
        transform.localRotation = startRot;

        animator.Play(animName, -1, 0f);
        animator.speed = 0f;
    }
}
