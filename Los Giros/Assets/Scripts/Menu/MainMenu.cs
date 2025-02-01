using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuJoseP : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    //Play Button
    public void Play()
    {
        //Load the Game Scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
