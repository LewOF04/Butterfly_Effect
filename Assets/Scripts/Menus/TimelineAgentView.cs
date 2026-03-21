using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimelineAgentView : MonoBehaviour
{
    public Image spriteView;
    public TextMeshProUGUI nameField;
    public TextMeshProUGUI idField;

    public void displayData(NPC agent)
    {
        spriteView.gameObject.SetActive(true);
        spriteView.sprite = agent.gameObject.GetComponent<SpriteRenderer>().sprite;
        nameField.text = "Name: "+agent.fullName;
        idField.text = "ID: "+agent.id;
    }

    public void displayData(int id)
    {
        spriteView.gameObject.SetActive(false);
        nameField.text = "";
        idField.text = "ID: "+id.ToString();
    }
}