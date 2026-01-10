using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MainMenu : MonoBehaviour
{
    public GameObject gameSysPrefab;
    public void LoadGame()
    {
        Instantiate(gameSysPrefab);
        try
        {
            DataController.Instance.LoadFromMemory();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return;
        }
        enterGame();
    }

    public void GenerateGame(int seed)
    {
        if (DataController.Instance != null)
        {
            Destroy(DataController.Instance);
        }
        Instantiate(gameSysPrefab);
        DataController.Instance.seedGenerate(seed);
        enterGame();
    }

    public void QuiteGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    private void enterGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
