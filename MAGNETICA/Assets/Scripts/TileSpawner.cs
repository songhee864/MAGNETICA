using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public GameObject tilePrefab;

    [Header("Lane Settings")]
    public float bottomY = -1f;  //아래 레인 Y
    public float topY = 2f;  //위 레인 Y
    public float tileSpacing = 5f;  //타일 간 X 간격

    [Header("Spawn Settings")]
    public int initialTileCount = 15;  //처음 깔리는 타일 개수
    public float spawnDistanceAhead = 25f;  //플레이어 앞 몇 유닛까지 미리 생성할지
    public float removeDistanceBehind = 20f;  //플레이어 뒤 몇 유닛 지나면 삭제할지

    float nextSpawnX;
    List<GameObject> spawnedTiles = new List<GameObject>();

    MagnetGround lastTile = null;  //직전에 만든 타일
    bool isFirstTile = true;  //첫 타일은 무조건 아래 레인

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (player == null || tilePrefab == null)
        {
            Debug.LogError("TileSpawner: player 또는 tilePrefab이 비어 있음!");
            enabled = false;
            return;
        }

        //첫 생성 위치. 플레이어 조금 앞에서 시작
        nextSpawnX = player.transform.position.x;

        //시작 타일들 쫙 생성
        for (int i = 0; i < initialTileCount; i++)
        {
            SpawnTile();
        }
    }

    private void Update()
    {
        if (player == null) return;

        //플레이어 앞쪽에 타일이 충분히 있는지 확인하면서 계속 생성
        while (nextSpawnX < player.transform.position.x + spawnDistanceAhead)
        {
            SpawnTile();
        }

        //너무 뒤에 있는 타일은 삭제
        for (int i = spawnedTiles.Count - 1; i >= 0; i--)
        {
            if (spawnedTiles[i] == null)
            {
                spawnedTiles.RemoveAt(i);
                continue;
            }

            if (spawnedTiles[i].transform.position.x < player.transform.position.x - removeDistanceBehind)
            {
                Destroy(spawnedTiles[i]);
                spawnedTiles.RemoveAt(i);
            }
        }
    }

    void SpawnTile()
    {
        //X 간격 고정
        nextSpawnX += tileSpacing;

        //이번 타일이 어느 레인에 나올지 결정
        int lane;
        if (isFirstTile)
        {
            lane = 0;          //첫 타일은 무조건 아래 레인
            isFirstTile = false;
        }
        else
        {
            //이후부터는 위/아래 랜덤
            lane = (Random.value > 0.5f) ? 1 : 0;
        }

        float y = (lane == 0) ? bottomY : topY;
        Vector3 pos = new Vector3(nextSpawnX, y, 0f);

        GameObject obj = Instantiate(tilePrefab, pos, Quaternion.identity);
        spawnedTiles.Add(obj);

        MagnetGround mg = obj.GetComponent<MagnetGround>();
        if (mg != null)
        {
            //자성 랜덤
            mg.tilePolarity = (Random.value > 0.5f) ? Polarity.N : Polarity.S;
            mg.laneIndex = lane;

            //직전 타일 기준으로 "다음 레인 + 다음 타일" 정보 연결
            if (lastTile != null)
            {
                lastTile.nextLaneIndex = lane;  //다음에 갈 레인 인덱스
                lastTile.nextTile = mg;  //다음 타일 객체
            }
        }

        //이번 타일을 lastTile로 저장
        lastTile = mg;
    }
}
