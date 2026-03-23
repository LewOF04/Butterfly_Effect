using UnityEngine;
public class SpeechBubbleController : MonoBehaviour
{
   public SpeechBubble speechBubble;

    public float minDuration = 2f;
    public float maxDuration = 6f;

    private float timer = 0f;
    private float currentDuration = 0f;

    private void Start()
    {
        switchOnOff();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= currentDuration)
        {
            switchOnOff();
        }
    }

    private void switchOnOff()
    {
        timer = 0f;

        currentDuration = Random.Range(minDuration, maxDuration);

        bool show = Random.value > 0.5f;
        speechBubble.gameObject.SetActive(show);
    }
}