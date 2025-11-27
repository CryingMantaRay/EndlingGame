// CaptureHandMiniGame.cs
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class CaptureHandMiniGame : MonoBehaviour
{
    public enum Location
    {
        Up,
        Down,
        Left,
        Right
    }

    public Location location;
    public Transform netTransform;
    public float netAIChance = 1;
    public float netRightOffset = 0.1f;
    public float netMoveSpeed = 5f;
    public float netMoveDelay = 0.4f;
    public float hitSplashDuration = 0.2f;
    public Grid grid;
    public SquareBounds netSquareBounds;
    public SpriteRenderer handRenderer;
    public SpriteRenderer netRenderer;
    public Sprite smackHandSprite;
    public Sprite smackNetSprite;
    public GameObject hitSplashSprite;

    int spawnRow = -1;
    int spawnColumn = -1;
    int targetRow = -1;
    int targetColumn = -1;

    PlayerController player;

    public void Init(Grid grid, PlayerController player)
    {
        hitSplashSprite.SetActive(false);
        this.grid = grid;
        this.player = player;
    }

    // Teleports hand to random (or aimed) unoccupied edge cell and moves net towards target cell
    public void TeleportToRandomLocation(Action onComplete)
    {
        if (grid == null)
        {
            onComplete?.Invoke();
            return;
        }

        int row;
        int column;
        Location loc;

        int playerRow = -1;
        int playerColumn = -1;
        bool hasPlayerCell = false;

        if (player != null)
        {
            hasPlayerCell = grid.TryGetCellFromWorldPosition(player.transform.position, out playerRow, out playerColumn);
        }

        bool useAimedShot = hasPlayerCell && Random.value < netAIChance;

        if (useAimedShot)
        {
            List<(int, int, Location)> candidates = new List<(int, int, Location)>();

            int topRow = grid.rows - 1;
            int bottomRow = 0;
            int leftColumn = 0;
            int rightColumn = grid.columns - 1;

            if (playerColumn >= 0 && playerColumn < grid.columns)
            {
                if (!grid.IsCellOccupied(topRow, playerColumn))
                    candidates.Add((topRow, playerColumn, Location.Up));

                if (!grid.IsCellOccupied(bottomRow, playerColumn))
                    candidates.Add((bottomRow, playerColumn, Location.Down));
            }

            if (playerRow >= 0 && playerRow < grid.rows)
            {
                if (!grid.IsCellOccupied(playerRow, leftColumn))
                    candidates.Add((playerRow, leftColumn, Location.Left));

                if (!grid.IsCellOccupied(playerRow, rightColumn))
                    candidates.Add((playerRow, rightColumn, Location.Right));
            }

            if (candidates.Count > 0)
            {
                var choice = candidates[Random.Range(0, candidates.Count)];
                row = choice.Item1;
                column = choice.Item2;
                loc = choice.Item3;
            }
            else
            {
                useAimedShot = false;
                (row, column, loc) = grid.GetRandomUnoccupiedCenterEdgeCell();
            }
        }
        else
        {
            (row, column, loc) = grid.GetRandomUnoccupiedCenterEdgeCell();
        }

        spawnRow = row;
        spawnColumn = column;

        grid.SetCellOccupied(spawnRow, spawnColumn, true);

        SetLocation(row, column, loc);

        int chosenTargetRow = row;
        int chosenTargetColumn = column;

        if (useAimedShot && hasPlayerCell && !grid.IsCellOccupied(playerRow, playerColumn))
        {
            chosenTargetRow = playerRow;
            chosenTargetColumn = playerColumn;
        }
        else
        {
            if (loc == Location.Up || loc == Location.Down)
            {
                int startRow = row == 0 ? 1 : 0;
                int endRow = row == 0 ? grid.rows - 1 : grid.rows - 2;

                List<int> candidateRows = new List<int>();

                for (int r = startRow; r <= endRow; r++)
                {
                    if (!grid.IsCellOccupied(r, column))
                        candidateRows.Add(r);
                }

                if (candidateRows.Count == 0)
                {
                    grid.SetCellOccupied(spawnRow, spawnColumn, false);
                    spawnRow = -1;
                    spawnColumn = -1;
                    onComplete?.Invoke();
                    return;
                }

                chosenTargetRow = candidateRows[Random.Range(0, candidateRows.Count)];
                chosenTargetColumn = column;
            }
            else
            {
                int startColumn = column == 0 ? 1 : 0;
                int endColumn = column == 0 ? grid.columns - 1 : grid.columns - 2;

                List<int> candidateColumns = new List<int>();

                for (int c = startColumn; c <= endColumn; c++)
                {
                    if (!grid.IsCellOccupied(row, c))
                        candidateColumns.Add(c);
                }

                if (candidateColumns.Count == 0)
                {
                    grid.SetCellOccupied(spawnRow, spawnColumn, false);
                    spawnRow = -1;
                    spawnColumn = -1;
                    onComplete?.Invoke();
                    return;
                }

                chosenTargetColumn = candidateColumns[Random.Range(0, candidateColumns.Count)];
                chosenTargetRow = row;
            }
        }

        targetRow = chosenTargetRow;
        targetColumn = chosenTargetColumn;

        grid.SetCellOccupied(targetRow, targetColumn, true);

        Vector3 targetPos = grid.GetCellCenterPosition(row, column);

        if (loc == Location.Left)
        {
            targetPos.x += grid.cellWidth;
        }
        else if (loc == Location.Right)
        {
            targetPos.x -= grid.cellWidth;
        }
        else if (loc == Location.Up)
        {
            targetPos.y -= grid.cellHeight;
        }
        else
        {
            targetPos.y += grid.cellHeight;
        }

        MoveNetTowards(targetPos, loc, true);

        Vector3 finalTargetPos = grid.GetCellCenterPosition(targetRow, targetColumn);

        StartCoroutine(StartNetMoveAfterDelay(finalTargetPos, loc, netMoveDelay, onComplete));
    }

    public void ReleaseOccupiedCells()
    {
        if (grid == null)
            return;

        if (spawnRow >= 0 && spawnColumn >= 0)
            grid.SetCellOccupied(spawnRow, spawnColumn, false);

        if (targetRow >= 0 && targetColumn >= 0)
            grid.SetCellOccupied(targetRow, targetColumn, false);

        spawnRow = -1;
        spawnColumn = -1;
        targetRow = -1;
        targetColumn = -1;
    }

    IEnumerator StartNetMoveAfterDelay(Vector3 targetPosition, Location handLocation, float delay, Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        MoveNetTowards(targetPosition, handLocation, false, onComplete);
    }

    public void SetLocation(int row, int column, Location location)
    {
        this.location = location;
        transform.position = grid.GetCenterEdgePosition(row, column, location);

        SetRotation(location);
        transform.position += transform.right * netRightOffset;
    }

    public void SetRotation(Location location)
    {
        float zRotation = 0f;

        switch (location)
        {
            case Location.Up:
                zRotation = 180f;
                break;

            case Location.Down:
                zRotation = 0f;
                break;

            case Location.Left:
                zRotation = -90f;
                break;

            case Location.Right:
                zRotation = 90f;
                break;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
    }

    public void MoveNetTowards(Vector2 targetPosition, Location handLocation, bool noAnimation = false, Action onComplete = null)
    {
        Vector3 newPosition = targetPosition;

        if (handLocation == Location.Up || handLocation == Location.Down)
        {
            newPosition.x = netTransform.position.x;
        }
        else
        {
            newPosition.y = netTransform.position.y;
        }

        if (noAnimation)
        {
            netTransform.position = newPosition;
        }
        else
        {
            DOTween.KillAll();
            netTransform
                .DOMove(newPosition, Vector3.Distance(netTransform.position, newPosition) / netMoveSpeed)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    OnSmackNet();
                    onComplete?.Invoke();
                });
        }
    }

    IEnumerator PlayHitSplash()
    {
        hitSplashSprite.SetActive(true);
        yield return new WaitForSeconds(hitSplashDuration);
        hitSplashSprite.SetActive(false);
    }

    void OnSmackNet()
    {
        handRenderer.sprite = smackHandSprite;
        netRenderer.sprite = smackNetSprite;
        StartCoroutine(PlayHitSplash());

        if (netSquareBounds != null)
        {
            if (player != null)
            {
                SquareBounds playerSquareBounds = player.GetComponent<SquareBounds>();

                if (playerSquareBounds != null)
                {
                    if (netSquareBounds.IsInBoundsWithAnotherSquare(playerSquareBounds))
                    {
                        if (player.TryGetComponent(out Health playerHealth))
                        {
                            playerHealth.ChangeHealth(-1);
                        }
                    }
                }
            }
        }
    }
}
