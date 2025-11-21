using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public enum GameState
{
    Ready,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public PlayerController player;
    public Transform playerStartPoint;  //없으면 Player 시작 위치 그대로 사용

    [Header("UI")]
    public GameObject startPanel;
    public GameObject gameOverPanel;
    public TMP_Text distanceTextInGame; 
    public TMP_Text distanceTextResult;

    GameState state = GameState.Ready;
    float startX;
    float distance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }

        startX = (playerStartPoint != null) ? playerStartPoint.position.x : player.transform.position.x;

        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateDistanceUI(0);

        if (distanceTextInGame != null)
            distanceTextInGame.gameObject.SetActive(false);
    }

    private void Update()
    {
        switch (state)
        {
            case GameState.Ready:
                //아무 키나 누르면 시작
                if (Input.anyKeyDown)
                {
                    StartGame();
                }
                break;

            case GameState.Playing:
                UpdateDistance();
                break;

            case GameState.GameOver:
                break;
        }
    }

    void StartGame()
    {
        state = GameState.Playing;

        if (startPanel != null)
            startPanel.SetActive(false);

        if (distanceTextInGame != null)
            distanceTextInGame.gameObject.SetActive(false);

        if (player != null)
            player.canRun = true;
    }

    void UpdateDistance()
    {
        if (player == null) return;

        float x = player.transform.position.x;
        distance = Mathf.Max(0f, x - startX);

        UpdateDistanceUI(distance);
    }

    void UpdateDistanceUI(float d)
    {
        if (distanceTextInGame != null)
        {
            distanceTextInGame.text = string.Format("{0:0} m", d);
        }
    }

    public void GameOver()
    {
        if (state == GameState.GameOver) return;

        state = GameState.GameOver;

        if (distanceTextResult != null)
        {
            distanceTextResult.text = string.Format("You went {0:0} m", distance);
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (distanceTextInGame != null)
            distanceTextInGame.gameObject.SetActive(false);
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnClickQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
