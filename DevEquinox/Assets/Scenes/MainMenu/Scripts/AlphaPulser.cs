using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AlphaPulser : MonoBehaviour
{
    private Image dot;
    public bool Synchronize = true;
    public float Frequency = -1f;
    public float BaseAlpha = -1;
    public float Shift = -1f;
    public float Delay = -1f;
    private WaitForSeconds delayWait;
    private WaitForSeconds interWait;

    void Start()
    {
        dot = GetComponent<Image>();
        dot.canvasRenderer.SetAlpha(BaseAlpha);
        delayWait = new WaitForSeconds(Delay);
        interWait = new WaitForSeconds(Frequency / 2);
        Invoke("InvokePulse", Shift);
    }

    private void InvokePulse() => StartCoroutine(Pulse());
    private IEnumerator Pulse() {
        dot.CrossFadeAlpha(1f, Frequency / 2, true);
        yield return interWait;
        dot.CrossFadeAlpha(BaseAlpha, Frequency / 2, true);
        yield return interWait;
        yield return delayWait;
        StartCoroutine(Pulse());
    }

}
