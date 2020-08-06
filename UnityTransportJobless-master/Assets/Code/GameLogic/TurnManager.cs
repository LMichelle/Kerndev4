using UnityEngine;

namespace KernDev.GameLogic
{
    public class TurnManager
    {
        public int Turn { get; set; } // Start at player amount, next turn will do modulo and turn will be zero the first time.
        public int FormerAmountOfPlayers { get; set; }

        public int NextTurn(int amountOfPlayers)
        {
            if (FormerAmountOfPlayers == amountOfPlayers)
                Turn++;
            Turn = Turn % amountOfPlayers;
            FormerAmountOfPlayers = amountOfPlayers;
            return Turn;
        }
    }
}
