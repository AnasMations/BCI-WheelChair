using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveDuration = 10f;
    public float rotateDuration = 10f;
    public float moveSpeed = 1f;
    public float rotateSpeed = 90f;
    public AudioSource audioSource;
    private Coroutine actionCoroutine;

    // Public methods to be called externally to trigger movement
    public void MoveForward()
    {
        StartAction(MoveDirectionForward, moveDuration);
    }

    public void StopMovement()
    {
        if (actionCoroutine != null)
        {
            StopCoroutine(actionCoroutine);
            actionCoroutine = null; // Clear the coroutine reference
        }
        if (audioSource.isPlaying)
        {
            audioSource.Stop(); // Stop the audio playback
        }
    }

    public void RotateRight()
    {
        StartAction(RotateDirectionRight, rotateDuration);
    }

    public void RotateLeft()
    {
        StartAction(RotateDirectionLeft, rotateDuration);
    }

    // Method to start an action (move or rotate) in a specific direction
    private void StartAction(System.Action action, float duration)
    {
        if (actionCoroutine != null)
        {
            StopCoroutine(actionCoroutine);
        }
        actionCoroutine = StartCoroutine(PerformAction(action, duration));
        audioSource.Play();
    }

    // Coroutine to handle action for a specific duration
    private IEnumerator PerformAction(System.Action action, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            action.Invoke();
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        audioSource.Stop(); // Stop the audio playback when the action completes
    }

    // Movement direction methods
    private void MoveDirectionForward()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    private void RotateDirectionRight()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void RotateDirectionLeft()
    {
        transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime);
    }
}
