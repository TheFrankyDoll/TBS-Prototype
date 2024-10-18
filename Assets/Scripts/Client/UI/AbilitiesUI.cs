using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilitiesUI : MonoBehaviour
{
    public Hero Target;
    public AbilityCell[] AbilityCells;
    public Transform CellsParent;

    public Image LastCasted;

    Vector3 offset = new(0, -25f);

    private void Start()
    {
        AbilityCells = new AbilityCell[Target.HeroData.Abilities.Length];

        for (int i = 0; i < Target.HeroData.Abilities.Length; i++)
        {
            var cell = Instantiate(GameData.main.AbilityCell, CellsParent).GetComponent<AbilityCell>();
            AbilityCells[i] = cell;

            cell.Redraw(Target.HeroData.Abilities[i]);
        }

        Redraw();
    }

    public void Redraw()
    {
        transform.position = Camera.main.WorldToScreenPoint(new Vector3(Target.transform.position.x, Target.GetComponent<SpriteRenderer>().bounds.min.y)) + offset;

        if (Target.HeroData.ControlledByAI) CellsParent.gameObject.SetActive(false);
        else
        {
            CellsParent.gameObject.SetActive(true);
            for (int i = 0; i < AbilityCells.Length; i++)
            {
                AbilityCells[i].Redraw(Target.HeroData.Abilities[i]);
            }
        }
        

        if (Target.HeroData.LastCasted == null) LastCasted.gameObject.SetActive(false);
        else
        {
            LastCasted.gameObject.SetActive(true);
            LastCasted.sprite = Target.HeroData.LastCasted.GetIcon();
        }
    }
}
