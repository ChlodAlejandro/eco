using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineTest : MonoBehaviour
{
    int testvarA = 0;

    void Start()
    {
        StartCoroutine(IncreaseTestVarA());
    }

    IEnumerator IncreaseTestVarA()
    {
        RunMe(); // this will only run once
        Coroutine test = StartCoroutine(TestVarAPlusOne()); // this is the same amongs all iterations
        while (testvarA < 20)
        {
            if (testvarA == 10)
            {
                StopCoroutine(test);
            }
            Debug.Log(testvarA);
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log(testvarA);
        yield break;
    }

    IEnumerator TestVarAPlusOne()
    {
        for (;;)
        {
            testvarA += 1;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void RunMe()
    {
        Debug.Log("Hello, Unity!");
    }

}
