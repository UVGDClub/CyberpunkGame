using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public AudioSource active_source;
    public AudioSource inactive_source;
    bool crossfading = false;

    private void Awake()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.levelgrid.CrossfadeBGM += CrossFade;
        }
    }

    public void CrossFade(AudioClip clip, float fade_rate)
    {
        StartCoroutine(ICrossFade(clip, fade_rate));
    }

    IEnumerator ICrossFade(AudioClip clip, float fade_rate)
    {
        while(crossfading == true)
        {
            yield return null;
        }

        crossfading = true;

        inactive_source.PlayOneShot(clip);

        while(active_source.volume > 0.1f && fade_rate > 0)
        {
            active_source.volume -= fade_rate;
            inactive_source.volume += fade_rate;

            yield return null;
        }

        active_source.volume = 0;

        AudioSource temp = active_source;
        active_source = inactive_source;
        inactive_source = temp;

        crossfading = false;
    }

}
