using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Mode
{
    Build, Play, Success
}

public class GameController : MonoBehaviour
{
    private Mode mode = Mode.Build;
    public Mode Mode => mode;

    [SerializeField] private CameraMovement camMovement = null;

    [SerializeField] private GameObject marbleSpawner = null;
    [SerializeField] private GameObject marble = null;

    [SerializeField] private String nextScene = null;
    
    private GameObject currentMarble = null;
    public GameObject CurrentMarble => currentMarble;

    // Update is called once per frame
    void Update()
    {
        if (mode == Mode.Build && Input.GetKeyDown(KeyCode.Space))
        {
            mode = Mode.Play;
            SpawnNewMarble();
        }
    }

    private void SpawnNewMarble()
    {
        currentMarble = Instantiate(marble, marbleSpawner.transform.position, Quaternion.identity);
    }

    public void MarbleFailed()
    {
        mode = Mode.Build;
        camMovement.MoveCamUp();
    }

    public void MarbleSucceeded()
    {
        Debug.Log("Success!!! Wohoooooo");
        mode = Mode.Success;
        
        // load next level
        if (nextScene != null) StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(nextScene);
    }
}
