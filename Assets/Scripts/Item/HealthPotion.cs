using Unity.VisualScripting;
using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    public int amount = 1; // mỗi lần nhặt +1

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory.Instance.AddHealthPotion(amount);
            Destroy(gameObject);
        }
    }
}