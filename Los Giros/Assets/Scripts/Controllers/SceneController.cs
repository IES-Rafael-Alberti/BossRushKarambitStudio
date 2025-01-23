using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController: MonoBehaviour
{
    public static SceneController instance;
    [SerializeField] Animator transitionAnim;
    [SerializeField] GameObject mainMenuCanvas;
    public GameObject SceneTransition;

    public void StartGame()
    {
        StartCoroutine(LoadLevel());
        mainMenuCanvas.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

    IEnumerator LoadLevel()
    {
        transitionAnim.SetTrigger("End");
        yield return new WaitForSeconds(1);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        transitionAnim.SetTrigger("Start");
    }
}
