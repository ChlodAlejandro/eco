using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Foliage))]
public class FoliageFoodSource : FoodSource
{
    public float baseNutrition = 2.0f; // how much nutrition this "food" can provide when full
    public float remainingNutrition = 2.0f;
    public float regenerationSpeed = 50f;
    public override FoodType type { get { return FoodType.Foliage; } }

    private void Start()
    {
        remainingNutrition = baseNutrition;
        StartCoroutine("Refill");
    }

    IEnumerator Refill()
    {
        for (;;)
        {
            if (remainingNutrition < baseNutrition && !reserved)
                remainingNutrition += (regenerationSpeed * 0.001f);
            if (remainingNutrition > baseNutrition)
                remainingNutrition = baseNutrition;
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void Update()
    {
        if (baseNutrition != 0)
        {
            float rNF = remainingNutrition / baseNutrition; // remaining nutrition factor
            transform.localScale = new Vector3(rNF, rNF, rNF);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FoliageFoodSource))]
public class FoliageFoodSourceInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FoliageFoodSource foodSource = (FoliageFoodSource)target;
        if (GUILayout.Button("Refill"))
        {
            foodSource.remainingNutrition = foodSource.baseNutrition;
        }

        if (GUILayout.Button("Empty"))
        {
            foodSource.remainingNutrition = 0f;
        }
    }
}
#endif