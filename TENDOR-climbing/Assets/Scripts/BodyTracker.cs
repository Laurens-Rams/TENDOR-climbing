using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class BodyTracker : MonoBehaviour
{
    private ARHumanBody currentBody;
    public ARHumanBody Current => currentBody;

    private ARHumanBodyManager manager;

    void OnEnable()
    {
        manager = GetComponent<ARHumanBodyManager>();
        if (manager != null)
            manager.humanBodiesChanged += OnBodiesChanged;
    }

    void OnDisable()
    {
        if (manager != null)
            manager.humanBodiesChanged -= OnBodiesChanged;
    }

    void OnBodiesChanged(ARHumanBodiesChangedEventArgs args)
    {
        if (args.updated.Count > 0)
            currentBody = args.updated[0];
        else if (args.added.Count > 0)
            currentBody = args.added[0];
    }
}
