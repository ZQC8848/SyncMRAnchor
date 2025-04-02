using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace BezierCurvePath
{
    [AddComponentMenu("Bezier/SimpleBezierCurvePath", 0), DisallowMultipleComponent]
    public class SimpleBezierCurvePath : MonoBehaviour
    {
        [SerializeField] public BezierCurve bezierCurve = new BezierCurve();
        
#if UNITY_EDITOR
        internal Color pathColor = Color.green;
        internal bool loop;
        internal bool draw;

        private void OnDrawGizmos()
        {
            if (bezierCurve == null) return;
            if (bezierCurve.points == null) return;
            if (bezierCurve.points.Count == 0) return;
            //缓存颜色
            Color cacheColor = Gizmos.color;
            //路径绘制颜色
            Gizmos.color = pathColor;
            //缓存上个坐标点
            Vector3 lastPos = transform.TransformPoint(bezierCurve.EvaluatePosition(0f));
            float end = (bezierCurve.points.Count - 1 < 1 ? 0 : (bezierCurve.loop ? bezierCurve.points.Count : bezierCurve.points.Count - 1)) + 0.01f * .5f;
            for (float t = 0.01f; t <= end; t += 0.01f)
            {
                //计算位置
                Vector3 p = transform.TransformPoint(bezierCurve.EvaluatePosition(t));
                //绘制曲线
                Gizmos.DrawLine(lastPos, p);
                //记录
                lastPos = p;
            }
            //恢复颜色
            Gizmos.color = cacheColor;
        }
#endif
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(SimpleBezierCurvePath))]
    public class SimpleBezierCurvePathEditor : Editor
    {
        #region 变量
        private BezierCurve.BezierPoint currentbezierPoint;
        private float sphereHandleCapSize = 0.1f;
        private float depth = 1f;
        private SimpleBezierCurvePath path;
        private Color pointColor = Color.red;
        private Color controlColor = Color.blue;
        private Color tangentColor = Color.yellow;
        
        private int selectIndex = 0;
        private bool select;

        #endregion
        #region 样式变量
        private ReorderableList m_reorderableList;
        private bool editor_color_drop = false;

        #endregion
        private void OnEnable()
        {
            path = target as SimpleBezierCurvePath;

            if (m_reorderableList == null)
                m_reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("bezierCurve").FindPropertyRelative("points"));
            m_reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                string name = "Bezier Points";
                EditorGUI.LabelField(rect, name);
            };

            m_reorderableList.onAddCallback = (ReorderableList orderList) =>
            {
                if (orderList.serializedProperty != null)
                {
                    Vector3 offset = orderList.serializedProperty.arraySize > 0 ? orderList.serializedProperty.GetArrayElementAtIndex(orderList.serializedProperty.arraySize - 1).FindPropertyRelative("position").vector3Value : Vector3.zero;
                    orderList.serializedProperty.arraySize++;
                    orderList.index = orderList.serializedProperty.arraySize - 1;
                    SerializedProperty itemData = orderList.serializedProperty.GetArrayElementAtIndex(orderList.index);
                    itemData.FindPropertyRelative("position").vector3Value = Vector3.forward + offset;
                    itemData.FindPropertyRelative("tangent").vector3Value = Vector3.right;
                    itemData.FindPropertyRelative("index").intValue = orderList.index;
                    selectIndex = orderList.index;
                    select = true;
                    SceneView.RepaintAll();
                }
            };

            m_reorderableList.elementHeight = 70f;
            m_reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
              {
                  if (m_reorderableList.serializedProperty != null)
                  {
                      SerializedProperty itemData = m_reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                      EditorGUI.PropertyField(new Rect(rect.x, rect.y + 10f, rect.width, EditorGUIUtility.singleLineHeight), itemData.FindPropertyRelative("position"), new GUIContent("Position"));
                      EditorGUI.PropertyField(new Rect(rect.x, rect.y + 30f, rect.width, EditorGUIUtility.singleLineHeight), itemData.FindPropertyRelative("tangent"), new GUIContent("Tangent"));
                      EditorGUI.PropertyField(new Rect(rect.x, rect.y + 50f, rect.width, EditorGUIUtility.singleLineHeight), itemData.FindPropertyRelative("index"), new GUIContent("Index"));
                  }
              };

            m_reorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                if (list.serializedProperty != null && path.bezierCurve.points.Count > 0)
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    selectIndex = Mathf.Clamp(selectIndex, 0, list.count);
                    for (int i = 0; i < list.count; i++)
                    {
                        m_reorderableList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("index").intValue = i;
                    }
                    SceneView.RepaintAll();
                }
            };

            m_reorderableList.onReorderCallback = (ReorderableList list) =>
            {
                if (list.serializedProperty != null && path.bezierCurve.points.Count > 0)
                {
                    for (int i = 0; i < list.count; i++)
                    {
                        m_reorderableList.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("index").intValue = i;
                    }
                }
            };

            m_reorderableList.onSelectCallback = (ReorderableList list) =>
            {
                if (list.serializedProperty != null)
                {
                    selectIndex = list.index;
                    select = true;
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (GUILayout.Button("ClearAll", new GUIStyle("ButtonMid") { margin = new RectOffset(19, 5, 1, 0) }))
            {
                if (path.bezierCurve != null && path.bezierCurve.points != null)
                    path.bezierCurve.points.Clear();
            }
            editor_color_drop = EditorGUILayout.BeginFoldoutHeaderGroup(editor_color_drop, "EditorColor", new GUIStyle("DropDownToggleButton") { margin = new RectOffset(31, 8, 1, 0) });
            if (editor_color_drop)
            {
                path.pathColor = EditorGUILayout.ColorField("Line", path.pathColor);
                pointColor = EditorGUILayout.ColorField("Point", pointColor);
                controlColor = EditorGUILayout.ColorField("Control", controlColor);
                tangentColor = EditorGUILayout.ColorField("Tangent", tangentColor);
                EditorGUILayout.Space(5f);
            }
            m_reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;
            Rect windowSize = sceneView.position; ;
            GUILayout.Window(0, new Rect(windowSize.width - 210f, windowSize.height - 230f, 200f, 200f), DoBeizerWindow, string.Format("Bezier(Count:{0})", path.bezierCurve != null && path.bezierCurve.points != null ? path.bezierCurve.points.Count : null), new GUIStyle("flow node 0"));

            if (path.draw)
            {
                // 隐藏移动手柄
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Color cacheColor = Handles.color;
                //恢复颜色
                Handles.color = cacheColor;

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && SceneView.mouseOverWindow == sceneView)
                {
                    if (path.bezierCurve == null)
                        path.bezierCurve = new BezierCurve();
                    //当前屏幕坐标,左上角(0,0)右下角(camera.pixelWidth,camera.pixelHeight)
                    Vector2 mousePos = Event.current.mousePosition;
                    //retina 屏幕需要拉伸值
                    float mult = 1;
#if UNITY_5_4_OR_NEWER
                    mult = EditorGUIUtility.pixelsPerPoint;
#endif
                    //转换成摄像机可接受的屏幕坐标,左下角是(0,0,0);右上角是(camera.pixelWidth,camera.pixelHeight,0)
                    mousePos.y = sceneView.camera.pixelHeight - mousePos.y * mult;
                    mousePos.x *= mult;
                    //近平面往里一些,才能看到摄像机里的位置
                    Vector3 fakePoint = mousePos;
                    fakePoint.z = depth;

                    Vector3 position = sceneView.camera.ScreenToWorldPoint(fakePoint);

                    Undo.RecordObject(path, "Add Point");
                    path.bezierCurve.AddPoint(position);
                    EditorUtility.SetDirty(path);

                    selectIndex = path.bezierCurve.points.Count - 1;
                    if (select)
                        select = false;
                    sceneView.Repaint();
                }

                //遍历路径点集合
                if (path.bezierCurve != null && path.bezierCurve.points != null)
                {
                    var originPointCount = path.bezierCurve.points.Count;
                    for (int i = 0; i < originPointCount; i++)
                    {
                        //操作柄的旋转类型
                        Quaternion rotation = Tools.pivotRotation == PivotRotation.Local
                            ? path.transform.rotation : Quaternion.identity;
                        if (path.bezierCurve.points.Count != originPointCount)
                            break;
                        DrawPositionHandle(i, rotation);
                    }
                }
                HandleUtility.Repaint();
            }

            if (path.bezierCurve == null || path.bezierCurve.points == null || path.bezierCurve.points.Count == 0 || Tools.current != Tool.Move)
                return;
            if (!path.draw)
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftArrow)
                {
                    if (selectIndex == 0)
                        selectIndex = path.bezierCurve.points.Count - 1;
                    else
                    {
                        selectIndex--;
                    }
                }
                else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.RightArrow)
                {
                    if (selectIndex == path.bezierCurve.points.Count - 1)
                        selectIndex = 0;
                    else
                    {
                        selectIndex++;
                    }
                }
                // 隐藏移动手柄
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                Color cacheColor = Handles.color;
                //遍历路径点集合
                var originPointCount = path.bezierCurve.points.Count;
                for (int i = 0; i < originPointCount; i++)
                {
                    //操作柄的旋转类型
                    Quaternion rotation = Tools.pivotRotation == PivotRotation.Local
                        ? path.transform.rotation : Quaternion.identity;
                    DrawPositionHandle(i, rotation);
                    if (path.bezierCurve.points.Count != originPointCount)
                        break;
                    DrawTangentHandle(i, rotation);
                }
                //恢复颜色
                Handles.color = cacheColor;
                HandleUtility.Repaint();
            }

        }

        //工具窗口绘制
        private void DoBeizerWindow(int id)
        {
            EditorGUI.BeginChangeCheck();
            sphereHandleCapSize = EditorGUILayout.Slider("Size", sphereHandleCapSize, 0.1f, 1f);
            depth = EditorGUILayout.Slider("Depth", depth, 0.1f, 20f);
            path.loop = EditorGUILayout.Toggle("Loop", path.bezierCurve.loop);
            if (EditorGUI.EndChangeCheck())
            {
                if (path.bezierCurve != null)
                {
                    Undo.RecordObject(path, "Loop Curve");
                    path.bezierCurve.loop = path.loop;
                    EditorUtility.SetDirty(path);
                }
                SceneView.RepaintAll();
            }
            if (GUILayout.Button(path.draw ? "Operate" : "Draw"))
            {
                path.draw = !path.draw;
            }

            if (path.bezierCurve != null && path.bezierCurve.points != null && selectIndex < path.bezierCurve.points.Count && selectIndex >= 0)
            {
                EditorGUI.BeginChangeCheck();
                currentbezierPoint.position = EditorGUILayout.Vector3Field("Position", path.bezierCurve.points[selectIndex].position);
                currentbezierPoint.tangent = EditorGUILayout.Vector3Field("Tangent", path.bezierCurve.points[selectIndex].tangent);
                if (EditorGUI.EndChangeCheck())
                {
                    if (currentbezierPoint.index < path.bezierCurve.points.Count && currentbezierPoint.index >= 0)
                    {
                        path.bezierCurve.points[selectIndex] = new BezierCurve.BezierPoint()
                        {
                            position = currentbezierPoint.position,
                            tangent = currentbezierPoint.tangent,
                            index = path.bezierCurve.points[selectIndex].index
                        };
                    }
                }
                EditorGUI.BeginChangeCheck();
                currentbezierPoint.index = EditorGUILayout.IntField("Index", path.bezierCurve.points[selectIndex].index);
                if (EditorGUI.EndChangeCheck())
                {
                    if (selectIndex != currentbezierPoint.index)
                    {
                        path.bezierCurve.points[selectIndex] = new BezierCurve.BezierPoint()
                        {
                            position = currentbezierPoint.position,
                            tangent = currentbezierPoint.tangent,
                            index = path.bezierCurve.points[currentbezierPoint.index].index
                        };
                        path.bezierCurve.points[currentbezierPoint.index] = new BezierCurve.BezierPoint()
                        {
                            position = path.bezierCurve.points[currentbezierPoint.index].position,
                            tangent = path.bezierCurve.points[currentbezierPoint.index].tangent,
                            index = selectIndex
                        };
                        selectIndex = currentbezierPoint.index;
                        path.bezierCurve.points.Sort();
                    }
                }
            }

        }

        //路径点操作柄绘制
        private void DrawPositionHandle(int index, Quaternion rotation)
        {
            var point = path.bezierCurve.points[index];
            //局部转全局坐标
            Vector3 position = path.transform.TransformPoint(point.position);
            //操作柄的大小
            float size = HandleUtility.GetHandleSize(position) * sphereHandleCapSize;
            //在该路径点绘制一个球形
            Handles.color = pointColor;
            Handles.SphereHandleCap(0, position, rotation, size, EventType.Repaint);
            Handles.Label(position, string.Format("Point{0}", point.index));

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                // 创建射线
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                // 计算射线与手柄的距离
                float d = Vector3.Dot(position - ray.origin, ray.direction);
                float radius = Mathf.Sqrt(Mathf.Pow(Vector3.Distance(ray.origin, position), 2) - Mathf.Pow(d, 2));
                // 判断距离是否在手柄的范围内
                if (radius <= size)
                {
                    var currenIndex = index;
                    if (selectIndex == currenIndex)
                        select = !select;
                    selectIndex = index;
                }
            }
            if (selectIndex == index && select)
            {
                //检测变更
                EditorGUI.BeginChangeCheck();
                //坐标操作柄
                position = Handles.PositionHandle(position, rotation);
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space && SceneView.mouseOverWindow == SceneView.currentDrawingSceneView)
                {
                    Undo.RecordObject(path, "Remove Point");
                    path.bezierCurve.RemovePoint(index);
                    EditorUtility.SetDirty(path);
                    SceneView.currentDrawingSceneView.Repaint();
                    return;
                }
                //变更检测结束 如果发生变更 更新路径点
                if (EditorGUI.EndChangeCheck())
                {
                    //全局转局部坐标
                    point.position = path.transform.InverseTransformPoint(position);
                    //记录操作
                    Undo.RecordObject(path, "Position Changed");
                    //更新路径点
                    path.bezierCurve.points[index] = point;
                    EditorUtility.SetDirty(path);
                }
            }
        }

        //控制点操作柄绘制
        private void DrawTangentHandle(int index, Quaternion rotation)
        {
            var point = path.bezierCurve.points[index];
            //局部转全局坐标
            Vector3 position = path.transform.TransformPoint(point.position + point.tangent);
            //操作柄的大小
            float size = HandleUtility.GetHandleSize(position) * sphereHandleCapSize;
            //在该控制点绘制一个球形
            Handles.color = controlColor;
            Handles.SphereHandleCap(0, position, rotation, size, EventType.Repaint);
            //绘制切线
            Handles.color = tangentColor;
            Handles.DrawDottedLine(path.transform.TransformPoint(point.position), position, 1f);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                // 创建射线
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                // 计算射线与手柄的距离
                float d = Vector3.Dot(position - ray.origin, ray.direction);
                float radius = Mathf.Sqrt(Mathf.Pow(Vector3.Distance(ray.origin, position), 2) - Mathf.Pow(d, 2));
                // 判断距离是否在手柄的范围内
                if (radius <= size)
                {
                    var currenIndex = index;
                    if (selectIndex == currenIndex)
                        select = !select;
                    selectIndex = index;
                }
            }

            if (selectIndex == index && select)
            {
                //检测变更
                EditorGUI.BeginChangeCheck();
                //坐标操作柄
                position = Handles.PositionHandle(position, rotation);
                //变更检测结束 如果发生变更 更新路径点
                if (EditorGUI.EndChangeCheck())
                {
                    //全局转局部坐标
                    point.tangent = path.transform.InverseTransformPoint(position) - point.position;
                    //记录操作
                    Undo.RecordObject(path, "Control Point Changed");
                    //更新路径点
                    path.bezierCurve.points[index] = point;
                    EditorUtility.SetDirty(path);
                }
            }
        }
    }

#endif
}
