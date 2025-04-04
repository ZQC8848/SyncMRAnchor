using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTutorialState : MonoBehaviour
{
    public List<GameObject> tutorials;
    public bool state = true;
    public List<GameObject> deerDialogs;

    public void SwitchTutorialStateFunc()
    {
        state = !state;
        foreach (var tutorial in tutorials)
        {
            tutorial.SetActive(state);
        }
        if (state)
        {
            foreach (var deerDialog in deerDialogs)
            {
                deerDialog.GetComponent<CanvasGroup>().alpha = 1;
            }
        }
        else
        {
            foreach (var deerDialog in deerDialogs)
            {
                deerDialog.GetComponent<CanvasGroup>().alpha = 0;
            }
        }
        
    }
}
