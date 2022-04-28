using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PodController : MonoBehaviour
{
    private Animator _animator;
    private int opened;

    void Start() {
        _animator = GetComponent<Animator>();
        opened = Animator.StringToHash("Open");
    }


    private void OnTriggerEnter(Collider other) {
        OpenPod();
    }

    private void OnTriggerExit(Collider other) {
        ClosePod();
    }

    public void OpenPod() {
        _animator.SetBool(opened, true);
    }

    public void ClosePod() {
        _animator.SetBool(opened, false);
    }

}
