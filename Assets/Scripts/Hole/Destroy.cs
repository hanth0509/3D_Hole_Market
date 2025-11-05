using UnityEngine;
public class Destroy : MonoBehaviour
{
     private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Object"))
        {
            Destroy(other.gameObject);
            Manager.Instance.AddScore(1);
        }
    }
}
