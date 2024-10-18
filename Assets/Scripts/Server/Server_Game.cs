using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Server_Game : Singleton<Server_Game>
{
    // Simple way to gen unique NetIDs.
    private static ulong nextNetID = 0;
    public static ulong NextNetID { get { var val = nextNetID; nextNetID++; return val; } }

    /// <summary> Keeps all NetObjects. Must be assessable only server-side. </summary>
    public static readonly Dictionary<ulong, object> NetworkToServerObjects = new Dictionary<ulong, object>();

    public delegate void TurnState();
    public event TurnState OnTurnEnd;

    public enum Teams { Red, Blue };
    public Teams CurrentTurnSide;
    public bool MidFight;

    public Server_Hero[] AllHeroes;


    void Start()
    {
        BeginBattle();
    }

    // "Rpc" means this method must be executed by server for EACH client.
    // Similar to clientrpc-calls in mirror: https://mirror-networking.gitbook.io/docs/manual/guides/communications/remote-actions#clientrpc-calls
    static void Rpc_WipeBoard()
    {
        GameData.main.EndOverlay.SetActive(false);
        foreach (Transform child in GameData.main.HeroUIs) Destroy(child.gameObject);
        foreach (Transform child in GameData.main.BattleBoard) Destroy(child.gameObject);
    }

    void BeginBattle()
    {
        AllHeroes = new Server_Hero[2];
        
        Server_Hero left = new(team: Teams.Red, maxHP: Random.Range(75, 251), controlledByAI: false);
        Server_Hero right = new(team: Teams.Blue, maxHP: Random.Range(75, 251), controlledByAI: true);

        AllHeroes[0] = left;
        AllHeroes[1] = right;

        CurrentTurnSide = Teams.Red;
        MidFight = false;

        Rpc_LoadHeroes(AllHeroes);
    }


    static void Rpc_LoadHeroes(Server_Hero[] heroes)
    {
        for (int i = 0; i < heroes.Length; i++)
        {
            Server_Hero serverHero = heroes[i];
            var clientHero = Instantiate(GameData.main.HeroPrefab, GameData.main.BattleBoard).GetComponent<Hero>();

            clientHero.HeroData = serverHero.MakeClientCopy();
            clientHero.HeroData.RelatedBehaviour = clientHero;
            ClientSideData.NetworkToClientObjects.Add(serverHero.NetID, clientHero.HeroData);


            // TestTask suggests only 2 Heroes to be placed. Hard-code them by the team.
            switch (serverHero.Team)
            {
                case Teams.Red:
                    clientHero.transform.position = new Vector3(-3,0,0);
                    clientHero.transform.rotation = Quaternion.identity;
                    break;
                case Teams.Blue:
                    clientHero.transform.position = new Vector3(3, 0, 0);
                    clientHero.transform.rotation = Quaternion.Euler(0,180,0);
                    break;
            }
        }
    }


    // "Cmd" means this method is a command from client to be executed on server.
    // Similar to [Command] attribute in mirror: https://mirror-networking.gitbook.io/docs/manual/guides/communications/remote-actions#commands
    public void Cmd_TurnRequest(Client_Hero from, int abilityID, Client_Hero target = null)
    {
        Server_Hero fromS = NetworkToServerObjects[from.NetID] as Server_Hero;
        Server_Hero targetS = null;

        if (target == null)
        {
            //This TestTask doesn't account for multi-enemy battles or target selection,
            //so if ability is "CastToSelf" - target is "from"; otherwise it is first hero from an opposite team.
            if (fromS.Abilities[abilityID].CastToSelf) targetS = fromS;
            else if(fromS.Team == Teams.Red) targetS = AllHeroes.First(x => x.Team == Teams.Blue);
            else targetS = AllHeroes.First(x => x.Team == Teams.Red);
        }
        else targetS = NetworkToServerObjects[target.NetID] as Server_Hero;

        //Validate turn
        if (!fromS.Abilities[abilityID].ValidToUse())
        {
            Debug.Log("[Server] Cmd_TurnRequest denied, invalid move!");
            return;
        }
        if(fromS.Team != CurrentTurnSide || MidFight)
        {
            Debug.Log("[Server] Cmd_TurnRequest denied, it's not time for this team to move!");
            return;
        }

        doTurn(fromS, fromS.Abilities[abilityID] as Server_Ability, targetS);
    }

    void doAITurn(Server_Hero from)
    {
        //Pick random ability from those that are valid to use.
        var validAbilities = from.Abilities.Where(x => x.ValidToUse()).ToArray();
        Server_Ability ability = validAbilities[Random.Range(0, validAbilities.Length)] as Server_Ability;

        //Pick target
        Server_Hero target;

        //if ability is "CastToSelf" - target is "from"; otherwise it is first hero from an opposite team.
        if (ability.CastToSelf) target = from;
        else if (from.Team == Teams.Red) target = AllHeroes.First(x => x.Team == Teams.Blue);
        else target = AllHeroes.First(x => x.Team == Teams.Red);

        doTurn(from, ability, target);
    }

    void doTurn(Server_Hero from, Server_Ability ability, Server_Hero target)
    {
        MidFight = true;

        //Here would be a good place to call animations and effects on client's side.
        ability.UponCast(target);
        from.LastCasted = ability;

        RedrawAllClientHeroes();

        StartCoroutine(afterTurn());
    }


    IEnumerator afterTurn()
    {
        OnTurnEnd?.Invoke();

        MidFight = false;
        switch (CurrentTurnSide)
        {
            case Teams.Red:
                CurrentTurnSide = Teams.Blue;
                break;
            case Teams.Blue:
                CurrentTurnSide = Teams.Red;
                break;
        }

        RedrawAllClientHeroes();

        foreach (var hero in AllHeroes)
        {
            if(hero.HP <= 0)
            {
                GameOver();
                yield break;
            }
        }

        if (CurrentTurnSide == Teams.Blue)
        {
            //Give AI a little time to 'think'.
            yield return new WaitForSecondsRealtime(0.5f);

            doAITurn(AllHeroes.First(x => x.Team == Teams.Blue));
        }
    }

    public void GameOver()
    {
        Rpc_SetEndOverlay();
        Invoke(nameof(RestartGame), 3f);
    }
    void Rpc_SetEndOverlay()
    {
        GameData.main.EndOverlay.SetActive(true);
    }


    public void RedrawAllClientHeroes()
    {
        foreach (var hero in AllHeroes) Rpc_SendHeroData(hero);
    }
    public static void Rpc_SendHeroData(Server_Hero serverHero)
    {
        var clientHero = (ClientSideData.NetworkToClientObjects[serverHero.NetID] as Client_Hero);
        serverHero.UpdateClientData(clientHero);
        clientHero.RelatedBehaviour.RedrawUIs();
    }

    public static void Rpc_SendNumbers(Server_Hero serverHero, int numbers)
    {
        var clientHero = (ClientSideData.NetworkToClientObjects[serverHero.NetID] as Client_Hero);

        string numStr = string.Empty;
        Color color;

        if(numbers > 0)
        {
            color = Color.green;
            numStr = $"+{numbers}";
        }
        else if(numbers < 0) {
            color = Color.red;
            numStr = $"{numbers}";
        }
        else
        {
            numStr = "0";
            color = Color.gray;
        }

        clientHero.RelatedBehaviour.SendNumber(numStr, color);
    }

    public void Cmd_CallRestartGame()
    {
        //We could deny client request here.
        RestartGame();
    }
    void RestartGame()
    {
        Rpc_WipeBoard();
        BeginBattle();
    }
}
