using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSoundManager : MonoBehaviour
{
    public AudioClip buttonClick;
    public AudioSource audioSource;

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>() != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        audioSource.PlayOneShot(buttonClick);
                    }
                }
            }
        }
    }
}