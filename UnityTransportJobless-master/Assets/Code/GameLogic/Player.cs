using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KernDev.GameLogic
{    
    public class Opponent
    {
        public int CurrentHP { get; internal set; }
        public Node CurrentNode { get; set; }
        public int DamageAmount { get; set; }
        public bool Dead { get; private set; }

        public virtual void SetStartHP(int startHP)
        {
            CurrentHP = startHP;
        }

        public void TakeDamage(int amount)
        {
            amount = Mathf.Abs(amount); // I want to make sure amount is never a -x, so we won't suddenly get a healing effect due to the variance
            CurrentHP -= amount;
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                Dead = true;
            }
        }
    }

    public class Player : Opponent
    {
        public int TreasureAmount { get; set; }

        private int startHP;

        public override void SetStartHP(int startHP)
        {      
            base.SetStartHP(startHP);
            this.startHP = startHP;
        }


        public void Heal(int amount)
        {
            CurrentHP += amount;
            if (CurrentHP > startHP)
                CurrentHP = startHP;
        }

    }
}
