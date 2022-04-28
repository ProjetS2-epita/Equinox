using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameStateManager : MonoBehaviour
{

    [SerializeField] private Image DeathBackground;
    [SerializeField] private TextMeshProUGUI DeathText;

    public static GameStateManager Instance;
    

    private void Awake() {
        Instance = this;
        DeathBackground.CrossFadeAlpha(0f, 0f, true);
        DeathText.CrossFadeAlpha(0f, 0f, true);
    }

    public void DeathScreen() {
        StartCoroutine(FadeDeathScreen());
    }

    private IEnumerator FadeDeathScreen() {
        DeathBackground.CrossFadeAlpha(1f, 3f, false);
        yield return new WaitForSeconds(3f);
        DeathText.CrossFadeAlpha(1f, 2f, false);
    }







}
