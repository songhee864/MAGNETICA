using UnityEngine;

public class MagnetGround : MonoBehaviour
{
    public Polarity tilePolarity = Polarity.N;

    public Color nColor = new Color(0.6f, 0.6f, 1f);
    public Color sColor = new Color(1f, 0.6f, 0.6f);

    [Header("Lane Info")]
    public int laneIndex = 0;  //0 = 아래, 1 = 위
    public int nextLaneIndex = 0; //이 타일 끝나고 플레이어가 가야 할 레인

    [Header("Next Tile Info")]
    public MagnetGround nextTile;  //이 타일 다음에 나올 타일

    SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        UpdateColor();
    }

    public void SetRandomPolarity()
    {
        tilePolarity = (Random.value > 0.5f) ? Polarity.N : Polarity.S;
        UpdateColor();
    }

    public void UpdateColor()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.color = (tilePolarity == Polarity.N) ? nColor : sColor;
    }
}
