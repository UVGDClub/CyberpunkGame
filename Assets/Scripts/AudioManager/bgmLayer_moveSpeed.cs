using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class bgmLayer_moveSpeed : ConditionalMusicLayer
    {
        public Player player;
        public float speedThreshold = 1;

        public override bool Condition()
        {
            if (base.Condition() == false)
                return false;

            return player.rigidbody2d.velocity.magnitude >= speedThreshold;
        }

    }
}