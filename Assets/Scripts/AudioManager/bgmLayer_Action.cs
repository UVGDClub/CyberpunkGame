using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class bgmLayer_Action : ConditionalMusicLayer
    {
        public Player player;
        public KeyCode actionKey;
        public float timeThreshold;
        float timeStamp;

        public override bool Condition()
        {
            if (base.Condition() == false || player.initialized == false)
                return false;

            if (Input.GetKey(actionKey))
            {
                timeStamp = Time.time;
                return true;
            }
            else if (timeStamp + timeThreshold > Time.time)
                return true;

            return false;
        }
    }
}

