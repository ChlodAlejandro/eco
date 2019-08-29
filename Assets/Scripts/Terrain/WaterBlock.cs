using UnityEngine;

public class WaterBlock : MonoBehaviour
{

    internal bool reserved;

    public void Reserve()
    {
        reserved = true;
    }

    public void Release()
    {
        reserved = false;
    }

}
