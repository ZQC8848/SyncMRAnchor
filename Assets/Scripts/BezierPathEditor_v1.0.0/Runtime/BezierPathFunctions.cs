using UnityEngine;

namespace BezierCurvePath
{
    public static class BezierPathFunctions
    {
        /// <summary>
        /// 仅跟随贝塞尔路径移动
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="path">自定义路径</param>
        /// <param name="speed">速度</param>
        /// <param name="normalized">归一化变量(用于插值计算路径位置)</param>
        public static void MoveAlongPath(this Transform transform, SimpleBezierCurvePath path, float speed, ref float normalized)
        {
            if (path.bezierCurve == null || path.bezierCurve.points == null || path.bezierCurve.points.Count == 0) return;
            float t = normalized + speed * Time.deltaTime;
            float max = path.bezierCurve.points.Count - 1 < 1 ? 0 : (path.bezierCurve.loop ? path.bezierCurve.points.Count : path.bezierCurve.points.Count - 1);
            normalized = (path.bezierCurve.loop && max > 0) ? ((t %= max) + (t < 0 ? max : 0)) : Mathf.Clamp(t, 0, max);
            transform.position = path.bezierCurve.EvaluatePosition(normalized);
        }

        /// <summary>
        /// 跟随贝塞尔路径移动且方向朝向路基方向
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="path">自定义路径</param>
        /// <param name="speed">速度</param>
        /// <param name="lastPosition">上一位置（用于计算方向）</param>
        /// <param name="normalized">归一化变量(用于插值计算路径位置)</param>
        public static void MoveAlongPath_AlongDir(this Transform transform, SimpleBezierCurvePath path, float speed, ref Vector3 lastPosition, ref float normalized)
        {
            if (path.bezierCurve == null || path.bezierCurve.points == null || path.bezierCurve.points.Count == 0) return;
            float t = normalized + speed * Time.deltaTime;
            float max = path.bezierCurve.points.Count - 1 < 1 ? 0 : (path.bezierCurve.loop ? path.bezierCurve.points.Count : path.bezierCurve.points.Count - 1);
            normalized = (path.bezierCurve.loop && max > 0) ? ((t %= max) + (t < 0 ? max : 0)) : Mathf.Clamp(t, 0, max);
            transform.position = path.bezierCurve.EvaluatePosition(normalized);
            Vector3 forward = transform.position - lastPosition;
            transform.forward = forward != Vector3.zero ? forward : transform.forward;
            lastPosition = transform.position;
        }

    }

}
