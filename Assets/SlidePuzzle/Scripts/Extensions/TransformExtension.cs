using System.Collections;
using UnityEngine;

public static class TransformExtension
{
    public static IEnumerator MoveLocalCoroutine(this Transform transform, Vector3 destination, float duration)
    {
        float time = 0f;
        float normalizedTime = 0f;
        Vector3 source = transform.localPosition;

        while (time < duration)
        {
            time += Time.deltaTime;
            normalizedTime = time / duration;

            transform.localPosition = (1f - normalizedTime) * source + normalizedTime * destination;
            yield return null;
        }

        transform.localPosition = destination;
    }
}