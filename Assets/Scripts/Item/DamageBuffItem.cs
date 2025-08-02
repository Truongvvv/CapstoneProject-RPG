using UnityEngine;

public class DamageBuffItem : MonoBehaviour
{
    public float buffAmount = 20f;
    public float duration = 10f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.ApplyDamageBuff(20f, 10f);
                Destroy(gameObject);
            }
        }
        if (other.CompareTag("Player"))
        {
            PlayerCombat combat = other.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.ApplyDamageBuff(buffAmount, duration);
                Destroy(gameObject);
            }
        }
    }
}