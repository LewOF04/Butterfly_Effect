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

        bool success = ulong.TryParse(text, out ulong input);

        int seed;
        if (input > (ulong)Int32.MaxValue)
        {
            seed = (int)input % Int32.MaxValue;
        }
        else
        {
            seed = (int)input;
        }
       
        Debug.Log(input.ToString());

        if (!success)
        {
            Debug.Log("Your seed value must be an integer numerical value, less than 18,446,744,073,709,551,615.");
            //TODO: return error that it wasn't a valid integer
        }
        else
        {
          mainMenu.GenerateGame(seed);  
        }
        
    }
}
