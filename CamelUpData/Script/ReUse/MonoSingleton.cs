#if UsingUnity
using UnityEngine;
#endif
using System.Collections;

#if UsingUnity
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("Game Manager");
                _instance = go.AddComponent<T>();
            }

            return _instance;
        }
    }

    public void Awake()
    {
        _instance = GameObject.FindObjectOfType<T>();
    }
}
#else 
public abstract class MonoSingleton<T> where T : MonoSingleton<T>, new()
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }

            return _instance;
        }
    }
}
#endif
