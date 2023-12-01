using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void PlayOptions()
    {
        SceneManager.LoadSceneAsync("Options");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}