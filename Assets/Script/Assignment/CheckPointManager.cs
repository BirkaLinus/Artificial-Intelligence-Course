using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    public static CheckPointManager Instance { get; private set; }

    [Header("Respawn Logics")]
    [SerializeField] float fRespawnTimer;
    [SerializeField] Checkpoint _lastCheckPoint;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetLastCheckpoint(Checkpoint checkpoint)
    {
        _lastCheckPoint = checkpoint;
    }

    public Transform GetLatestSpawnPoint()
    {
        if (_lastCheckPoint != null && _lastCheckPoint.spawnPoint != null) return _lastCheckPoint.spawnPoint;
        return null;
    }

    public void RespawnPlayer(GameObject player)
    {
        StartCoroutine(RespawnWithDelay(player));
    }

    private IEnumerator RespawnWithDelay(GameObject player)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        Transform spawn = GetLatestSpawnPoint();

        if (rb == null || spawn == null)
            yield break;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        //Moving the player
        player.transform.SetPositionAndRotation(spawn.position,Quaternion.Euler(0f, 90f, 0f));

        //Disable the player (and the camera)...
        player.SetActive(false);

        yield return new WaitForSeconds(fRespawnTimer);//respawntimer for when to actually spawn.

        //Enables the player again.
        player.SetActive(true);
        rb.isKinematic = false;
    }



}
