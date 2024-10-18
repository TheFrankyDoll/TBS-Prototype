using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityCell : MonoBehaviour
{
    public AbilitiesUI UI => GetComponentInParent<AbilitiesUI>();

    public Image Icon;
    public TextMeshProUGUI Info;
    public Button Button;

    public void MoveRequest()
    {
        Server_Game.main.Cmd_TurnRequest(UI.Target.HeroData, transform.GetSiblingIndex());
    }

    public void Redraw(Client_Ability abilityData)
    {
        Button.interactable = abilityData.ValidToUse();
        Icon.sprite = abilityData.GetIcon();

        if (abilityData.Duration > 0) Info.text = $"DUR: {abilityData.Duration}";
        else if (abilityData.Cooldown > 0) Info.text = $"CD: {abilityData.Cooldown}";
        else Info.text = string.Empty;
    }
}
