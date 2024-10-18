using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class can be inherited to quickly implement the functionality of the "Singleton". The instance can be accessed through the static property "main".
/// </summary>
[DefaultExecutionOrder(-2)]
public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    /// <summary>
    /// [Singleton] Link to the main and only copy of this script.
    /// </summary>
    public static T main { private set; get; }

    public virtual void Awake() 
    {
        InitSingleton();
    }

    /// <summary> Defines the 'main' value for this Singleton and removes itself if duplicate. </summary>
    public void InitSingleton()
    {
        if (!main)
        {
            main = this.GetComponent<T>();
        }
        else if (main != this)
        {
            Destroy(this);
            Debug.Log($"Duplicate of Singleton \"{GetType()}\" was destroyed.");
            return;
        }
    }
}
