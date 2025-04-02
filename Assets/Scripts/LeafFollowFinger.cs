using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafFollowFinger : MonoBehaviour
{
    Transform finger;
    private void Start()
    {
        finger = transform.parent;
    }
    // Update is called once per frame
    void Update()
    {
        this.transform.position = finger.position;
    }
}
