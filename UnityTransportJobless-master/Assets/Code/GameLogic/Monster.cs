using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KernDev.GameLogic
{
    public class Monster
    {
        public int CurrentHP { get; private set; }
        public Node CurrentNode { get; set; }

        public int DamageAmount { get; set; }

        public bool Dead { get; private set; }

        public void SetStartHP(int amount)
        {
            CurrentHP = amount;
        }

        public void TakeDamage(int amount)
        {
            CurrentHP -= amount;
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                Dead = true;
            }
        }
    }
}
