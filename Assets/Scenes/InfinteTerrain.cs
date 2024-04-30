using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InfinteTerrain : MonoBehaviour
{
    public int width;
    public int depth;
    public int chunkDimension;
    public float terrainHeight;
    public float minGrassDistance = -1.44f;
    public float minTreeDistance = 3f;
    public float minRockDistance = 15f;
    public float minWaterDistance = -120.36f;
    public Gradient terraininColorGradient;
    public Material mTerrain;
    public GameObject treePrefab;
    public GameObject rockPrefab;
    public GameObject grassPrefab;
    public GameObject waterPrefab;

    private void Start()
    {
        StartCoroutine(GenerateTerrain());
        
        //Dynamically set the camera so the whole mesh is visible
        Camera.main.transform.position = new Vector3((width / 2f) * chunkDimension, 2 * terrainHeight,(depth/10f) * chunkDimension);
        Camera.main.transform.LookAt(new Vector3((width / 2f) * chunkDimension, 0, (depth / 10f) * chunkDimension));
    }

    IEnumerator GenerateTerrain()
    {
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject go = new GameObject("Chunk_" + x + "_" + z,
                    typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshGenerator));
                go.transform.parent = transform;
                go.transform.localPosition = new Vector3(x * chunkDimension, 0, z * chunkDimension);
                go.GetComponent<MeshRenderer>().material = mTerrain;
                go.GetComponent<MeshGenerator>().Init(chunkDimension, terrainHeight,x,z,terraininColorGradient, minGrassDistance, minTreeDistance, minRockDistance, minWaterDistance);
                yield return new WaitForEndOfFrame();
                
                go.GetComponent<MeshGenerator>().PlaceObjects(grassPrefab, treePrefab, rockPrefab, waterPrefab);


            }
        }

        yield return true;
    }
}
