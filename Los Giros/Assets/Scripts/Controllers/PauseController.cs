using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [SerializeField] GameObject pauseMenuCanvas;
    [SerializeField] GameObject mainMenuCanvas;
    [SerializeField] AudioManager audioManager;

    void Awake()
    {
        pauseMenuCanvas.SetActive(false);
    }

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
        Time.timeScale = 0;
        Debug.Log("Tiempo detenido");
    }

    public void Resume()
    {
        Time.timeScale = 1;
        Debug.Log("Tiempo reanudado");
    }

    public void Return()
    {
        Resume();
        pauseMenuCanvas.SetActive(false);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex - 1);
        mainMenuCanvas.SetActive(true);
        audioManager.MainTheme();
    }

}
