using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DoorMecanism : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            Debug.Log("Entered DoorMecanism detection");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
            Debug.Log("Exited DoorMecanism detection");
    }

}
