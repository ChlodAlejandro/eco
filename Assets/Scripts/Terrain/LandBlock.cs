using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandBlock : MonoBehaviour
{
    private float sandiness;
    
    internal Material baseLandMaterial;
    internal Material sandLandMaterial;
    internal GameObject sandBlock;

    public float CalculateSandiness()
    {
        Collider[] nearbyBlocks = Physics.OverlapBox(transform.position, new Vector3(3f, 0.5f, 3f));
        List<Collider> nearbyWaterBlocks = new List<Collider>();
        foreach (Collider collider in nearbyBlocks)
        {
            if (collider.GetComponent<WaterBlock>() != null)
            {
                nearbyWaterBlocks.Add(collider);
            }
        }
        sandiness = Mathf.Clamp(nearbyWaterBlocks.Count / 12f, 0f, 1f);
        return sandiness;
    }

    public void ApplySandiness()
    {
        CalculateSandiness();

        if (sandiness == 1 && !name.StartsWith("S"))
        {
            GameObject newBlock = Instantiate(sandBlock);
            newBlock.transform.parent = transform.parent;
            newBlock.transform.position = new Vector3(transform.position.x, 0.875f, transform.position.z);
            newBlock.name = "S | " + (transform.position.x + 0.5) + ", " + (transform.position.z + 0.5);
            LandBlock newBlockTerrain = newBlock.GetComponent<LandBlock>();
            newBlockTerrain.baseLandMaterial = baseLandMaterial;
            newBlockTerrain.sandLandMaterial = sandLandMaterial;
            newBlockTerrain.sandBlock = sandBlock;
            newBlockTerrain.ApplySandiness();
            DestroyImmediate(gameObject);
            return;
        }

        Material rendererMaterial = new Material(GetComponent<Renderer>().sharedMaterial);
        rendererMaterial.EnableKeyword("_EmissionColor");
        Color baseColor = baseLandMaterial.color;
        Color sandColor = sandLandMaterial.color;
        Color baseEmissionColor = baseLandMaterial.GetColor("_EmissionColor");
        Color sandEmissionColor = sandLandMaterial.GetColor("_EmissionColor");
        rendererMaterial.color = Color.Lerp(baseColor, sandColor, sandiness);
        rendererMaterial.SetColor("_EmissionColor", Color.Lerp(baseEmissionColor, sandEmissionColor, sandiness));
        GetComponent<Renderer>().sharedMaterial = rendererMaterial;
    }

    public void SpawnFoliage(Foliage plant)
    {
        if (sandiness != 0) return;
        GameObject foliageObject = Instantiate(plant.gameObject);
        foliageObject.transform.parent = transform;
        foliageObject.transform.position = new Vector3(
            transform.position.x,
            foliageObject.transform.position.y + transform.position.y,
            transform.position.z);
    }
}
