using UnityEngine;

[System.Serializable]
public class Client_Ability : IdentifiableServerData
{
    protected ulong _netID;
    public ulong NetID { get => _netID; }

    public bool CastToSelf;

    [Space(5)]
    public int Cooldown;
    public int Duration;

    [HideInInspector] public string TypeName; //Have to store it in variable for it to be Serializable to client.
    public Sprite GetIcon() => Resources.Load<Sprite>($"AbilityIcons/{TypeName}");

    public virtual bool ValidToUse() => Cooldown <= 0 && Duration <= 0;
}

[System.Serializable]
public abstract class Server_Ability : Client_Ability, IServerItemOf<Client_Ability>
{
    public Client_Ability MakeClientCopy() => this.MemberwiseClone() as Client_Ability;
    public void UpdateClientData(Client_Ability clientObject)
    {
        string json = JsonUtility.ToJson(this as Client_Ability);
        JsonUtility.FromJsonOverwrite(json, clientObject);
    }

    public virtual void OnTurnEndTick()
    {
        if (Duration > 0) Duration--;
        else if(Cooldown > 0) Cooldown--;
    }

    public abstract void UponCast(Server_Hero target);

    //public virtual void CastOver(int cooldown) => Cooldown = cooldown;
}


public class Ability_Attack : Server_Ability
{
    public int Damage = 8;

    public Ability_Attack(int damage)
    {
        TypeName = GetType().Name;
        CastToSelf = false;

        Damage = damage;
    }

    public override void UponCast(Server_Hero target) => target.TakeDamage(Damage);
}


public class Ability_Block : Server_Ability
{
    public int EffectDuration = 8;
    public int BlockTotal = 5;

    public Ability_Block(int effectDuration, int blockTotal)
    {
        TypeName = GetType().Name;
        CastToSelf = true;

        EffectDuration = effectDuration;
        BlockTotal = blockTotal;
    }

    public override void UponCast(Server_Hero target)
    {
        Duration = 2;
        Cooldown = 4;

        var effect = new Effect_Protection(target, EffectDuration, BlockTotal);

        target.AppliedEffects.Add(effect);
    }
}


public class Ability_Regen : Server_Ability
{
    public Ability_Regen()
    {
        TypeName = GetType().Name;
        CastToSelf = true;
    }

    public override void UponCast(Server_Hero target)
    {
        Duration = 3;
        Cooldown = 5;

        var effect = new Effect_Heal(target, Duration);

        target.AppliedEffects.Add(effect);
    }
}

public class Ability_Fireball : Server_Ability
{
    public Ability_Fireball()
    {
        TypeName = GetType().Name;
        CastToSelf = false;
    }

    public override void UponCast(Server_Hero target)
    {
        target.TakeDamage(5);

        Duration = 5;
        Cooldown = 6;

        var effect = new Effect_Burn(target, Duration);

        target.AppliedEffects.Add(effect);
    }
}


public class Ability_Cleansing : Server_Ability
{
    public Ability_Cleansing()
    {
        TypeName = GetType().Name;
        CastToSelf = true;
    }

    public override void UponCast(Server_Hero target)
    {
        target.AppliedEffects.RemoveAll(x => x.GetType() == typeof(Effect_Burn));

        Cooldown = 5;
    }
}