using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TraitViewItem : MonoBehaviour
{
    public TextMeshProUGUI traitName;
    public TextMeshProUGUI traitDescription; 
    public TraitData trait;

    public void displayData()
    {
        traitName.text = trait.displayName;
        traitDescription.text = trait.description;
    }
}