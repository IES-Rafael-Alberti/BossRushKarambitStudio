using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyPauseMenu : MonoBehaviour
{
    public static DontDestroyPauseMenu instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }


    }
}
