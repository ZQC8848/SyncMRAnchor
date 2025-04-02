using UnityEngine;
using BezierCurvePath;

public class SimpleBezierAlonger : MonoBehaviour
{
    [SerializeField] private SimpleBezierCurvePath path;

    [SerializeField] public float speed = 0.1f;


    internal float normalized = 0f;

    internal Vector3 lastPosition;
    private void Update()
    {
        //transform.MoveAlongPath(path, speed, ref normalized);
        transform.MoveAlongPath_AlongDir(path, speed, ref lastPosition, ref normalized);

    }
}
