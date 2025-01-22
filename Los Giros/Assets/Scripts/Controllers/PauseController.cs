using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{

    [SerializeField] GameObject pauseMenuCanvas;

    void Update()
    {
        PauseButton();
    }

    public void PauseButton()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Pause();

        }
        
    }

    public void Pause()
    {
        pauseMenuCanvas.SetActive(true);
    }

    public void Return()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
