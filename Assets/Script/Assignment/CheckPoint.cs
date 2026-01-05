using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Spawn Point for this Checkpoint")]
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckPointManager.Instance.SetLastCheckpoint(this);
            Debug.Log("Checkpoint reached: " + gameObject.name);
        }
    }
}
