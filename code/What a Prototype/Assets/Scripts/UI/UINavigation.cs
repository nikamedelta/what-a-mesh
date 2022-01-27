using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UINavigation : MonoBehaviour
{
    public void OpenScene(String scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void Terminate()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name == "LevelSelection")
            {
                Terminate();
            }
            else
            {
                SceneManager.LoadScene("LevelSelection");
            }
        }
    }
}