using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class HumanoidRetarget : MonoBehaviour
{
    public float scale = 1f;

    public void Retarget(ARHumanBody body, Animator animator)
    {
        if (body == null || animator == null)
            return;

        Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        if (hips)
        {
            hips.position = body.transform.position;
            hips.rotation = body.transform.rotation;
        }
    }

    public void UpdateScale(Transform sourceHips, Transform sourceHead, Transform targetHips, Transform targetHead)
    {
        if (!sourceHips || !sourceHead || !targetHips || !targetHead)
            return;
        float src = Vector3.Distance(sourceHips.position, sourceHead.position);
        float dst = Vector3.Distance(targetHips.position, targetHead.position);
        if (dst > 0f)
            scale = src / dst;
    }
}
