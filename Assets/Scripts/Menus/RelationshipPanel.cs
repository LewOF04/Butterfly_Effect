using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RelationshipPanel : MonoBehaviour
{
    public NPC secondaryNPC;
    public TextMeshProUGUI npcNameField;
    public TextMeshProUGUI relationshipValueField;
    public TextMeshProUGUI idInfoField;
    public Slider slider;
    public Image spriteImageLoc;
    public DataController dataController = DataController.Instance;
    public Relationship relationship;

    public void displayData()
    {
        npcNameField.text = secondaryNPC.npcName;
        idInfoField.text = "ID: "+secondaryNPC.id.ToString();

        Sprite npcSprite = secondaryNPC.GetComponent<SpriteRenderer>().sprite;
        spriteImageLoc.sprite = npcSprite;
        spriteImageLoc.preserveAspect = true;

        relationshipValueField.text = "Relationship Level: "+relationship.value.ToString("F2");
        slider.value = relationship.value;
    }

    public void handleSliderChange()
    {
        relationshipValueField.text = "Relationship Level: "+slider.value.ToString("F2");
    }

    public void save()
    {
        relationship.value = slider.value;
    }
}
