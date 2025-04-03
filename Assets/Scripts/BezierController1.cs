using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.InputActions;
public class BezierController1 : MonoBehaviour
{
    public List<GameObject> bezierPoints;
    bool inEditerState = false;
    List<MeshRenderer> meshRenderers;
    List<LineRenderer> lineRenderers;

    private void Start()
    {
        meshRenderers = new List<MeshRenderer>();
        lineRenderers = new List<LineRenderer>();
        
        foreach (var point in bezierPoints)
        {
            MeshRenderer render;
            LineRenderer lineRenderer;
            if ((render = point.GetComponent<MeshRenderer>())!=null)
            {
                meshRenderers.Add(render);
            }

            if ((lineRenderer = point.GetComponent<LineRenderer>()) != null)
            {
                lineRenderers.Add(lineRenderer);
            }
        }

        TurnOffEdioterState();
    }

    void TurnOffEdioterState()
    {
        inEditerState = false;
        foreach (var mesh in meshRenderers)
        {
            mesh.enabled = false;
        }

        foreach (var line in lineRenderers)
        {
            line.enabled = false;
        }
    }

    void TurnOnEdioterState()
    {
        inEditerState = true;
        
        foreach (var mesh in meshRenderers)
        {
            mesh.enabled = true;
        }

        foreach (var line in lineRenderers)
        {
            line.enabled = true;
        }
    }
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Start))
        {
            if (inEditerState)
            {
                TurnOffEdioterState();
            }
            else
            {
                TurnOnEdioterState();
            }
        }
        
    }
}
