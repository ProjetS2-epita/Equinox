using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorController : MonoBehaviour
{
    public float Speed = 1f;
    public Transform TopDest;
    public Transform BottomDest;
    public Transform Plateforme;

    private IEnumerator Translate(bool Up) {
        while (Plateforme.position != (Up ? TopDest : BottomDest).position) {
            Plateforme.position = Vector3.Lerp(Plateforme.position, (Up ? TopDest : BottomDest).position, Speed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other) {
        StopAllCoroutines();
        other.gameObject.transform.SetParent(Plateforme);
        StartCoroutine(Translate(Vector3.Distance(Plateforme.position, TopDest.position) > Vector3.Distance(Plateforme.position, BottomDest.position)));
    }

    private void OnTriggerExit(Collider other) {
        other.gameObject.transform.SetParent(null);
    }
}
