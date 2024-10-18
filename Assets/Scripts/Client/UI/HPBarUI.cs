using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBarUI : MonoBehaviour
{
    public Hero Target;
    public TextMeshProUGUI Count;
    public Image Fill;
    [Space(5)]
    public Transform EffectsParent;
    public List<EffectCell> EffectCells = new List<EffectCell>();

    Vector3 offset = new(0, 25f);

    void Start()
    {
        Redraw();
    }

    public void SendNumber(string number, Color color)
    {
        Vector3 pos = new(transform.position.x + Random.Range(-25f, 25f), transform.position.y + 30f, 0f);
        var numObj = Instantiate(GameData.main.DamageNumber, pos, Quaternion.identity, transform).GetComponent<DamageNumber>();
        numObj.TextMesh.color = color;
        numObj.TextMesh.text = number;
    }

    public void Redraw()
    {
        transform.position = Camera.main.WorldToScreenPoint(new Vector3(Target.transform.position.x, Target.GetComponent<SpriteRenderer>().bounds.max.y)) + offset;

        Count.text = $"{Target.HeroData.HP}/{Target.HeroData.MaxHP}";
        Fill.fillAmount = (float)Target.HeroData.HP / (float)Target.HeroData.MaxHP;

        RedrawEffects();
    }

    // Check all (already) drawn effects, update or add new if needed and clear all that were disposed.
    public void RedrawEffects()
    {
        List<EffectCell> drawnCells = new List<EffectCell>(EffectCells);
        
        foreach (var effect in Target.HeroData.AppliedEffects)
        {
            EffectCell cell;
            if (drawnCells.Exists(x => x.EffectNetID == effect.NetID))
            {
                cell = drawnCells.First(x => x.EffectNetID == effect.NetID);
            }
            else
            {
                cell = Instantiate(GameData.main.EffectCell, EffectsParent).GetComponent<EffectCell>();
                EffectCells.Add(cell);
            }
            cell.Redraw(effect);
            drawnCells.Remove(cell);
        }

        foreach(var notUsed in drawnCells)
        {
            Destroy(notUsed.gameObject);
            EffectCells.Remove(notUsed);
        }

        drawnCells.Clear();
    }
}
