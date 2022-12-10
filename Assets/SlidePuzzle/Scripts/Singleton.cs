using UnityEngine;

[DisallowMultipleComponent]
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    public static T Instance
    {
        get => instance ?? (instance = GameObject.FindObjectOfType<T>(true));
    }

    protected virtual void Awake()
    {
        instance = this as T;
    }
}