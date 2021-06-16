using UnityEditor;
using UnityEngine;

public class CraterDome : MonoBehaviour
{
    private readonly float originalRadius = 128f;
    private float radius;

    [SerializeField] private GameObject innerDome;
    [SerializeField] private GameObject outerDome;

    /*
     * UNCOMMENT ONLY FOR NEW DOME GENERATION IN THE UNITY EDITOR:
     * 
    private int numberOfLayers = 9;
    private Vector3[] vertices;
    private int[] innerTriangles;
    private int[] outerTriangles;
    private int[] initialAtLayer; // The first vertex index of each layer
    private float[] angleRecord;  // A copy of all the horizontal angles calculated within each layer
    */

    private Vector2[] uvBackup;
    private Vector2[] uvScaled;

    private GameManager gameManager;

    private void Awake()
    {
        // Prevent the creation of this object prior to initialization:
        if (GameManager.Instance == null)
        {
            Destroy(gameObject);
        }
    }

    // Uncomment these lines to regenerate a new mesh for the dome:
    private void Start()
    {
        radius = originalRadius;
        uvBackup = innerDome.GetComponent<MeshFilter>().mesh.uv;
        uvScaled = innerDome.GetComponent<MeshFilter>().mesh.uv;

        //Generate();
        //SaveToAssets();

        gameManager = GameManager.Instance;
        transform.localScale = new Vector3(radius, radius, radius);
    }

    private void UpdateScale()
    {
        float currentRadius = GameManager.terrainInstance.GetCraterRadius();

        // Update only on change:
        if (currentRadius != radius)
        {
            radius = currentRadius;
            transform.localScale = new Vector3(radius, radius, radius);

            for (int i = uvBackup.Length - 1; i >= 0; i--)
            {
                uvScaled[i] = uvBackup[i] * radius / originalRadius;
            }

            innerDome.GetComponent<MeshFilter>().mesh.uv = uvScaled;
            outerDome.GetComponent<MeshFilter>().mesh.uv = uvScaled;
        }
    }

    private void Update()
    {
        if (gameManager.triggerDomeResize)
        {
            UpdateScale();
            gameManager.triggerDomeResize = false;
        }
    }

    /*
     * DOME GENERATION, FOR UNITY EDITOR ONLY
     * 
    // Get the closest vertex corresponding to the same angle from the layer above:
    private int GetClosestVertex(int layer, float angle)
    {
        int upperLayer = layer - 1;
        int initialIndex = initialAtLayer[upperLayer];
        int finalIndex = initialAtLayer[layer];

        int index = initialIndex;

        float angleDifference = MathX.AngleWithin180((angleRecord[index] - angle) * Mathf.Rad2Deg);

        if (0f <= angleDifference)
        {
            if (angleDifference < 90f) // Skip 75% of the input index range:
            {
                index = (3 * finalIndex + index) / 4;
            }
            else // Skip 50% of the input index range:
            {
                index = (finalIndex + index) / 2;
            }
        }

        while (index < finalIndex)
        {
            angleDifference = MathX.AngleWithin180((angleRecord[index] - angle) * Mathf.Rad2Deg);
            //Debug.Log("DEBUG: i = " + index + ", a = " + angleDifference);

            if (angleDifference > 0f)
            {
                return index;
            }

            index++;
        }

        return initialIndex; // Occurs only in the case that the initial index was the closest
    }

    void SaveToAssets()
    {
        MeshFilter innerMeshFilter = innerDome.GetComponent<MeshFilter>();
        MeshFilter outerMeshFilter = outerDome.GetComponent<MeshFilter>();

        if (innerMeshFilter != null)
        {
            AssetDatabase.CreateAsset(innerMeshFilter.mesh, "Assets/Terrain/InnerDomeMesh.asset");
        }

        if (outerMeshFilter != null)
        {
            AssetDatabase.CreateAsset(outerMeshFilter.mesh, "Assets/Terrain/OuterDomeMesh.asset");
        }

        AssetDatabase.SaveAssets();
    }

    // All angles are in radians:
    private void Generate()
    {
        Mesh innerMesh = new Mesh();
        Mesh outerMesh = new Mesh();

        int rowsBetweenLayers = numberOfLayers - 1;
        float targetArcAngle = 0.55f * Mathf.PI / rowsBetweenLayers;

        float[] angleAtLayer = new float[numberOfLayers]; // Layer angle from the vertical axis
        int[] verticesAtLayer = new int[numberOfLayers];  // Number of vertices in each layer
        initialAtLayer = new int[numberOfLayers];         // The first vertex index of each layer

        angleAtLayer[0] = 0f;
        verticesAtLayer[0] = 1;

        int numberOfTriangles = 0;
        int numberOfVertices = 1;

        initialAtLayer[0] = 0;

        // Determine the quantity of vertices and triangles needed:
        for (int layer = 1; layer < numberOfLayers; layer++)
        {
            initialAtLayer[layer] = numberOfVertices;

            float angleFromVertical = (float)layer * targetArcAngle;
            float radiusThisLayer = Mathf.Sin(angleFromVertical);
            float circumferenceThisLayer = 2f * Mathf.PI * radiusThisLayer;
            int verticesThisLayer = Mathf.RoundToInt(circumferenceThisLayer / targetArcAngle);

            angleAtLayer[layer] = angleFromVertical;
            verticesAtLayer[layer] = verticesThisLayer;

            numberOfVertices += verticesThisLayer;
            numberOfTriangles += verticesAtLayer[layer - 1] + verticesThisLayer;
        }

        vertices = new Vector3[numberOfVertices];
        Vector2[] uv = new Vector2[numberOfVertices];

        innerTriangles = new int[numberOfTriangles * 3];
        outerTriangles = new int[numberOfTriangles * 3];

        // Layer 0 (Topmost point):
        vertices[0] = new Vector3(0f, 1f, 0f);
        uv[0] = new Vector2(0f, 0f);

        float currentHorizontalAngle;
        float currentVerticalAngle = angleAtLayer[1];
        int currentNumberOfVertices = verticesAtLayer[1];
        float currentLayerRadius = Mathf.Sin(currentVerticalAngle);
        float uvOffset = 1f + currentVerticalAngle;

        float currentY = Mathf.Cos(currentVerticalAngle);
        float currentX;
        float currentZ;

        int triangleIndex = 0;
        int vertexIndex = 1;
        int initialVertex = initialAtLayer[1];
        int finalVertex = initialAtLayer[2] - 1;

        angleRecord = new float[numberOfVertices];
        angleRecord[0] = 0f;

        // Layer 1 (Hexagon):
        while (vertexIndex <= finalVertex)
        {
            currentHorizontalAngle = 2f * vertexIndex * Mathf.PI / currentNumberOfVertices;
            angleRecord[vertexIndex] = currentHorizontalAngle;

            currentX = currentLayerRadius * Mathf.Cos(currentHorizontalAngle);
            currentZ = currentLayerRadius * Mathf.Sin(currentHorizontalAngle);

            innerTriangles[triangleIndex] = 0;
            outerTriangles[triangleIndex++] = vertexIndex;
            innerTriangles[triangleIndex] = vertexIndex;
            outerTriangles[triangleIndex++] = 0;

            if (vertexIndex == finalVertex)
            {
                innerTriangles[triangleIndex] = initialVertex;
                outerTriangles[triangleIndex++] = initialVertex;
            }
            else
            {
                innerTriangles[triangleIndex] = vertexIndex + 1;
                outerTriangles[triangleIndex++] = vertexIndex + 1;
            }

            vertices[vertexIndex] = new Vector3(currentX, currentY, currentZ);
            uv[vertexIndex++] = new Vector2(currentX * uvOffset, currentZ * uvOffset);
        }

        float offsetAngle = 0f;
        int previousClosestVertex;

        // All subsequent layers:
        for (int layer = 2; layer < numberOfLayers; layer++)
        {
            currentVerticalAngle = angleAtLayer[layer];
            currentNumberOfVertices = verticesAtLayer[layer];
            currentLayerRadius = Mathf.Sin(currentVerticalAngle);
            currentY = Mathf.Cos(currentVerticalAngle);

            initialVertex = initialAtLayer[layer];
            finalVertex = initialVertex + currentNumberOfVertices - 1;
            offsetAngle += 0.5f * targetArcAngle;

            previousClosestVertex = vertexIndex - 1;
            uvOffset = 1f + currentVerticalAngle;

            while (vertexIndex <= finalVertex)
            {
                currentHorizontalAngle = 2f * (1 + vertexIndex - initialVertex) * Mathf.PI / currentNumberOfVertices - offsetAngle;
                angleRecord[vertexIndex] = currentHorizontalAngle;

                currentX = currentLayerRadius * Mathf.Cos(currentHorizontalAngle);
                currentZ = currentLayerRadius * Mathf.Sin(currentHorizontalAngle);

                int closestVertexIndex = GetClosestVertex(layer, currentHorizontalAngle);

                if (closestVertexIndex != previousClosestVertex) // Fill in the extra gap with a triangle:
                {
                    innerTriangles[triangleIndex] = closestVertexIndex;
                    outerTriangles[triangleIndex++] = previousClosestVertex;
                    innerTriangles[triangleIndex] = previousClosestVertex;
                    outerTriangles[triangleIndex++] = closestVertexIndex;
                    innerTriangles[triangleIndex] = vertexIndex;
                    outerTriangles[triangleIndex++] = vertexIndex;
                }

                innerTriangles[triangleIndex] = closestVertexIndex;
                outerTriangles[triangleIndex++] = vertexIndex;
                innerTriangles[triangleIndex] = vertexIndex;
                outerTriangles[triangleIndex++] = closestVertexIndex;

                if (vertexIndex == finalVertex)
                {
                    innerTriangles[triangleIndex] = initialVertex;
                    outerTriangles[triangleIndex++] = initialVertex;
                }
                else
                {
                    innerTriangles[triangleIndex] = vertexIndex + 1;
                    outerTriangles[triangleIndex++] = vertexIndex + 1;
                }

                vertices[vertexIndex] = new Vector3(currentX, currentY, currentZ);
                uv[vertexIndex++] = new Vector2(currentX * uvOffset, currentZ * uvOffset);
                previousClosestVertex = closestVertexIndex;
            }
        }

        innerMesh.Clear();
        outerMesh.Clear();
        innerMesh.vertices = vertices;
        outerMesh.vertices = vertices;
        innerMesh.triangles = innerTriangles;
        outerMesh.triangles = outerTriangles;
        innerMesh.uv = uv;
        outerMesh.uv = uv;

        innerDome.GetComponent<MeshFilter>().mesh = innerMesh;
        outerDome.GetComponent<MeshFilter>().mesh = outerMesh;
    }
    */
}
