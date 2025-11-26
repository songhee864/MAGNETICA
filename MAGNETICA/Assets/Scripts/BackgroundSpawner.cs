using System.Collections.Generic;
using UnityEngine;

public class BackgroundSpawner : MonoBehaviour
{
    public GameObject backgroundPrefab;   // 생성할 배경 프리팹
    public Vector3 startPosition;         // 첫 배경 시작 위치
    public float spawnInterval = 3f;      // 새 배경 생성 간격 시간
    private float nextSpawnTime = 0f;
    private Vector3 nextPosition;         // 다음 배경 생성 위치

    private List<GameObject> backgrounds = new List<GameObject>(); // 생성된 배경 리스트

    void Start()
    {
        // 첫 배경 생성
        nextPosition = startPosition;
        GameObject firstBG = Instantiate(backgroundPrefab, nextPosition, Quaternion.identity);
        backgrounds.Add(firstBG);

        // 다음 배경 위치 갱신
        nextPosition.x += 115f;

        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (nextPosition.x > 3750f) return;

        if (Time.time >= nextSpawnTime)
        {
            // 새 배경 생성
            GameObject newBG = Instantiate(backgroundPrefab, nextPosition, Quaternion.identity);
            backgrounds.Add(newBG);

            // 다음 배경 위치 갱신
            nextPosition.x += 115f;

            // 다음 생성 시간 갱신
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
}
