using System;
using System.Linq;
using System.Collections.Generic;
using Cards;
using Cysharp.Threading.Tasks;
using Dman.Utilities;
using Dman.Utilities.Logger;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


public class GameManager : NetworkBehaviour, ICoordinateGame
{
    public static ICoordinateGame GAME_MANAGER;
    /// <summary>
    /// the client ID is generated once per client, before network connection.
    /// Used to uniquely identify each client in lobbies. 
    /// </summary>
    private static Guid CLIENT_ID = Guid.NewGuid();
    private static FixedString64Bytes ClientIdString => new(CLIENT_ID.ToString());
    private static string ClientIdStringShort => CLIENT_ID.ToString().Substring(0, 4);
    

    [SerializeField] private List<PlayerData> playerDirectory;
    
    [Serializable]
    private struct PlayerData
    {
        public FixedString64Bytes clientId;
    }

    public NetworkList<CardData> allCardData = new();
    
    public NetworkVariable<GamePhase> gamePhase = new(GamePhase.ChoosingActions);
    
    public NetworkVariable<FixedString64Bytes> p0Id = new();
    public NetworkVariable<FixedString64Bytes> p1Id = new();

    public UnityEvent OnMyStateChanged => onMyStateChanged;
    public UnityEvent onMyStateChanged = new();
    public NetworkVariable<PlayerState> p0State = new();
    
    public UnityEvent OnOpponentStateChanged => onOpponentStateChanged;
    public UnityEvent onOpponentStateChanged = new();
    public NetworkVariable<PlayerState> p1State = new();
    
    public NetworkVariable<CombatWinner> lastWinner = new();
    public UnityEvent<MyWinState> onGameResolved = new();
    public UnityEvent<MyWinState> OnGameResolved => onGameResolved;
    
    [Tooltip("In Seconds")]
    public float countdownTime = 1f;
    [Tooltip("In Seconds")]
    public float handRevealTime = 1f;
    [Tooltip("In Seconds")]
    public float winRevealTime = 1f;

    public int initialHandSize = 3;
    public bool drawOnLose = true;
    public bool drawOnWin = true;
    public bool drawOnDraw = true;

    private CardIdGenerator _cardIdGenerator;
    
    private GameUIManager GameUIManager => SingletonLocator<GameUIManager>.Instance; 

    public MyWinState GetMyWinState()
    {
        return GetWinState(gamePhase.Value, lastWinner.Value, GetCurrentPlayer());
    }

    private static MyWinState GetWinState(
        GamePhase currentPhase,
        CombatWinner lastWinner,
        CombatPlayer? currentPlayer)
    {
        if (currentPhase != GamePhase.RevealWinner)
        {
            return MyWinState.None;
        }
        
        if(currentPlayer == null)
        { // I may not be participating in the game at all
            return MyWinState.None;
        }
        
        var playerWinner = lastWinner.TryToPlayer();
        if(playerWinner == null) return MyWinState.Draw;
        
        return playerWinner == currentPlayer ? MyWinState.MyWin : MyWinState.MyLoss;
    }
    
    
    public PlayerState? GetMyState()
    {
        if (IsP0()) return p0State.Value;
        if (IsP1()) return p1State.Value;
        return null;
    }
    
    public PlayerState? GetOpponentState()
    {
        if (IsP0()) return p1State.Value;
        if (IsP1()) return p0State.Value;
        return null;
    }
    
    private bool IsP0()
    {
        return p0Id.Value == ClientIdString;
    }
    private bool IsP1()
    {
        return p1Id.Value == ClientIdString;
    }

    private CombatPlayer? GetCurrentPlayer()
    {
        if (IsP0()) return CombatPlayer.Player0;
        if (IsP1()) return CombatPlayer.Player1;
        return null;
    }
    
    public void Register()
    {
        var currentPlayerId = ClientIdString;
        Log.Info("REGISTERING " + currentPlayerId);
        RegisterRpc(currentPlayerId);
    }

    public void PlayCard(CardId cardId)
    {
        var currentPlayerId = ClientIdString;
        
        Log.Info($"{currentPlayerId} is playing {cardId}");
        
        PlayCardServerRpc(cardId, currentPlayerId);
    }
    
    [Rpc(SendTo.Server)]
    private void PlayCardServerRpc(CardId cardId, FixedString64Bytes id)
    {
        Log.Info($"Received {cardId} from {id}");
        PlayCard(id, cardId);
    }

    /// <summary>
    /// always runs on the server
    /// </summary>
    private void PlayCard(FixedString64Bytes playerId, CardId playedCard)
    {
        if(!gamePhase.Value.AllowsChangeAction())
        {
            return;
        }

        var cardLookup = allCardData.AsEnumerable().ToDictionary(x => x.cardId, x => x);
        
        if(p0Id.Value == playerId)
        {
            p0State.Value = p0State.Value.PlayCard(playedCard, cardLookup);
        }
        else if(p1Id.Value == playerId)
        {
            p1State.Value = p1State.Value.PlayCard(playedCard, cardLookup);
        }
    }

    [Rpc(SendTo.Server)]
    private void RegisterRpc(FixedString64Bytes id)
    {
        Log.Info("REGISTERING " + id);

        PlayerData newData = new() { clientId = id };

        playerDirectory.Add(newData);

        TryAddToMatch(id);
    }

    private bool TryAddToMatch(FixedString64Bytes id)
    {
        if (p0Id.Value.IsEmpty)
        {
            p0Id.Value = id;
            return true;
        }
        if (p1Id.Value.IsEmpty)
        {
            p1Id.Value = id;
            return true;
        }

        return false;
    }
    
    void Awake()
    {
        ThereCanBeOnlyOne();

        _cardIdGenerator = CardIdGenerator.Create();
        playerDirectory = new List<PlayerData>();
        
        SingletonLocator<IConnectionManager>.Instance.OnConnectionBegin += InstanceOnOnConnectionBegin;
        
        gamePhase.OnValueChanged += OnGamePhaseChanged;
        
        p0State.OnValueChanged += OnP0StateChanged;
        p1State.OnValueChanged += OnP1StateChanged;
    }
    
    void ThereCanBeOnlyOne()
    {
        if (GAME_MANAGER == null)
        {
            GAME_MANAGER = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnP0StateChanged(PlayerState previousvalue, PlayerState newvalue)
    {
        Log.Info("P0 state changed");
        if(IsP0()) onMyStateChanged.Invoke();
        if(IsP1()) onOpponentStateChanged.Invoke();
    }
    private void OnP1StateChanged(PlayerState previousvalue, PlayerState newvalue)
    {
        Log.Info("P1 state changed");
        if(IsP1()) onMyStateChanged.Invoke();
        if(IsP0()) onOpponentStateChanged.Invoke();
    }
    
    private void OnGamePhaseChanged(GamePhase prevValue, GamePhase newValue)
    {
        GameUIManager.OnGamePhaseChanged(newValue);
        if(newValue == GamePhase.CountingDown)
        {
            GameUIManager.BeginCountdown(countdownTime);
        }
    }

    private void InstanceOnOnConnectionBegin(ConnectionType conType)
    {
        if (conType == ConnectionType.Client) return;

        if (!IsServer)
        {
            Log.Error("Expected to be server when not client, but was not server");
            return;
        }
        RunGameServerTiming().Forget();
    }

    private async UniTask RunGameServerTiming()
    {
        Log.Info("Waiting for registrations");
        
        await UniTask.WaitUntil(() => 
            !p0Id.Value.IsEmpty && !p1Id.Value.IsEmpty);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        
        Log.Info("Registration done, initializing players!");
        allCardData.Clear();
        var rng = new System.Random(UnityEngine.Random.Range(1, int.MaxValue));
        
        var p0Cards = GenerateCards().ToArray();
        foreach (CardData card in p0Cards)
        {
            allCardData.Add(card);
        }
        p0State.Value =  PlayerState.CreateNew(p0Cards.Select(x => x.cardId), initialHandSize, rng);
        
        var p1Cards = GenerateCards().ToArray();
        foreach (CardData card in p1Cards)
        {
            allCardData.Add(card);
        }
        p1State.Value =  PlayerState.CreateNew(p1Cards.Select(x => x.cardId), initialHandSize, rng);
        
        while (true)
        {
            gamePhase.Value = GamePhase.ChoosingActions;
            Log.Info("choosing actions!");
            
            await UniTask.WaitUntil(() => 
                p0State.Value.ChosenAction != CardId.None && 
                p1State.Value.ChosenAction != CardId.None);
            
            gamePhase.Value = GamePhase.CountingDown;
            Log.Info("counting down!");
            
            await UniTask.Delay(TimeSpan.FromSeconds(countdownTime));
            // var countdownBeginTime = Time.time;
            // float timeSinceCountdownStart = 0;
            // while((timeSinceCountdownStart = Time.time - countdownBeginTime) < countdownTime)
            // {
            //     var secondsRemaining = countdownTime - timeSinceCountdownStart;
            //     GameUIManager.SetCountdown(secondsRemaining);
            //     await UniTask.Yield();
            // }
            
            gamePhase.Value = GamePhase.RevealActions;
            Log.Info("revealing actions!");
            
            await UniTask.Delay(TimeSpan.FromSeconds(handRevealTime));
            
            gamePhase.Value = GamePhase.RevealWinner;
            var winnerMaybe = ForceResolveActions();
            if (winnerMaybe is {} winner)
            {
                Log.Info("Winner: " + winner);
                lastWinner.Value = winner;
                await UniTask.Delay(TimeSpan.FromSeconds(winRevealTime));
                Log.Info("revealing winner!");
                NotifyWinResolvedBroadcastRPC(gamePhase.Value, winner);
                DrawBasedOnWin(winner);
            }
            else
            {
                Log.Error("No winner found!");
                await UniTask.Delay(TimeSpan.FromSeconds(winRevealTime));
            }
        }
    }

    /// <summary>
    /// runs on the server
    /// </summary>
    private CombatWinner? ForceResolveActions()
    {
        if (p0State.Value.ChosenAction == CardId.None ||
            p1State.Value.ChosenAction == CardId.None)
        {
            Debug.LogError("ForceResolveActions called with missing actions!");
            return null;
        }

        var p0CardType = GetCardType(p0State.Value.ChosenAction);
        p0State.Value = p0State.Value.DiscardPlayedCard();
        
        var p1CardType = GetCardType(p1State.Value.ChosenAction);
        p1State.Value = p1State.Value.DiscardPlayedCard();
        

        return GameEnumsExtensions.GetWinner(p0CardType, p1CardType);
    }

    /// <summary>
    /// runs on the server
    /// </summary>
    private void DrawBasedOnWin(CombatWinner winner)
    {
        switch (winner)
        {
            case CombatWinner.Player0:
                if(drawOnWin) p0State.Value = p0State.Value.DrawCard();
                if(drawOnLose) p1State.Value = p1State.Value.DrawCard();
                break;
            case CombatWinner.Player1:
                if(drawOnLose) p0State.Value = p0State.Value.DrawCard();
                if(drawOnWin) p1State.Value = p1State.Value.DrawCard();
                break;
            case CombatWinner.Draw:
                if(drawOnDraw) p0State.Value = p0State.Value.DrawCard();
                if(drawOnDraw) p1State.Value = p1State.Value.DrawCard();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(winner), winner, null);
        }
    }

    private IEnumerable<CardData> GenerateCards()
    {
        return GenerateCardTypes()
            .Select(x => new CardData
            {
                cardType = x,
                cardId = _cardIdGenerator.Next()
            });
    }
    
    private IEnumerable<PlayerCardType> GenerateCardTypes()
    {
        int cardsOfEach = 4;
        for (int i = 0; i < cardsOfEach; i++)
        {
            yield return PlayerCardType.Rock;
            yield return PlayerCardType.Paper;
            yield return PlayerCardType.Scissors;
        }
    }

    /// <summary>
    /// Notify of a win state. sends over the game phase and winner to all clients, even though we have network variables for this.
    /// This is to account for potential out-of-order sync of these state values with the RPC delivery.
    /// </summary>
    /// <param name="phase"></param>
    /// <param name="winner"></param>
    [Rpc(SendTo.Everyone)]
    private void NotifyWinResolvedBroadcastRPC(GamePhase phase, CombatWinner winner)
    {
        var winState = GetWinState(phase, winner, GetCurrentPlayer());
        onGameResolved.Invoke(winState);
    }
    

    [Rpc(SendTo.Everyone)]
    private void DestroyCardRPC(CardId card, float secondsDelay)
    {
        SingletonLocator<ICardRegistry>.Instance.DestroyCard(card, secondsDelay);
    }

    public PlayerCardType GetCardType(CardId forId)
    {
        return allCardData.AsEnumerable()
            .SingleOrDefault(x => x.cardId == forId).cardType;
    }
}