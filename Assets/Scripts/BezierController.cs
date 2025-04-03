using System;
using System.Collections;
using System.Collections.Generic;
using BezierCurvePath;
using UnityEngine;

public class BezierController : MonoBehaviour
{
    SimpleBezierCurvePath _path;
    private float timer = 0f;
    private float changeBeizierTime = 1f;
    
    
    [System.Serializable]
    public class BezierPoint
    {
        public Transform originalPoint;
        [HideInInspector]
        public Transform controllerPoint;
    }
    
    public List<BezierPoint> newBezierPoints = new List<BezierPoint>();

    private void Start()
    {
        _path = GetComponent<SimpleBezierCurvePath>();
        
        foreach (var point in newBezierPoints)
        {
            point.controllerPoint = point.originalPoint.GetChild(1);
        }
        
        _path.bezierCurve.points.Clear();
        for (int i = 0; i < newBezierPoints.Count; i++)
        {
            _path.bezierCurve.AddPoint(newBezierPoints[i].originalPoint.position, newBezierPoints[i].controllerPoint.position);
            //_path.bezierCurve.AddPoint(newBezierPoints[i].originalPoint.position);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < changeBeizierTime) return;
        timer = 0f;
        ChanegBerzierPoints();
    }

    void ChanegBerzierPoints()
    {
        for (int i = 0; i < newBezierPoints.Count; i++)
        {
            _path.bezierCurve.ChangePoint(i, newBezierPoints[i].originalPoint.position, newBezierPoints[i].controllerPoint.position);
        }
    }
}
