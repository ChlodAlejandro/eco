using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TerrainGenerator : MonoBehaviour
{
    public Texture2D map;

    public GameObject landBlock;
    public GameObject sandBlock;
    public GameObject waterBlock;

    public Material baseLandMaterial;
    public Material sandLandMaterial;
    
    public Foliage[] foliage;

    float width { get { return map.width; } }
    float height { get { return map.height; } }

    public void GenerateTerrain()
    {
        ClearTerrain();
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Color color = map.GetPixel(x, z);
                GameObject terrainBlock;
                if (color.g > color.b) terrainBlock = Instantiate(landBlock);
                else terrainBlock = Instantiate(waterBlock);
                terrainBlock.transform.parent = gameObject.transform;
                terrainBlock.transform.position = new Vector3(x, terrainBlock.transform.position.y, z);
                terrainBlock.name = (color.g > color.b ? "L" : "W") + " | " + x + ", " + z;
            }
        }
    }

    public void DecorateTerrain()
    {
        LandBlock[] blocks = GetComponentsInChildren<LandBlock>();
        foreach (LandBlock block in blocks)
        {
            block.baseLandMaterial = baseLandMaterial;
            block.sandLandMaterial = sandLandMaterial;
            block.sandBlock = sandBlock;
            block.ApplySandiness();
            Foliage plant = foliage[Random.Range(0, foliage.Length)];
            if (block != null && ((Mathf.Round(Random.Range(0f, 1f) * 100) / 100) < (plant.concentration)))
                block.SpawnFoliage(plant);
        }
    }

    public void ClearTerrain()
    {
        int children = transform.childCount;
        for (int i = children - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainGenerator terrainGen = (TerrainGenerator)target;
        if (GUILayout.Button("Generate Terrain"))
        {
            terrainGen.GenerateTerrain();
        }

        if (GUILayout.Button("Decorate Terrain"))
        {
            terrainGen.DecorateTerrain();
        }

        if (GUILayout.Button("Clear Terrain"))
        {
            terrainGen.ClearTerrain();
        }
    }
}
#endif