using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Audio
{

    public class AudioManager : MonoBehaviour
    {

        public static AudioManager instance = null;

        public AudioSource sfx;
        public AudioSource activeBGM;
        public AudioSource inactiveBGM;
        bool crossfading = false;
        public float cutoffThreshold = 0.05f;

        //Quick and dirty testing UI
        public Slider bgmVolume;
        public Slider sfxVolume;

        private void Awake()
        {
            if (instance != null)
                Destroy(this);

            instance = this;

            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                player.levelgrid.CrossfadeBGM += CrossFade;
            }
        }

        public void CrossFade(LevelAudioSettings las)
        {
            if (crossfading)
                return;

            StartCoroutine(ICrossFade(las));
        }

        IEnumerator ICrossFade(LevelAudioSettings las)
        {
            crossfading = true;
            inactiveBGM.clip = las.clip;
            inactiveBGM.Play();
            inactiveBGM.pitch = las.pitch;

            while (activeBGM.volume > cutoffThreshold && las.fadeRate > 0)
            {
                activeBGM.volume -= las.fadeRate;
                inactiveBGM.volume += las.fadeRate;

                yield return null;
            }

            inactiveBGM.volume = 1;
            activeBGM.volume = 0;
            activeBGM.Stop();
            AudioSource temp = activeBGM;
            activeBGM = inactiveBGM;
            inactiveBGM = temp;

            crossfading = false;
        }

        public void PlaySFX(AudioClip clip)
        {
            sfx.PlayOneShot(clip);
        }

        public void Pause()
        {
            activeBGM.Pause();
            inactiveBGM.Pause();
            sfx.Pause();
        }

        public void UnPause()
        {
            activeBGM.UnPause();
            inactiveBGM.UnPause();
        }

        public void SetVolumeBGM()
        {
            activeBGM.volume = bgmVolume.value;
            inactiveBGM.volume = bgmVolume.value;
        }

        public void SetVolumeSFX()
        {
            sfx.volume = sfxVolume.value;
        }

    }

    [System.Serializable]
    public struct LevelAudioSettings
    {
        public AudioClip clip;
        public float fadeRate;
        public float pitch;
    }
}

