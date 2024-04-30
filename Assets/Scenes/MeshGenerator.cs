using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshGenerator : MonoBehaviour
{
    int width = 255;
    int depth = 255;
    private int xOffset, zOffset;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uv;
    Color[] colors;

    Gradient terraininColorGradient;
    float terrainHeight;
    
    public float minGrassDistance = -1.44f;
    public float minTreeDistance = 3f;
    public float minRockDistance = 15f;
    public float minWaterDistance = -120.36f;


    private float minTerrainHeight, maxTerrainHeight;

    private List<Vector3> blockPositions = new List<Vector3>();

    
    //Sets up the terrain 
    public void Init(int chunkDimension, float terrainHeight, int xOffset, int zOffset, Gradient terraininColorGradient,
        float minGrassDistance, float minTreeDistance, float minRockDistance, float minWaterDistance)
    {
        width = chunkDimension;
        depth = chunkDimension;
        this.terrainHeight = terrainHeight;
        this.xOffset = xOffset;
        this.zOffset = zOffset;
        this.terraininColorGradient = terraininColorGradient;
        this.minGrassDistance = minGrassDistance;
        this.minTreeDistance = minTreeDistance;
        this.minRockDistance = minRockDistance;
        this.minWaterDistance = minWaterDistance;
        blockPositions.Clear();
        
    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //CreateQuad();
        CreateGrid();
        UpdateMesh();
        

   
    }

    /// <summary>
    /// Create a group of quads automated to generate a grid of quads.
    /// </summary>
    void CreateGrid()
    {
        vertices = new Vector3[(1 + width) * (1 + depth)];

        minTerrainHeight = terrainHeight;
        maxTerrainHeight = -terrainHeight;

        for (int z = 0, i = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            { 
                vertices[i] = new Vector3(x, GetHeight(x + (xOffset * width),z+ (zOffset * depth), 20f, 3, 2f, 0.5f) * terrainHeight, z);
                if (vertices[i].y < minTerrainHeight) minTerrainHeight = vertices[i].y;
                else if(vertices[i].y > maxTerrainHeight) maxTerrainHeight = vertices[i].y;

            }
        }
        

        triangles = new int[6 * width * depth];

        int currentVertice = 0;
        int numOfTriangles = 0;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[numOfTriangles + 0] = currentVertice + 0;
                triangles[numOfTriangles + 1] = currentVertice + width + 1;
                triangles[numOfTriangles + 2] = currentVertice + 1;
                triangles[numOfTriangles + 3] = currentVertice + 1;
                triangles[numOfTriangles + 4] = currentVertice + width + 1;
                triangles[numOfTriangles + 5] = currentVertice + width + 2;

                numOfTriangles += 6;
                currentVertice++;
            }
            currentVertice++;
        }

        colors = new Color[vertices.Length];
        for (int z = 0, i = 0 ; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                colors[i] = terraininColorGradient.Evaluate(Mathf.InverseLerp(minTerrainHeight,maxTerrainHeight, vertices[i].y));
            }
        }
        
        /*uv = new Vector2[vertices.Length];
        for (int z = 0, i = 0 ; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                uv[i] = new Vector2(x * 1f/ width, z * 1f/ depth);
            }
        }*/
    }
    
    //Places objects based on height
    public void PlaceObjects(GameObject grassPrefab, GameObject treePrefab, GameObject rockPrefab, GameObject waterPrefab)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            float height = vertices[i].y;
            Vector3 position = transform.TransformPoint(vertices[i]);

            bool isNearWater = height <= 1;

            if (height >= 1 && height <= 3 /* && Random.value < 0.25*/)
            {
                if (ShouldPlaceObject(position, minGrassDistance * 1f))
                {
                    Instantiate(grassPrefab, position, Quaternion.identity);
                }
            }
            else if (height > 3 && height <= 15  /* && Random.value < 0.3f*/)
            {
                if (ShouldPlaceObject(position, minTreeDistance))
                {
                    Instantiate(treePrefab, position, Quaternion.identity);
                }
            }
            else if (height > 15 && height <= 20  /*&& Random.value < 0.05f*/)
            {
                if (ShouldPlaceObject(position, minRockDistance))
                {
                    Instantiate(rockPrefab, position, Quaternion.identity);
                }
            }
            else if (height <= 1)
            {
                if (ShouldPlaceObject(position, minWaterDistance * 1f))
                {
                    Instantiate(waterPrefab, position, Quaternion.identity);
                }
            }
        }
    }

    //Checks distance to other objects then places them
    bool ShouldPlaceObject(Vector3 position, float minDistance)
    {
        foreach (Vector3 blockPosition in blockPositions)
        {
            if (Vector3.Distance(position, blockPosition) < minDistance)
            {
                return false;
            }
        }
        blockPositions.Add(position);
        return true;
    }




    float GetHeight(float x, float z, float scale, int octaves, float lucunarity, float persistance)
    {
        if (scale <= 0) scale = 0.0001f;
        
        float height = 0;

        float frequency = 1f;
        float amplitude = 1f;

        for (int i = 0; i < octaves; i++)
        {
            float perlinValue = Mathf.PerlinNoise((x /scale) * frequency, (z/scale) * frequency);
            height += perlinValue * amplitude;

            frequency *= lucunarity;
            amplitude *= persistance;

        }
        
     
        return height;
        
    }

    /// <summary>
    /// Create a single Quad by hand.
    /// We've started with this code and then switched to "Create Grid"
    /// </summary>
    void CreateQuad()
    {
        vertices = new Vector3[]
        {
            new Vector3(0,0,0),
            new Vector3(0,0,1),
            new Vector3(1,0,0),
            new Vector3(1,0,1)
        };

        triangles = new int[]
        {
            0, 1, 2,
            1, 3, 2
        };
    }

    /// <summary>
    /// Update the Mesh Data
    /// </summary>
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        //mesh.uv = uv;

        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Draw vertice position in the scene view
    /// Deactivate to improve performance
    /// </summary>
    /*private void OnDrawGizmos()
    {
        if (vertices == null) return;
        for (int i = 0; i < vertices.Length; i++)
            Gizmos.DrawSphere(vertices[i], 0.1f);
    }*/
}