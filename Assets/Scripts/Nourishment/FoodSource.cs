using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FoodSource : MonoBehaviour
{
    internal bool reserved = false;

    abstract public FoodType type { get; }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void Reserve()
    {
        reserved = true;
    }

    public void Release()
    {
        reserved = false;
    }
}
