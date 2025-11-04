using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MainMenu : MonoBehaviour
{
    private DataController dataManager = DataController.Instance;
    public void LoadGame()
    {
        try
        {
            dataManager.LoadFromMemory();
        }
        catch (Exception e)
        {
            return; //TODO: get some error message that allows the user to generate
        }
        enterGame();
    }

    public void GenerateGame(int seed)
    {
        //TODO: retrieve seed from generate game
        dataManager.seedGenerate(seed);
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
