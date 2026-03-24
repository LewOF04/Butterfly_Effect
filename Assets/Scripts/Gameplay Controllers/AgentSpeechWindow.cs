using UnityEngine;
using TMPro;

public class AgentSpeechWindow : MonoBehaviour
{
    public TextMeshProUGUI speechField;
    public TextMeshProUGUI agentNameField;

    public string targetSpeech = "";
    public string currentSpeech = "";
    public float charactersPerSecond = 30f;
    private float typeTimer = 0f;

    private void OnEnable()
    {
        refreshText();
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(targetSpeech))
        {
            currentSpeech = "";
            refreshText();
            return;
        }

        if (currentSpeech == targetSpeech) return;

        typeTimer += Time.deltaTime;
        float timePerCharacter = 1f / charactersPerSecond;

        while (typeTimer >= timePerCharacter && currentSpeech.Length < targetSpeech.Length)
        {
            typeTimer -= timePerCharacter;
            currentSpeech = targetSpeech.Substring(0, currentSpeech.Length + 1);
            refreshText();
        }
    }

    public void setSpeech(string newSpeech)
    {
        targetSpeech = newSpeech;
        currentSpeech = "";
        typeTimer = 0f;
        refreshText();
    }

    private void refreshText()
    {
        speechField.text = currentSpeech;
    }

    public void close()
    {
        targetSpeech = "";
        currentSpeech = "";
        speechField.text = "";
        agentNameField.text = "";
        GetComponent<Canvas>().enabled = false;
        InputLocker.Unlock();
    }
}