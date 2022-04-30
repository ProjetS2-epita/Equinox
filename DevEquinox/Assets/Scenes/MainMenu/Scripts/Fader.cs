using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class Fader : MonoBehaviour
{
    public float FadeSpeed = 2;
    public bool OnStart = false;
    private bool visible;
    private Button btn;
    [SerializeField] private CanvasGroup ToFade;

    void Start() {
        if (!OnStart) {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClick);
            visible = ToFade.gameObject.activeSelf;
        }
        else StartCoroutine(FadeIn());
    }

    private void OnClick() {
        StopAllCoroutines();
        if (visible) StartCoroutine(FadeOut());
        else StartCoroutine(FadeIn());
        visible = !visible;
    }

    private IEnumerator FadeIn() {
        ToFade.gameObject.SetActive(true);
        ToFade.alpha = 0;
        while (ToFade.alpha < 1) {
            ToFade.alpha += FadeSpeed * Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOut() {
        while (ToFade.alpha > 0) {
            ToFade.alpha -= FadeSpeed * Time.deltaTime;
            yield return null;
        }
        ToFade.gameObject.SetActive(false);
    }

}
