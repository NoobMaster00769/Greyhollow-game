using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Brightness")]
    public Image brightnessOverlay;  // Black fullscreen UI Image
    public Slider brightnessSlider;

    // Brightness alpha limits
    private const float alphaMin = 0.75f;  // darkest
    private const float alphaMax = 0f;     // brightest
    private const float defaultAlpha = 0.25f; // default brightness

    void Start()
    {
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        brightnessSlider.value = Mathf.InverseLerp(alphaMin, alphaMax, defaultAlpha);
        SetBrightness(brightnessSlider.value);        
    }

 
    // BRIGHTNESS
    public void SetBrightness(float sliderValue)
    {
        float alpha = Mathf.Lerp(alphaMin, alphaMax, sliderValue);
        Color c = brightnessOverlay.color;
        c.a = alpha;
        brightnessOverlay.color = c;
    }

}
