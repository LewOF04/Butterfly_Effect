using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text textField;

    public void hanldeSliderChange(float value)
    {
        textField.SetText(value.ToString("F2"));
    }
}
