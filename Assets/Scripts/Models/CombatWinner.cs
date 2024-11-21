public enum CombatWinner
{
    Player0,
    Player1,
    Draw
}

public static class CombatWinnerExtensions
{
    public static CombatPlayer? TryToPlayer(this CombatWinner winner)
    {
        return winner switch
        {
            CombatWinner.Player0 => CombatPlayer.Player0,
            CombatWinner.Player1 => CombatPlayer.Player1,
            _ => null
        };
    }
}