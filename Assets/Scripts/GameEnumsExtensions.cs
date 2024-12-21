public static class GameEnumsExtensions{

    public static bool AllowsChangeAction(this GamePhase phase)
    {
        return phase is
            GamePhase.ChoosingActions or
            GamePhase.CountingDown;
    }
    
    public static CombatWinner GetWinner(CombatAction p0, CombatAction p1)
    {
        return (p0, p1) switch
        {
            (CombatAction.Scissors, CombatAction.Paper) => CombatWinner.Player0,
            (CombatAction.Rock, CombatAction.Scissors) => CombatWinner.Player0,
            (CombatAction.Paper, CombatAction.Rock) => CombatWinner.Player0,
            (CombatAction.Paper, CombatAction.Scissors) => CombatWinner.Player1,
            (CombatAction.Scissors, CombatAction.Rock) => CombatWinner.Player1,
            (CombatAction.Rock, CombatAction.Paper) => CombatWinner.Player1,
            
            _ => CombatWinner.Draw
        };
    }
    public static CombatWinner GetWinner(PlayerCardType p0, PlayerCardType p1)
    {
        return (p0, p1) switch
        {
            (PlayerCardType.Scissors, PlayerCardType.Paper) => CombatWinner.Player0,
            (PlayerCardType.Rock, PlayerCardType.Scissors) => CombatWinner.Player0,
            (PlayerCardType.Paper, PlayerCardType.Rock) => CombatWinner.Player0,
            (PlayerCardType.Paper, PlayerCardType.Scissors) => CombatWinner.Player1,
            (PlayerCardType.Scissors, PlayerCardType.Rock) => CombatWinner.Player1,
            (PlayerCardType.Rock, PlayerCardType.Paper) => CombatWinner.Player1,
            
            _ => CombatWinner.Draw
        };
    }
}