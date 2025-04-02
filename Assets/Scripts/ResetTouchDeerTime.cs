using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTouchDeerTime : MonoBehaviour
{
    // Start is called before the first frame update
    private SetDeerAnimation deerAnimController;
    void Start()
    {
        deerAnimController = GameObject.FindObjectOfType<SetDeerAnimation>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Deer"))
        {
            deerAnimController.ResetTouchTime();
        }
    }

}
