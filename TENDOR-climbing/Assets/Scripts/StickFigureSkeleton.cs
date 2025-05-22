using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARHumanBodyManager))]
public class StickFigureSkeleton : MonoBehaviour
{
    [SerializeField] private LineRenderer linePrefab;

    private ARHumanBodyManager bodyManager;
    private readonly List<LineRenderer> lines = new List<LineRenderer>();

    // Pairs of human body bones to connect with line renderers
    private readonly HumanBodyBones[] bonesA = new HumanBodyBones[]
    {
        HumanBodyBones.Hips,
        HumanBodyBones.Spine,
        HumanBodyBones.Chest,
        HumanBodyBones.Chest,
        HumanBodyBones.LeftUpperLeg,
        HumanBodyBones.LeftLowerLeg,
        HumanBodyBones.RightUpperLeg,
        HumanBodyBones.RightLowerLeg,
        HumanBodyBones.LeftUpperArm,
        HumanBodyBones.LeftLowerArm,
        HumanBodyBones.RightUpperArm,
        HumanBodyBones.RightLowerArm
    };
    private readonly HumanBodyBones[] bonesB = new HumanBodyBones[]
    {
        HumanBodyBones.Spine,
        HumanBodyBones.Chest,
        HumanBodyBones.Neck,
        HumanBodyBones.Head,
        HumanBodyBones.LeftLowerLeg,
        HumanBodyBones.LeftFoot,
        HumanBodyBones.RightLowerLeg,
        HumanBodyBones.RightFoot,
        HumanBodyBones.LeftLowerArm,
        HumanBodyBones.LeftHand,
        HumanBodyBones.RightLowerArm,
        HumanBodyBones.RightHand
    };

    void Awake()
    {
        bodyManager = GetComponent<ARHumanBodyManager>();
    }

    void OnEnable()
    {
        bodyManager.humanBodiesChanged += OnBodiesChanged;
    }

    void OnDisable()
    {
        bodyManager.humanBodiesChanged -= OnBodiesChanged;
        foreach (var line in lines)
            if (line)
                Destroy(line.gameObject);
        lines.Clear();
    }

    private void EnsureLines()
    {
        while (lines.Count < bonesA.Length)
        {
            var lr = Instantiate(linePrefab, transform);
            lr.positionCount = 2;
            lines.Add(lr);
        }
    }

    private void OnBodiesChanged(ARHumanBodiesChangedEventArgs args)
    {
        if (args.updated.Count == 0) return;
        var body = args.updated[0];
        var animator = body.GetComponent<Animator>();
        if (!animator) return;
        EnsureLines();
        for (int i = 0; i < bonesA.Length; i++)
        {
            var a = animator.GetBoneTransform(bonesA[i]);
            var b = animator.GetBoneTransform(bonesB[i]);
            if (a && b)
            {
                var lr = lines[i];
                lr.SetPosition(0, a.position);
                lr.SetPosition(1, b.position);
            }
        }
    }
}
