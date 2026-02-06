using UnityEngine;

public class CoinPickupEvil : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Destroy(gameObject);
    }
}