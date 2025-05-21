using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class BodyTracker : MonoBehaviour
{
    public ARHumanBody Current { get; private set; }

    void OnEnable()
    {
        var manager = Globals.BodyManager;
        if (manager != null)
            manager.humanBodiesChanged += OnBodiesChanged;
    }

    void OnDisable()
    {
        var manager = Globals.BodyManager;
        if (manager != null)
            manager.humanBodiesChanged -= OnBodiesChanged;
    }

    void OnBodiesChanged(ARHumanBodiesChangedEventArgs args)
    {
        if (args.added.Count > 0)
            Current = args.added[0];
        else if (args.updated.Count > 0)
            Current = args.updated[0];
    }
}
