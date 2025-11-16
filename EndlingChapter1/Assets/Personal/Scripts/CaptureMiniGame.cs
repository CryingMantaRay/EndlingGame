using System.Collections.Generic;
// CaptureMiniGame.cs
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using TMPro;

public class CaptureMiniGame : MonoBehaviour
{
    public Grid grid;
    public PlayerController player;
    public Health playerHealth;
    public NumberCounter scoreCounter;
    public CaptureHandMiniGame handPrefab;
    public int maxHands = 5;
    public int maxScoreToWin = 99;
    public Vector2 handSpawnIntervalRange = new Vector2(3f, 5f);
    public float destroyDelay = 1.5f;
    public SpriteRenderer ggNet;
    public Vector3 normalGGNetScale = new Vector3(1.32f, 1.32f, 1.32f);
    public Vector3 hitGGNetScale = new Vector3(1.1f, 1.1f, 1.1f);
    public SpriteRenderer throphyRenderer;
    public SpriteRenderer gameoverRenderer;
    public TMP_Text gameoverText;

    public UnityEvent OnSmackNet;
    public UnityEvent OnFinishGame;

    int currentHands;
    Coroutine spawnRoutine;

    void Start()
    {
        throphyRenderer.gameObject.SetActive(false);
        gameoverRenderer.gameObject.SetActive(false);
        gameoverText.gameObject.SetActive(false);

        if (playerHealth != null)
        {
            playerHealth.OnDeath.AddListener(() =>
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;

                StartCoroutine(GameOverRoutine());
            });
        }

        OnSmackNet?.AddListener(() =>
        {
            scoreCounter.ChangeBy(1);
        });

        scoreCounter.OnNumberChanged.AddListener(newScore =>
        {
            if (newScore > maxScoreToWin - 1)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;

                StartCoroutine(DelayedWinStart());
            }
        });

        OnFinishGame?.AddListener(() =>
        {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }

    public void StartGame()
    {
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnHandsLoop());
    } 

    IEnumerator DelayedWinStart()
    {
        yield return new WaitForSeconds(2f);

        if (ggNet != null)
        {
            ggNet.gameObject.SetActive(true);
            ggNet.transform.localScale = normalGGNetScale;

            ggNet.transform.DOScale(hitGGNetScale, 0.2f).OnComplete(() =>
            {
                StartCoroutine(WinRoutine());
            });
        }
    }

    IEnumerator WinRoutine()
    {
        yield return new WaitForSeconds(2);
        ggNet.gameObject.SetActive(false);
        player.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // Fade in the trophy renderer color transparent to opaque
        Color baseColor = throphyRenderer.color;
        baseColor.a = 0f;
        throphyRenderer.color = baseColor;

        throphyRenderer.gameObject.SetActive(true);

        float alpha = 0f;
        bool isWaitingToFinish = false;

        DOTween.To(() => alpha, x =>
        {
            alpha = x;

            Color sc = throphyRenderer.color;
            sc.a = alpha;
            throphyRenderer.color = sc;

        }, 1f, 1f).onComplete += () =>
        {
            isWaitingToFinish = true;
        };

        while (!isWaitingToFinish)
            yield return null;

        yield return new WaitForSeconds(2);

        OnFinishGame?.Invoke();
    }

    IEnumerator GameOverRoutine()
    {
        player.gameObject.SetActive(false);
        yield return new WaitForSeconds(2);

        // Initialize colors fully transparent
        Color spriteColor = gameoverRenderer.color;
        spriteColor.a = 0f;
        gameoverRenderer.color = spriteColor;
        gameoverRenderer.gameObject.SetActive(true);

        Color textColor = gameoverText.color;
        textColor.a = 0f;
        gameoverText.color = textColor;
        gameoverText.gameObject.SetActive(true);

        float alpha = 0f;
        bool isWaitingToFinish = false;

        DOTween.To(() => alpha, x =>
        {
            alpha = x;

            Color sc = gameoverRenderer.color;
            sc.a = alpha;
            gameoverRenderer.color = sc;

            Color tc = gameoverText.color;
            tc.a = alpha;
            gameoverText.color = tc;
        }, 1f, 1f).onComplete += () =>
        {
            isWaitingToFinish = true;
        };

        while (!isWaitingToFinish)
            yield return null;

        yield return new WaitForSeconds(2);

        OnFinishGame?.Invoke();
    }


    IEnumerator SpawnHandsLoop()
    {
        while (true)
        {
            if (currentHands < maxHands)
                SpawnHand();

            float wait = Random.Range(handSpawnIntervalRange.x, handSpawnIntervalRange.y);
            yield return new WaitForSeconds(wait);
        }
    }

    void SpawnHand()
    {
        if (handPrefab == null || grid == null)
            return;

        CaptureHandMiniGame handInstance = Instantiate(handPrefab, transform);
        handInstance.Init(grid, player);

        currentHands++;

        handInstance.TeleportToRandomLocation(() =>
        {
            OnSmackNet?.Invoke();
            StartCoroutine(DelayedDestroy(handInstance));
        });
    }

    IEnumerator DelayedDestroy(CaptureHandMiniGame hand)
    {
        yield return new WaitForSeconds(destroyDelay);

        if (hand != null)
        {
            hand.ReleaseOccupiedCells();
            Destroy(hand.gameObject);
        }

        currentHands--;
    }
}
