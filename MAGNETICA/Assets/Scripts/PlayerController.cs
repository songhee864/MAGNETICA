using UnityEngine;

public enum Polarity { N, S }

public class PlayerController : MonoBehaviour
{
    [Header("Move Settings")]
    public float runSpeed = 5f;

    [Header("Lane Settings")]
    public float laneBottomY = -1f;
    public float laneTopY = 2f;
    public float laneJumpDuration = 0.25f;  //위/아래로 이동하는 시간
    public float preJumpOffset = 0.1f;  //타일 오른쪽 끝에서 조금 앞에서 점프

    [Header("Hop Settings")]
    public float hopHeight = 0.5f;  //같은 레인에서 통통 튀는 높이
    public float hopDuration = 0.2f;  //한 번 튀는 데 걸리는 시간

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color nColor = Color.blue;
    public Color sColor = Color.red;

    [Header("State")]
    public Polarity currentPolarity = Polarity.N;
    public bool isAlive = true;
    public bool canRun = false;

    Rigidbody2D rb;

    //레인 상태
    int currentLaneIndex = 0;  // 0 = 아래, 1 = 위
    bool isLaneJumping = false;
    float laneJumpTimer = 0f;
    float laneJumpStartY;
    float laneJumpTargetY;
    int laneJumpTargetIndex = 0;  //목표 레인 인덱스
    float currentLaneJumpDuration;  //이번 레인 점프에 쓸 실제 시간

    //같은 레인 안에서 통통 튀는 점프 상태
    bool isHopping = false;
    float hopTimer = 0f;

    //현재 밟고 있는 타일 정보
    MagnetGround currentTile = null;
    bool hasJumpedForThisTile = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        UpdateColor();

        if (rb != null)
        {
            rb.gravityScale = 0f;  //중력 사용 X
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        //시작은 아래 레인에 붙이기
        Vector3 pos = transform.position;
        pos.y = laneBottomY;
        transform.position = pos;
        currentLaneIndex = 0;
    }

    private void Update()
    {
        if (!isAlive) return;
        if (!canRun) return;

        //X방향 자동 이동
        transform.Translate(Vector2.right * runSpeed * Time.deltaTime);

        //현재 레인의 기준 Y (아래/위)
        float baseY = (currentLaneIndex == 0) ? laneBottomY : laneTopY;
        Vector3 pos = transform.position;

        //1순위. 레인 변경 점프 중이면, 레인 사이를 보간
        if (isLaneJumping)
        {
            hopTimer = 0f;  //레인 점프 중엔 호핑 초기화
            isHopping = false;

            laneJumpTimer += Time.deltaTime;
            float t = Mathf.Clamp01(
                currentLaneJumpDuration > 0f
                    ? laneJumpTimer / currentLaneJumpDuration
                    : 1f
            );

            float newY = Mathf.Lerp(laneJumpStartY, laneJumpTargetY, t);
            pos.y = newY;
            transform.position = pos;

            if (t >= 1f)
            {
                isLaneJumping = false;
                currentLaneIndex = laneJumpTargetIndex;  //목표 레인으로 확실하게 변경
            }
        }
        //2순위. 같은 레인에서 통통 튀는 연출
        else if (isHopping)
        {
            hopTimer += Time.deltaTime;
            float t = Mathf.Clamp01(hopTimer / hopDuration);

            float curve = 4f * t * (1f - t);
            float offsetY = hopHeight * curve;

            //아래 레인(0)에서는 위로 튀고, 위 레인(1)에서는 아래로 튀게
            float signedOffset = (currentLaneIndex == 0)
                ? +offsetY  //아래 레인: 위로 점프
                : -offsetY;  //위 레인: 아래로 점프(거꾸로 튀는 느낌)

            pos.y = baseY + signedOffset;
            transform.position = pos;

            if (t >= 1f)
            {
                isHopping = false;
                pos.y = baseY;  //레인 기준 Y로 다시 스냅
                transform.position = pos;
            }
        }


        //타일 끝에 도달했으면, 해당 타일의 nextLaneIndex로 점프
        HandleTileEndAutoJump();

        //자성 전환 (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePolarity();
        }
    }

    void HandleTileEndAutoJump()
    {
        if (currentTile == null) return;
        if (hasJumpedForThisTile) return;

        Collider2D col = currentTile.GetComponent<Collider2D>();
        if (col == null) return;

        float tileRightX = col.bounds.max.x;

        if (transform.position.x >= tileRightX - preJumpOffset)
        {
            int nextLane = currentTile.nextLaneIndex;

            if (nextLane != currentLaneIndex)
            {
                //현재 타일 정보도 같이 넘김
                StartLaneJump(nextLane, currentTile);
            }
            else
            {
                Jump();  //같은 레인: 그냥 통통 튀는 점프
            }

            hasJumpedForThisTile = true;
        }
    }


    //같은 레인에서 통통 튀는 점프 시작
    void Jump()
    {
        if (isLaneJumping) return;  //레인 변경 중이면 무시

        isHopping = true;
        hopTimer = 0f;
    }

    //레인 변경 점프 시작
    void StartLaneJump(int targetLaneIndex, MagnetGround fromTile)
    {
        isLaneJumping = true;
        laneJumpTimer = 0f;
        laneJumpStartY = transform.position.y;

        laneJumpTargetIndex = targetLaneIndex;
        laneJumpTargetY = (targetLaneIndex == 0) ? laneBottomY : laneTopY;

        currentLaneJumpDuration = laneJumpDuration;

        if (fromTile != null && fromTile.nextTile != null)
        {
            Collider2D nextCol = fromTile.nextTile.GetComponent<Collider2D>();
            if (nextCol != null)
            {
                float destX = nextCol.bounds.center.x;
                float distX = destX - transform.position.x; 

                if (distX < 0.1f) distX = 0.1f;

                currentLaneJumpDuration = distX / runSpeed;
            }
        }
    }


    void TogglePolarity()
    {
        //자성 전환
        currentPolarity = (currentPolarity == Polarity.N) ? Polarity.S : Polarity.N;
        UpdateColor();

        //타일 위에 서 있는 상태에서 자성 바꾸면 즉시 재검사
        if (currentTile != null && currentTile.tilePolarity != currentPolarity)
        {
            Die();
        }
    }

    void UpdateColor()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.color = (currentPolarity == Polarity.N) ? nColor : sColor;
    }

    public void Die()
    {
        if (!isAlive) return;
        isAlive = false;
        canRun = false;

        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
    }

    //타일 밟았을 때(자성 체크 + 현재 타일 갱신)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        MagnetGround tile = collision.collider.GetComponent<MagnetGround>();
        if (tile != null)
        {
            //자성 틀리면 죽음
            if (tile.tilePolarity != currentPolarity)
            {
                Die();
                return;
            }

            //자성 맞으면 이 타일이 현재 타일
            currentTile = tile;
            hasJumpedForThisTile = false;

            //이 타일 레인으로 Y 위치 스냅
            currentLaneIndex = tile.laneIndex;
            Vector3 pos = transform.position;
            pos.y = (currentLaneIndex == 0) ? laneBottomY : laneTopY;
            transform.position = pos;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (currentTile != null && collision.collider.GetComponent<MagnetGround>() == currentTile)
        {
            //currentTile = null;
        }
    }
}
