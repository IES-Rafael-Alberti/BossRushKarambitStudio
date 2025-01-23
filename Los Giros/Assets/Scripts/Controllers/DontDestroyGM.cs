using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyGM : MonoBehaviour
{
    public static DontDestroyGM instance;

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
