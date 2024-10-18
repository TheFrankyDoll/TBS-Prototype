using UnityEngine;

[System.Serializable]
public class Client_Effect : IdentifiableServerData
{
    protected ulong _netID;
    public ulong NetID { get => _netID; }

    public int StartDuration;
    public int DurationLeft;

    [HideInInspector] public string TypeName; //Have to store it in variable for it to be Serializable to client.
    public Sprite GetIcon() => Resources.Load<Sprite>($"StatusEffectsIcons/{TypeName}");
}

[System.Serializable]
public abstract class Server_Effect : Client_Effect, IServerItemOf<Client_Effect>
{
    public Client_Effect MakeClientCopy() => this.MemberwiseClone() as Client_Effect;
    public void UpdateClientData(Client_Effect clientObject)
    {
        string json = JsonUtility.ToJson(this as Client_Effect);
        JsonUtility.FromJsonOverwrite(json, clientObject);
    }

    public Server_Hero AppliedTo;

    /// <summary> Effect can tick this so it will be cleared before Duration rans out. </summary>
    public bool DisposeMe { get; protected set; }

    protected Server_Effect(Server_Hero appliedTo, int startDuration)
    {
        AppliedTo = appliedTo;
        DurationLeft = startDuration;
    }

    public virtual void OnTurnEndTick()
    {
        if (DurationLeft > 0) DurationLeft--;
        if (DurationLeft <= 0) DisposeMe = true;
    }

    public abstract void OnTick();
}

public interface IAlterDamage
{
    public int AlterDamage(int inputDamage);
}

public class Effect_Heal : Server_Effect
{
    public Effect_Heal(Server_Hero appliedTo, int startDuration) : base(appliedTo, startDuration) { TypeName = GetType().Name; }

    public int Heal = 2;

    public override void OnTick()
    {
        AppliedTo.TakeHeal(Heal);
    }
}

public class Effect_Burn : Server_Effect
{
    public Effect_Burn(Server_Hero appliedTo, int startDuration) : base(appliedTo, startDuration) { TypeName = GetType().Name; }

    public int Damage = 1;

    public override void OnTick()
    {
        AppliedTo.TakeDamage(Damage);
    }
}

public class Effect_Protection : Server_Effect, IAlterDamage
{
    public Effect_Protection(Server_Hero appliedTo, int startDuration, int armor) : base(appliedTo, startDuration) {
        TypeName = GetType().Name;
        Armor = armor;
    }

    public int Armor;

    public override void OnTick() { }

    public int AlterDamage(int inputDamage)
    {
        if (Armor > inputDamage)
        {
            Armor -= inputDamage;
            return 0;
        }
        else
        {
            inputDamage -= Armor;
            Armor = 0;
            DisposeMe = true;
            return inputDamage;
        }
    }
}

