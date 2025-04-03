using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLine : MonoBehaviour
{
    private LineRenderer firstLineRenderer;
    private LineRenderer secondLineRenderer;
    private Transform controllerPoint;
    public Transform nextPosition;
    void Start()
    {
        firstLineRenderer = GetComponent<LineRenderer>();
        secondLineRenderer = transform.GetChild(1).GetComponent<LineRenderer>();
        controllerPoint = transform.GetChild(1);
    }

    // Update is called once per frame
    void Update()
    {
        firstLineRenderer.SetPosition(0, controllerPoint.position);
        firstLineRenderer.SetPosition(1, transform.position);
        
        secondLineRenderer.SetPosition(0, controllerPoint.position);
        secondLineRenderer.SetPosition(1, nextPosition.position);
    }
}
