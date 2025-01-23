using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyMainMenu : MonoBehaviour
{
    public static DontDestroyMainMenu instance;

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
