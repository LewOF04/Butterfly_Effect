using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpdateOverview : MonoBehaviour
{
    public AgentUpdateInfo updateInfo;
    public TextMeshProUGUI textField;
    public void displayData()
    {
        textField.text = "ID: "+updateInfo.receiver.ToString()+"\n\n";
        textField.text += updateInfo.description;
    }
}