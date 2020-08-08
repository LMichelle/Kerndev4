using UnityEngine;

namespace KernDev.GameLogic
{
    public class TurnManager
    {
        /// <summary>
        /// Start at player amount, next turn will do modulo and turn will be zero the first time.
        /// </summary>
        public int Turn { get; set; } 
        public int FormerAmountOfPlayers { get; set; }

        /// <summary>
        /// Generates the number of the next turn.
        /// </summary>
        /// <param name="amountOfPlayers"> Amount of players that need a turn. </param>
        /// <returns></returns>
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
