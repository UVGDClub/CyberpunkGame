using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoOptions : MonoBehaviour {

    public Slider vsyncSlider;

    public void ToggleVsync()
    {
        QualitySettings.vSyncCount = (int)vsyncSlider.value;
    }

}
