using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Transform controllerPoint;
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        controllerPoint = transform.GetChild(1);
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(0, controllerPoint.position);
        lineRenderer.SetPosition(1, transform.position);
    }
}
