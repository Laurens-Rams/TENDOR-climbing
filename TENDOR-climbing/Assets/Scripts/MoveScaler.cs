using UnityEngine;

public class MoveScaler : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float rangeX = 1f;
    [SerializeField] private float rangeY = 1f;
    [SerializeField] private float rangeZ = 1f;
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 4f;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnValueXChanged(float value)
    {
        if (!target)
            return;

        var x = (-1f + 2f * value) * rangeX;
        target.localPosition = new Vector3(x, target.localPosition.y, target.localPosition.z);
    }

    public void OnValueYChanged(float value)
    {
        if (!target)
            return;

        var y = (-2f + 2f * value) * rangeY;
        target.localPosition = new Vector3(target.localPosition.x, y, target.localPosition.z);
    }

    public void OnValueZChanged(float value)
    {
        if (!target)
            return;

        var z = (-1f + 2f * value) * rangeZ;
        target.localPosition = new Vector3(target.localPosition.x, target.localPosition.y, z);
    }

    public void OnValueScaleChanged(float value)
    {
        if (!target)
            return;

        var range = maxScale - minScale;
        var scale = minScale + value * range;
        target.localScale = new Vector3(scale, scale, scale);
    }
}
