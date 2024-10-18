using UnityEngine;

// Client_Hero will use this to send info to related MonoBehaviour object.
public interface IHeroBehaviour { 
    public void RedrawUIs();
    public void SendNumber(string number, Color color);
}

public class Hero : MonoBehaviour, IHeroBehaviour
{
    public Client_Hero HeroData;

    public HPBarUI relatedBar;
    public AbilitiesUI relatedAbilities;

    void Start()
    {
        relatedBar = Instantiate(GameData.main.HPBarPrefab, GameData.main.HeroUIs).GetComponent<HPBarUI>();
        relatedBar.Target = this;

        relatedAbilities = Instantiate(GameData.main.AbilitiesUIPrefab, GameData.main.HeroUIs).GetComponent<AbilitiesUI>();
        relatedAbilities.Target = this;

        ClientSideData.OnTurnEnd += RedrawUIs;
    }

    public void RedrawUIs()
    {
        relatedBar.Redraw();
        relatedAbilities.Redraw();
    }
    public void SendNumber(string number, Color color) => relatedBar.SendNumber(number, color);

    private void OnDestroy()
    {
        ClientSideData.OnTurnEnd -= RedrawUIs;
        if (relatedBar) Destroy(relatedBar);
        if(relatedAbilities) Destroy(relatedAbilities);
    }

    
}
