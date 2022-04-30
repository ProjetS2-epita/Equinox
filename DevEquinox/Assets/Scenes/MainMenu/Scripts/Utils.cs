
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Utils : MonoBehaviour
{
    public TextMeshProUGUI date;
    public TextMeshProUGUI valueText;
    private Slider slider;

    private void Start() {
        enabled = date != null;
        if (enabled) return;
        slider = GetComponent<Slider>();
        if (slider == null) return;
        valueText.text = $"{AudioListener.volume * 100} %";
        slider.value = AudioListener.volume;
    }

    public void OnSliderChanged(float value) {
        valueText.text = $"{value} %";
        AudioListener.volume = value / 100;
    }


    private void Update() => date.text = currentTime();
    private string currentTime() => System.DateTime.UtcNow.ToLocalTime().ToString("M/d/2XXX     hh:mm:ss tt");
    public void setFullscreen(bool isFullScreen) => Screen.fullScreen = isFullScreen;
    public void Quit() => Application.Quit();

}
