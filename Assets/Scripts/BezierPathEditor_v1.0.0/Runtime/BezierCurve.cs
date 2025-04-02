using System;
using System.Collections.Generic;
using UnityEngine;

namespace BezierCurvePath
{
    [Serializable]
    public class BezierCurve
    {
        [Serializable]
        internal struct BezierPoint : IComparable<BezierPoint>
        {
            [SerializeField] internal Vector3 position;
            [SerializeField] internal Vector3 tangent;
            [SerializeField] internal int index;

            public int CompareTo(BezierPoint other)
            {
                if (index > other.index) return 1;
                else
                    return -1;
            }
        }
        [SerializeField] internal List<BezierPoint> points = new List<BezierPoint>();
        [SerializeField] internal bool loop;
        internal void AddPoint(Vector3 pointPos)
        {
            if (points == null)
                points = new List<BezierPoint>();
            var point = new BezierPoint() { position = pointPos, tangent = Vector3.right * 2f, index = points.Count };
            points.Add(point);
        }

        internal void RemovePoint(int orderIndex)
        {
            if (points != null && orderIndex < points.Count && points.Count > 0)
            {
                if (points.Count == 1)
                    points.Clear();
                else if (orderIndex == points.Count - 1)
                {
                    points.RemoveAt(orderIndex);
                }
                else
                {
                    points.RemoveAt(orderIndex);
                    for (int i = orderIndex; i < points.Count; i++)
                    {
                        points[i] = new BezierPoint() { position = points[i].position, tangent = points[i].tangent, index = points[i].index - 1 };
                    }
                }
                points.Sort();
            }
        }
        internal Vector3 EvaluatePosition(float t)
        {
            Vector3 retVal = Vector3.zero;
            if (points.Count > 0)
            {
                float max = points.Count - 1 < 1 ? 0 : (loop ? points.Count : points.Count - 1);
                float standardized = (loop && max > 0) ? ((t %= max) + (t < 0 ? max : 0)) : Mathf.Clamp(t, 0, max);
                int rounded = Mathf.RoundToInt(standardized);
                int i1, i2;
                if (Mathf.Abs(standardized - rounded) < Mathf.Epsilon)
                    i1 = i2 = (rounded == points.Count) ? 0 : rounded;
                else
                {
                    i1 = Mathf.FloorToInt(standardized);
                    if (i1 >= points.Count)
                    {
                        standardized -= max;
                        i1 = 0;
                    }
                    i2 = Mathf.CeilToInt(standardized);
                    i2 = i2 >= points.Count ? 0 : i2;
                }
                retVal = i1 == i2 ? points[i1].position : Three_Bezier(points[i1].position,
                    points[i1].position + points[i1].tangent, points[i2].position
                    - points[i2].tangent, points[i2].position, standardized - i1);
            }
            return retVal;
        }

        /// <summary>
        /// 三阶贝塞尔曲线
        /// </summary>
        /// <param name="startPos">起点</param>
        /// <param name="point1Pos">调整点1</param>
        /// <param name="point2Pos">调整点2</param>
        /// <param name="endPos">终点</param>
        /// <param name="t">插值进度</param>
        /// <returns></returns>
        internal Vector3 Three_Bezier(Vector3 startPos, Vector3 point1Pos, Vector3 point2Pos, Vector3 endPos, float t)
        {
            Vector3 p0p1 = (1 - t) * startPos + t * point1Pos;
            Vector3 p1p2 = (1 - t) * point1Pos + t * point2Pos;
            Vector3 p2p3 = (1 - t) * point2Pos + t * endPos;
            Vector3 p0p1p2 = (1 - t) * p0p1 + t * p1p2;
            Vector3 p1p2p3 = (1 - t) * p1p2 + t * p2p3;
            return (1 - t) * p0p1p2 + t * p1p2p3;
        }
    }
}
