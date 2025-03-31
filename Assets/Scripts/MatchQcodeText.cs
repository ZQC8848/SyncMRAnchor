using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MatchQcodeText : MonoBehaviour
{
    [SerializeField]
    private string originPointText;
    [SerializeField]
    private string targetPointText;
    [SerializeField]
    private GameObject[] anchorObject = new GameObject[2];
    
    public Transform[] anchorPoints = new Transform[2];//0 是原点，1是方向点 

    private Transform root;
    
    private SpatialAnchorCoreBuildingBlock _spatialAnchorCoreBuildingBlock;
    
    private  bool initialAnchorCompleted = false;
    void Start()
    {
        _spatialAnchorCoreBuildingBlock = FindAnyObjectByType<SpatialAnchorCoreBuildingBlock>();
        root = GameObject.Find("SenceObjRoot").transform;
    }
    public void InitSpcatialAnchor(Transform pointTransform, string qCodeText)
    {
        if (qCodeText == originPointText && anchorPoints[0] == null)
        {
            Debug.Log("创建锚点0");
            _spatialAnchorCoreBuildingBlock.InstantiateSpatialAnchor(anchorObject[0],pointTransform.position, pointTransform.rotation);
            anchorPoints[0] = pointTransform;
            CheckAnchorCompleted();
        }
        else if (qCodeText == targetPointText && anchorPoints[1] == null)
        {
            Debug.Log("创建锚点1");
            _spatialAnchorCoreBuildingBlock.InstantiateSpatialAnchor(anchorObject[1],pointTransform.position, pointTransform.rotation);
            anchorPoints[1] = pointTransform;
            CheckAnchorCompleted();
        }
    }

    private void CheckAnchorCompleted()
    {
        if(initialAnchorCompleted) return;
        if (anchorPoints[0] != null && anchorPoints[1] != null)
        {
            initialAnchorCompleted = true;
            Debug.Log("锚点初始化完成");
            root.position = anchorPoints[0].position;
            Vector3 targetDir = anchorPoints[1].position - anchorPoints[0].position;
            targetDir.y = 0.0f;

            if (targetDir.magnitude > 0.0f)
            {
                root.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
            }
        }
    }
    
    public void ClearAnchor()
    {
        for (int i = 0; i < anchorPoints.Length; i++)
        {
            Destroy(anchorPoints[i].gameObject);
            anchorPoints[i] = null;
        }
        initialAnchorCompleted = false;
        _spatialAnchorCoreBuildingBlock.EraseAllAnchors();
    }
}
