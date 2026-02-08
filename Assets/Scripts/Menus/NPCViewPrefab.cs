using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCViewPrefab : MonoBehaviour
{
    public NPC performer;
    public MonoBehaviour receiver;

    public TextMeshProUGUI nameField;
    public TextMeshProUGUI idField;
    public Image spriteImageLoc;
    public SpriteRenderer relationshipColour;
    public TextMeshProUGUI relationshipText;
    private DataController dataController = DataController.Instance;
    public void displayData()
    {
        if(receiver is NPC npc) 
        {
            relationshipText.gameObject.SetActive(true);
            nameField.text = "Name: "+npc.npcName; 
            idField.text = "ID: "+npc.id.ToString();
            spriteImageLoc.sprite = npc.GetComponent<SpriteRenderer>().sprite;
            spriteImageLoc.preserveAspect = true;
            relationshipColour.gameObject.SetActive(true); 

            RelationshipKey relKey = new RelationshipKey(performer.id, npc.id);
            setColour(0f, 100f, dataController.RelationshipStorage[relKey].value, relationshipColour);
        }
        if(receiver is Building building) 
        {
            relationshipText.gameObject.SetActive(false);
            nameField.text = "Name: "+building.buildingName; 
            idField.text = "ID: "+building.id.ToString();
            spriteImageLoc.sprite = building.GetComponent<SpriteRenderer>().sprite;
            spriteImageLoc.preserveAspect = true;
            relationshipColour.gameObject.SetActive(false);
        }
        
    }

    private void setColour(float minVal, float maxVal, float val, SpriteRenderer background)
    {
        val = Mathf.Clamp(val, minVal, maxVal);
        float t = Mathf.InverseLerp(Mathf.Min(minVal, maxVal), Mathf.Max(minVal, maxVal), val);

        Color amber = new Color(1f, 0.75f, 0f);

        Color colour = t < 0.5f
            ? Color.Lerp(Color.green, amber, t * 2f)
            : Color.Lerp(amber, Color.red, (t - 0.5f) * 2f);

        background.color = colour;
    }

    public void onPress()
    {
        NPCMenu npcMenu = FindFirstObjectByType<NPCMenu>();
        npcMenu.showNPCBuildActs(receiver);
    }
}