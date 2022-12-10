using System.Collections;
using UnityEngine;

public static class MonoBehaviourExtension
{
    public static void Move(this MonoBehaviour mono, Vector3 destination, float duration)
    {
        mono.StopAllCoroutines();
        mono.StartCoroutine(mono.transform.MoveLocalCoroutine(destination, duration));
    }
}