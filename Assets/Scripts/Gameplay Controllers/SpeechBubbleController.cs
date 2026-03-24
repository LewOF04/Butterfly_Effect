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
        Debug.Log("timer: "+timer.ToString("0.00")+", current duration = "+currentDuration.ToString("0.00"));

        if (timer >= currentDuration)
        {
            switchOnOff();
        }
    }

    private void switchOnOff()
    {
        timer = 0f;

        currentDuration = Random.Range(minDuration, maxDuration);
        Debug.Log("Current Duration: "+currentDuration.ToString("0.00"));

        bool show = Random.value > 0.5f;
        Debug.Log("Show: "+show.ToString());
        speechBubble.gameObject.SetActive(show);
    }
}