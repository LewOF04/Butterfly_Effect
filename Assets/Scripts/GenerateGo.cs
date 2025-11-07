using UnityEngine;
using TMPro;
using System;

public class GenerateGo : MonoBehaviour
{
    public TMP_InputField inputField; 
    public MainMenu mainMenu;

    public void OnButtonPress()
    {
        string text = inputField.text ?? string.Empty;
        int seed = SeedFromString(text);
        Debug.Log($"Seed: {seed}");
        mainMenu.GenerateGame(seed);

        /**int seed;
        string text = inputField.text; //get text from input field
        Debug.Log(text);
        char[] textArray = text.ToCharArray(); //convert string to char array


        int intRep = 0;
        ulong runningTotal = 0;
        int tempVal;
        List<int> seedArray = new List<int>();
        bool success = false;

        foreach (char chara in textArray) //iterate over charArray
        {
            success = Int32.TryParse(text, out tempVal); //try to parse the character to an int
            if (success) //if it is parsed correctly
            {
                seedArray.push(tempVal); //add the parsed value to the array
            }
            else
            {
                intRep = Char.GetNumericValue(chara); //convert character to its int representation
                seedArray.push(intRep); //add the integer representation to the array
            }
        }

        textArray = string.Join("", seedArray).ToCharArray(); //converts list into a singular string and then back into a character array

        ulong numVersion = seedSubSection(textArray);

        if (numVersion > (ulong)Int32.MaxValue)
        {
            seed = (int)numVersion % Int32.MaxValue;
        }
        else
        {
            seed = (int)numVersion;
        }

        Debug.Log(input.ToString());
        mainMenu.GenerateGame(seed);**/
    }

    /**private ulong seedSubSection(char[] textArray)
    {
        if (textArray.Length > 19) //if the array is too long
        {
            char[] firstHalf = string.Join("", textArray.Take(19)); //take the first half of the array
        }

        ulong secondNumber;
        if (textArray.Length - 19 > 19) //if the second half of the array is still too long
        {
            secondNumber = seedSubSection(textArray.Skip(19)); //calls seedSubSection on remaining half
        }
        else //if the second half of the array is not too long
        {
            secondNumber = ulong.Parse(new string(textArray.Skip(19))); //gets the numerical version of the second half
        }
        secondNumber = secondNumber % (ulong)Int32.MaxValue; //makes it no larger than the max int

        ulong firstNumber = ulong.Parse(new string(firstHalf));
        firstNumber = firstNumber % (ulong)Int32.MaxValue;

        return ulong.Parse(firstNumber.ToString() + secondNumber.ToString()); //returns concatenation of two halfs
    }**/
    
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
