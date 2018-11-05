using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public abstract class ConditionalMusicLayer : MonoBehaviour
    {
        public AudioClip layer;
        public AudioSource source;
        public float fade = 0.05f;
        public float cutoffThreshold = 0.05f;
        public float maxVolume = 1;
        public float minVolume = 0;
        public bool forceSilence = false;

        public virtual void Awake()
        {
            if (source == null)
                source = GetComponent<AudioSource>();

            source.volume = minVolume;
            source.clip = layer;

            if (forceSilence == false)
                StartCoroutine(CheckCondition());
        }

        public virtual IEnumerator CheckCondition()
        {
            while(forceSilence == false)
            {
                if (source.isPlaying && Condition() == false)
                {
                    /*while (source.volume > cutoffThreshold && Condition() == false)
                    {
                        source.volume -= fade;
                        yield return null;
                    }*/
                    source.volume -= fade;

                    if (source.volume <= cutoffThreshold)
                    {
                        source.volume = minVolume;
                        source.Stop();
                    }                    

                }else if (Condition() == true)
                {
                    //Debug.Log("Condition is true, trying to play track!");
                    if(source.isPlaying == false)
                    {
                        source.timeSamples = AudioManager.instance.activeBGM.timeSamples;
                        source.Play();
                    }

                    if(source.volume < maxVolume)
                        source.volume += fade;

                   /* while (source.volume < maxVolume && Condition() == true)
                    {
                        
                    }*/
                }

                yield return null;
            }
        }

        public virtual bool Condition()
        {
            if (layer == null && source == null)
                return false;

            return true;
        }

        public virtual void Initialize()
        {
            forceSilence = false;
            StartCoroutine(CheckCondition());
        }

        public virtual void ForceSilence()
        {
            forceSilence = true;
            source.Stop();
            source.volume = minVolume;
        }
    }
}


