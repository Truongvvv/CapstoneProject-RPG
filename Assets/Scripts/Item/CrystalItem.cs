using UnityEngine;

public class CrystalItem : MonoBehaviour
{
    public string crystalID = "Crystal"; // ID item, có thể mở rộng cho nhiều loại

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Báo cho QuestManager là player nhặt crystal
            PlayerQuestManager.Instance.CollectCrystal(crystalID);

            // Xoá item trên map
            Destroy(gameObject);
        }
    }
}