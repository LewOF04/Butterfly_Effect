using UnityEngine;
using TMPro;

public class ActionConfirm : MonoBehaviour
{
    public TextMeshProUGUI actionNameField;
    public ActionViewPrefab sourceAction;

    public void yesClick()
    {
        sourceAction.actionConfirmed();
        this.gameObject.SetActive(false);
    }

    public void noClick()
    {
        this.gameObject.SetActive(false);
    }
}
