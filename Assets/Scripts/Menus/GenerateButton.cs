using UnityEngine;
using TMPro;
using System;

public class GenerateButton : MonoBehaviour
{
    public TMP_InputField inputField; 
    public MainMenu mainMenu;

    public void OnButtonPress()
    {
        string text = inputField.text ?? string.Empty;
        int seed = SeedFromString(text);
        mainMenu.GenerateGame(seed);
    }
    
    //converts a given string into an integer representation
    public static int SeedFromString(string s)
    {
        unchecked
        //implementation of the Fowler–Noll–Vo hashing algorithm
        {
            const uint offset = 2166136261; //starting seed value
            const uint prime = 16777619; //prime used for bit mixing
            uint hash = offset;

            foreach (char c in s ?? string.Empty)
            {
                //splits and hashes each byte of the character
                hash ^= (byte)(c & 0xFF);
                hash *= prime;
                hash ^= (byte)(c >> 8);
                hash *= prime;
            }

            return (int)(hash & 0x7FFF_FFFF); //returns non negative int
        }
    }
}
