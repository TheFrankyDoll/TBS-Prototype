using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectCell : MonoBehaviour
{
    public ulong EffectNetID;

    public Image Icon;
    public TextMeshProUGUI Duration;


    public void Redraw(Client_Effect data)
    {
        EffectNetID = data.NetID; //Keep ID so we can check if effect is present later.

        Icon.sprite = data.GetIcon();
        Duration.text = data.DurationLeft.ToString();
    }
}
