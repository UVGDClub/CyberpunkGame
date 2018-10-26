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
        }

        public virtual IEnumerator CheckCondition()
        {
            while(forceSilence == false)
            {
                if (source.isPlaying && Condition() == false)
                {
                    while (source.volume > cutoffThreshold)
                    {
                        source.volume -= fade;
                        yield return null;
                    }
                    source.volume = minVolume;
                    source.Stop();

                }                 
                else if(source.isPlaying == false && Condition() == true)
                {
                    source.timeSamples = AudioManager.instance.activeBGM.timeSamples;
                    source.Play();
                    while(source.volume < maxVolume)
                    {
                        source.volume += fade;
                    }
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


