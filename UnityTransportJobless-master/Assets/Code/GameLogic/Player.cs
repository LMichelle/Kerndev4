using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KernDev.GameLogic
{    
    public class Player
    {
        public int CurrentHP { get; private set; }
        public Node CurrentNode { get; set; }
        public int TreasureAmount { get; private set; }

        private int startHP;

        public void SetStartHP(int amount)
        {
            startHP = amount;
            CurrentHP = startHP;
        }

        public void TakeDamage(int amount)
        {
            CurrentHP -= amount;
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                // player dies
            }
        }

        public void Heal(int amount)
        {
            CurrentHP += amount;
            if (CurrentHP > startHP)
                CurrentHP = startHP;
        }

    }
}
