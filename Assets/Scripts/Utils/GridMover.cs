using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Moves a character smoothly between tile centers and flips its sprite toward a target.
/// Shared by the player and enemies.
/// </summary>
public class GridMover : MonoBehaviour
{
    private const float FaceThreshold = 0.01f;

    public bool IsMoving { get; private set; }

    /// <summary>
    /// Starts moving toward a world position. Returns false if a move is already running.
    /// </summary>
    public bool MoveTo(Vector2 target, float speed, Action onFinished = null)
    {
        if (IsMoving)
        {
            return false;
        }

        FaceToward(target);
        StartCoroutine(MoveRoutine(target, speed, onFinished));
        return true;
    }

    /// <summary>
    /// Flips the sprite horizontally so it looks toward the target.
    /// </summary>
    public void FaceToward(Vector2 target)
    {
        Vector2 direction = target - (Vector2)transform.position;
        if (Mathf.Abs(direction.x) <= FaceThreshold || Mathf.Abs(direction.x) < Mathf.Abs(direction.y))
        {
            return;
        }

        Vector3 scale = transform.localScale;
        scale.x = direction.x < 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    /// <summary>
    /// Instantly places the character at a position and cancels any running move.
    /// </summary>
    public void Teleport(Vector2 position)
    {
        StopAllCoroutines();
        IsMoving = false;
        transform.position = position;
    }

    private IEnumerator MoveRoutine(Vector2 target, float speed, Action onFinished)
    {
        IsMoving = true;

        // Keep the current z so sorting/depth set on the character is preserved.
        Vector3 destination = new(target.x, target.y, transform.position.z);

        while (transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            yield return null;
        }

        IsMoving = false;
        onFinished?.Invoke();
    }
}
