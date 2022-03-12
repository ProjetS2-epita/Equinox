using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoller : MonoBehaviour
{

    public void ApplyForce(Vector3 point, Vector3 impactForce)
    {
        Rigidbody[] RBs = GetComponentsInChildren<Rigidbody>();
        Rigidbody impactPoint = RBs[0];
        float closestPoint = Vector3.Distance(RBs[0].ClosestPointOnBounds(point), point);
        for (int i = 1; i < RBs.Length; i++) {
            float tempPoint = Vector3.Distance(RBs[i].ClosestPointOnBounds(point), point);
            if (tempPoint < closestPoint) {
                closestPoint = tempPoint;
                impactPoint = RBs[i];
            }
        }
        impactPoint.AddExplosionForce(impactForce.x, point, 1f, impactForce.y, ForceMode.Impulse);
        enabled = false;
    }
}
