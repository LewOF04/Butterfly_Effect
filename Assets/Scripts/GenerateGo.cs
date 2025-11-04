using UnityEngine;
using TMPro;
using System;

public class GenerateGo : MonoBehaviour
{
    public TMP_InputField inputField; 
    public MainMenu mainMenu;   

    public void OnButtonPress()
    {
        string text = inputField.text;
        Debug.Log(text);
        int input;

        bool success = Int32.TryParse(text, out input);

        if (!success)
        {
            Debug.Log("Not success");
            //TODO: return error that it wasn't a valid integer
        }
        else
        {
          mainMenu.GenerateGame(input);  
        }
        
    }
}
