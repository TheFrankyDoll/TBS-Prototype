using UnityEngine;

// Client-side way of storing prefab data.
public class GameData : Singleton<GameData>
{
    public GameObject HeroPrefab;
    [Space(5)]
    public GameObject AbilityCell;
    public GameObject EffectCell;
    public GameObject AbilitiesUIPrefab;
    public GameObject HPBarPrefab;
    public GameObject DamageNumber;

    [Space(5)]
    public Transform BattleBoard;
    public Transform HeroUIs;

    [Space(5)]
    public GameObject EndOverlay;
}
