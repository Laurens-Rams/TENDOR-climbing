using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class StickFigureSkeleton : MonoBehaviour
{
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private ARHumanBodyManager bodyManager;

    private readonly List<LineRenderer> lines = new List<LineRenderer>();

    private static readonly (HumanBodyBones, HumanBodyBones)[] bonePairs = new (HumanBodyBones, HumanBodyBones)[]
    {
        (HumanBodyBones.Hips, HumanBodyBones.Spine),
        (HumanBodyBones.Spine, HumanBodyBones.Chest),
        (HumanBodyBones.Chest, HumanBodyBones.Head),
        (HumanBodyBones.Chest, HumanBodyBones.LeftUpperArm),
        (HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm),
        (HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand),
        (HumanBodyBones.Chest, HumanBodyBones.RightUpperArm),
        (HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm),
        (HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand),
        (HumanBodyBones.Hips, HumanBodyBones.LeftUpperLeg),
        (HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg),
        (HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot),
        (HumanBodyBones.Hips, HumanBodyBones.RightUpperLeg),
        (HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg),
        (HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot)
    };

    void Awake()
    {
        if (!bodyManager)
            bodyManager = GetComponent<ARHumanBodyManager>();
    }

    void Update()
    {
        if (bodyManager.trackables.count == 0)
            return;
        var body = bodyManager.trackables[0];
        var animator = body.GetComponent<Animator>();
        if (!animator) return;

        EnsureLines();
        for (int i = 0; i < bonePairs.Length; i++)
        {
            var (a, b) = bonePairs[i];
            var ta = animator.GetBoneTransform(a);
            var tb = animator.GetBoneTransform(b);
            if (!ta || !tb) continue;
            var line = lines[i];
            line.SetPosition(0, ta.position);
            line.SetPosition(1, tb.position);
        }
    }

    private void EnsureLines()
    {
        if (lines.Count == bonePairs.Length)
            return;
        foreach (var line in lines)
            Destroy(line.gameObject);
        lines.Clear();
        for (int i = 0; i < bonePairs.Length; i++)
        {
            var lr = Instantiate(linePrefab, transform);
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.widthMultiplier = 0.01f;
            lines.Add(lr);
        }
    }
}
