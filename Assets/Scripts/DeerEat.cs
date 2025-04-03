using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MultiplayerBlocks.Fusion;
public class DeerEat : MonoBehaviour
{
    public SetDeerAnimation deerAnimationController;
    private void OnTriggerEnter(Collider other)
    {
        //Debug.LogWarning(other.name);
        if (other.CompareTag("Leaf"))
        {
            Destroy(other.gameObject);
            deerAnimationController.SetDeerAnimTrigger("Eat");
        }
    }
}
