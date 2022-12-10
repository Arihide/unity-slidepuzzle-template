using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : Singleton<GameManager>
{
    [SerializeField] private Puzzle m_puzzlePrefab;
    [SerializeField] private GameObject m_title;
    [SerializeField] private GameObject m_game;
    [SerializeField] private GameObject m_completed;
    [SerializeField] private AudioSource m_source;
    [SerializeField] private AudioClip m_clear;
    [SerializeField] private Button m_startButton;
    [SerializeField] private Button m_leftButton;
    [SerializeField] private Button m_rightButton;
    [SerializeField] private Button m_homeButton;
    [SerializeField] private Button m_tapToNext;
    [SerializeField] private GestureManager gestureManager;
    [SerializeField] private AudioClip m_moveClip;

    private Puzzle m_puzzle;
    private int size = 3;
    private Vector3 goal = Vector3.zero;

    public const int MinSize = 3;
    public const int MaxSize = 32;

    protected override void Awake()
    {
        base.Awake();

        m_startButton.onClick.AddListener(() =>
        {
            StartGame();
        });
        m_leftButton.onClick.AddListener(() =>
        {
            Move(-1, false);
            RefreshButton();
        });
        m_rightButton.onClick.AddListener(() =>
        {
            Move(1, false);
            RefreshButton();
        });
        m_homeButton.onClick.AddListener(() =>
        {
            Start();
        });
        m_tapToNext.onClick.AddListener(() =>
        {
            Move(1, true);
        });

        RefreshButton();
    }

    private void RefreshButton()
    {
        m_leftButton.gameObject.SetActive(size != MinSize);
        m_rightButton.gameObject.SetActive(size != MaxSize && SlidePuzzleData.GetClearCount(size) > 0);
    }

    // 起動時の状態にする
    public void Start()
    {
        Create();
        RefreshButton();
        RemovePuzzles();

        m_title.SetActive(true);
        m_game.SetActive(false);
        m_completed.SetActive(false);
        gestureManager.enabled = false;
    }

    // ゲームを開始する
    public void StartGame()
    {
        m_game.SetActive(true);
        m_title.SetActive(false);
        m_completed.SetActive(false);
        gestureManager.SetPuzzle(m_puzzle);
        StartCoroutine(m_puzzle.ShuffleSimple(() => gestureManager.enabled = true));
    }

    public void Goal()
    {
        m_source.PlayOneShot(m_clear);
        m_completed.SetActive(true);
        gestureManager.enabled = false;
        SlidePuzzleData.AddClearCount(size);

        int clearCount = SlidePuzzleData.GetClearCount();

        if (clearCount == 2)
        {
            ReviewRequester.Instance.RequestReview();
        }
        else if (clearCount >= 3)
        {
            Admob.Instance.RequestInterstitial();
        }
    }

    // 次ステージに遷移する
    public void Move(int sign, bool start)
    {
        // 最小：一辺3
        size = Mathf.Clamp(size + sign, MinSize, MaxSize);
        m_completed.SetActive(false);

        Create();

        StopAllCoroutines();
        StartCoroutine(MoveCoroutine(sign, start));
        gestureManager.m_source.PlayOneShot(m_moveClip);
    }

    private IEnumerator MoveCoroutine(int sign, bool start)
    {
        const float speed = 0.5f;
        goal += new Vector3(-sign * 0.5f, 0, 0);

        Vector3 o = m_puzzle.transform.localPosition;
        m_puzzle.transform.localPosition -= goal;

        yield return m_puzzle.transform.parent.MoveLocalCoroutine(goal, speed);

        m_puzzle.transform.parent.localPosition = Vector3.zero;
        m_puzzle.transform.localPosition = o;

        goal.Set(0, 0, 0);

        RemovePuzzles();

        if (start) StartGame();
    }

    private void Create()
    {
        m_puzzle = Instantiate<Puzzle>(m_puzzlePrefab, gestureManager.transform);
        m_puzzle.onCleared.AddListener(Goal);
        m_puzzle.CreateSquare(size);
    }

    private void RemovePuzzles()
    {
        for (var i = gestureManager.transform.childCount - 1; i >= 0; i--)
        {
            var p = gestureManager.transform.GetChild(i).GetComponent<Puzzle>();
            if (p != m_puzzle)
                Destroy(p.gameObject);
        }
    }

}