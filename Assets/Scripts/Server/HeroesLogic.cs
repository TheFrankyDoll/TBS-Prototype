using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Client_Hero : IdentifiableServerData
{
    protected ulong _netID;
    public ulong NetID { get => _netID; }

    /// <summary> An interface to send requests, meant to be executed on the MonoBehaviour.
    /// <para> WARN: it likely will be accessible CLIENT-only! </para></summary>
    public IHeroBehaviour RelatedBehaviour;

    public Server_Game.Teams Team;
    public bool ControlledByAI;
    [Space(5)]
    public int HP;
    public int MaxHP;
    [Space(5)]
    public Client_Ability[] Abilities;
    public List<Client_Effect> AppliedEffects;

    [HideInInspector] public int LastCastedID = -1;
}

[System.Serializable]
public class Server_Hero : Client_Hero, IServerItemOf<Client_Hero>
{
    public Client_Hero MakeClientCopy()
    {
        var clientCopy = this.MemberwiseClone() as Client_Hero;

        clientCopy.Abilities = new Client_Ability[Abilities.Length];
        for (int i = 0; i < clientCopy.Abilities.Length; i++)
        {
            clientCopy.Abilities[i] = (Abilities[i] as Server_Ability).MakeClientCopy();
        }
        clientCopy.LastCastedID = LastCastedID;

        clientCopy.AppliedEffects = new List<Client_Effect>();
        foreach(var serverEffect in AppliedEffects)
        {
            clientCopy.AppliedEffects.Add((serverEffect as Server_Effect).MakeClientCopy());
        }

        return clientCopy;
    }
    public void UpdateClientData(Client_Hero clientObject)
    {
        string json = JsonUtility.ToJson(this as Client_Hero);
        JsonUtility.FromJsonOverwrite(json, clientObject);

        for (int i = 0; i < clientObject.Abilities.Length; i++)
        {
            (Abilities[i] as Server_Ability).UpdateClientData(clientObject.Abilities[i]);
        }
        clientObject.LastCastedID = LastCastedID;

        clientObject.AppliedEffects = new List<Client_Effect>();
        foreach (var serverEffect in AppliedEffects)
        {
            clientObject.AppliedEffects.Add((serverEffect as Server_Effect).MakeClientCopy());
        }
    }

    public Server_Hero(Server_Game.Teams team, bool controlledByAI, int maxHP)
    {
        Team = team;
        ControlledByAI = controlledByAI;
        MaxHP = maxHP;
        HP = maxHP;

        // 
        _netID = Server_Game.NextNetID;
        Server_Game.NetworkToServerObjects.Add(NetID, this);

        //For this test task hard-code Abilities
        Abilities = new Server_Ability[5];
        Abilities[0] = new Ability_Attack(damage: 8);
        Abilities[1] = new Ability_Block(effectDuration: 8, blockTotal: 5);
        Abilities[2] = new Ability_Regen();
        Abilities[3] = new Ability_Fireball();
        Abilities[4] = new Ability_Cleansing();

        AppliedEffects = new List<Client_Effect>();

        Server_Game.main.OnTurnEnd += OnTurnEnd;
    }
    ~Server_Hero()
    {
        Server_Game.main.OnTurnEnd -= OnTurnEnd;
    }
    public void OnTurnEnd()
    {
        foreach (Server_Ability ability in Abilities.Cast<Server_Ability>()) {
            ability.OnTurnEndTick();
        }

        for (int i = AppliedEffects.Count - 1; i >= 0; i--)
        {
            var effect = (AppliedEffects[i] as Server_Effect);
            if (effect.DisposeMe)
            {
                AppliedEffects.RemoveAt(i);
                continue;
            }

            effect.OnTick();
            effect.OnTurnEndTick();

            if (effect.DisposeMe)
            {
                AppliedEffects.RemoveAt(i);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // If the order of the effects matters - we could sort them here. 
        foreach (IAlterDamage effect in AppliedEffects.OfType<IAlterDamage>())
        {
            damage = effect.AlterDamage(damage);
        }

        HP -= damage;
        Server_Game.Rpc_SendNumbers(this, -damage);
    }

    public void TakeHeal(int heal)
    {
        HP = Mathf.Min(HP + heal, MaxHP);
        Server_Game.Rpc_SendNumbers(this, heal);
    }
}

