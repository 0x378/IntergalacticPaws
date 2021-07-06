using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LunarSurface : MonoBehaviour
{
    // Map properties:
    private int mapWidth;           // Array size for the map data
    private float moonRadius;       // Relative to the array size

    // Scales the physical map size while maintaining the number of polygons:
    private int sizeScale;

    // Crater properties:
    private float craterRadius;     // Horizontal radius relative to the map center
    private float craterDepth;      // Height from the vertex to the outer edges
    private float craterEdgeDecay;  // How quickly the edge transitions to ground level
    private float craterEdgeRatio;  // Portion of the height above ground level

    // Random terrain noise:
    private float coarseNoiseCoefficient;
    private float coarseNoiseMaximum;
    private float fineNoiseCoefficient;
    private float fineNoiseMaximum;

    private float[] offset;
    private float previousUpdateTime;

    private Terrain mySurface;
    private GameManager gameManager;

    // Additional constants, to be set by the Generate function:
    private float centerOffset;
    private float radiusSquared;
    private float coarseScale;
    private float fineScale;
    private float craterRadiusSquared;
    private float craterVertex;
    private float craterEdge;
    private float scaledMapWidth;
    private float scaledCenterOffset;
    private float scaledCraterRadius;
    private Vector3 globalOrigin;

    private void Awake()
    {
        // Prevent the creation of this object prior to initialization:
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Destroy(gameObject);
        }
    }

    // Get the map width using its actual, global size:
    public float GetMapWidth()
    {
        return scaledMapWidth;
    }

    // Crater radius relative to global coordinates:
    public float GetCraterRadius()
    {
        return scaledCraterRadius;
    }

    // Center x, z coordinate in global space:
    public float GetCenterOffset()
    {
        return scaledCenterOffset;
    }

    public float DistanceFromOrigin(GameObject target)
    {
        return Vector3.Distance(target.transform.position, globalOrigin);
    }

    public float DistanceFromCenter(GameObject target)
    {
        float differenceX = target.transform.position.x - scaledCenterOffset;
        float differenceZ = target.transform.position.z - scaledCenterOffset;
        return Mathf.Sqrt((differenceX * differenceX) + (differenceZ * differenceZ));
    }
    public Vector3 getCraterCenter()
    {
        return new Vector3(GetMapWidth() / 2f, GetExactHeight(GetMapWidth() / 2f, GetMapWidth() / 2f), GetMapWidth() / 2f);
    }
    public bool IsWithinCrater(GameObject target)
    {
        return DistanceFromCenter(target) < scaledCraterRadius;
    }

    // Get the terrain height at the specified global coordinates:
    public float GetExactHeight(float x, float z)
    {
        float xIndex = x / (float)sizeScale;
        float zIndex = z / (float)sizeScale;

        float outputHeight = GenerateHeightmapValue(xIndex, zIndex, true) * scaledMapWidth;
        return outputHeight;
    }

    public float GetSmoothHeight(float x, float z)
    {
        float xIndex = x / (float)sizeScale;
        float zIndex = z / (float)sizeScale;

        float outputHeight = GenerateHeightmapValue(xIndex, zIndex, false) * scaledMapWidth;
        return outputHeight;
    }

    private void RandomizeOffsets()
    {
        // Random offsets for the Perlin terrain noise generation:

        for (int index = 0; index < 4; index++)
        {
            offset[index] = Random.Range(0f, 512f);
        }
    }

    // Generate a terrain heightmap value for a single coordinate:
    private float GenerateHeightmapValue(float xIndex, float zIndex, bool fineNoiseEnabled)
    {
        float rDistance = MathX.Distance(xIndex, zIndex, centerOffset, centerOffset);
        float rDistanceSquared = rDistance * rDistance;
        float localPlanetHeight = Mathf.Sqrt(radiusSquared - rDistanceSquared);
        float localCraterHeight;
        float noiseRatio;
        float noiseX;
        float noiseY;
        float noiseZ;

        if (rDistance < craterRadius) // Within the scope of the crater:
        {
            float radiusRatioSquared = rDistanceSquared / craterRadiusSquared;
            localCraterHeight = craterDepth * radiusRatioSquared + craterVertex;

            // Reduce the noise towards the center to smooth out the crater:
            noiseRatio = 0.8f * radiusRatioSquared + 0.2f;
        }
        else // Smoothing the planet surface to the the crater edges:
        {
            localCraterHeight = craterEdge * Mathf.Exp(craterEdgeDecay * (1f - (rDistance / craterRadius)));
            noiseRatio = 1f;
        }

        // Coarse noise generation:
        noiseX = coarseScale * xIndex + offset[0];
        noiseZ = coarseScale * zIndex + offset[1];
        float coarseNoiseLevel = Mathf.PerlinNoise(noiseX, noiseZ) * coarseNoiseMaximum;

        if (fineNoiseEnabled)
        {
            // Fine noise generation:
            noiseX = fineScale * xIndex + offset[2];
            noiseZ = fineScale * zIndex + offset[3];
            float fineNoiseLevel = Mathf.PerlinNoise(noiseX, noiseZ) * fineNoiseMaximum;

            // Total noise, scaled for smoothing towards the middle of the crater:
            noiseY = (coarseNoiseLevel + fineNoiseLevel) * noiseRatio;
        }
        else
        {
            noiseY = coarseNoiseLevel * noiseRatio;
        }

        // Taking away the moonRadius from the overall terrain height and scaling by the mapWidth allows
        // us to align the top of the surface into the middle of the heightmap's accepted range of 0 to 1.
        float heightSum = localPlanetHeight + localCraterHeight - noiseY;
        float heightmapValue = (heightSum - moonRadius) / mapWidth + 0.5f;

        return heightmapValue;
    }

    private void Generate()
    {
        // Multidimensional array to hold the heightmap data:
        float[,] heights = new float[mapWidth, mapWidth];

        // Constants:
        coarseScale = coarseNoiseCoefficient / mapWidth;
        fineScale = fineNoiseCoefficient / mapWidth;
        radiusSquared = moonRadius * moonRadius;
        centerOffset = (float)(mapWidth - 1) * 0.5f;

        // Top and bottom-most heights of the crater:
        craterEdge = craterEdgeRatio * craterDepth;
        craterVertex = craterEdge - craterDepth;
        craterRadiusSquared = craterRadius * craterRadius;

        scaledMapWidth = (float)mapWidth * sizeScale;
        scaledCenterOffset = centerOffset * sizeScale;
        scaledCraterRadius = craterRadius * sizeScale;

        globalOrigin = new Vector3(scaledCenterOffset, scaledCenterOffset, scaledCenterOffset);

        for (int xIndex = 0; xIndex < mapWidth; xIndex++)
        {
            for (int zIndex = 0; zIndex < mapWidth; zIndex++)
            {
                heights[zIndex, xIndex] = GenerateHeightmapValue(xIndex, zIndex, true);
            }
        }

        mySurface.terrainData.heightmapResolution = mapWidth + 1;
        mySurface.terrainData.size = new Vector3(scaledMapWidth, scaledMapWidth, scaledMapWidth);
        mySurface.terrainData.SetHeights(0, 0, heights);
        previousUpdateTime = Time.unscaledTime;
        gameManager.triggerDomeResize = true;
    }

    // Get terrain parameters from the UI sliders:
    private void GetSliderValues()
    {
        craterRadius = gameManager.sliders[0].value;
        craterDepth = gameManager.sliders[1].value;
        craterEdgeDecay = gameManager.sliders[2].value;
        craterEdgeRatio = gameManager.sliders[3].value;
        coarseNoiseCoefficient = gameManager.sliders[4].value;
        coarseNoiseMaximum = gameManager.sliders[5].value;
        fineNoiseCoefficient = gameManager.sliders[6].value;
        fineNoiseMaximum = gameManager.sliders[7].value;
    }

    // Update the text readouts of each UI slider with its current value
    private void UpdateSliderText()
    {
        gameManager.sliderTexts[0].text = craterRadius.ToString("F2");
        gameManager.sliderTexts[1].text = craterDepth.ToString("F2");
        gameManager.sliderTexts[2].text = craterEdgeDecay.ToString("F2");
        gameManager.sliderTexts[3].text = craterEdgeRatio.ToString("F2");
        gameManager.sliderTexts[4].text = coarseNoiseCoefficient.ToString("F2");
        gameManager.sliderTexts[5].text = coarseNoiseMaximum.ToString("F2");
        gameManager.sliderTexts[6].text = fineNoiseCoefficient.ToString("F1");
        gameManager.sliderTexts[7].text = fineNoiseMaximum.ToString("F2");
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        mapWidth = 128;
        moonRadius = 375f;
        sizeScale = 8;
        offset = new float[4];

        GetSliderValues();
        UpdateSliderText();

        mySurface = GetComponent<Terrain>();

        RandomizeOffsets();
        Generate();

        GameManager.terrainInstance = this;
    }

    private void Update()
    {
        if (gameManager.editorInterface.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                RandomizeOffsets();
            }

            if (Time.unscaledTime - previousUpdateTime > 0.125f)
            {
                GetSliderValues();
                UpdateSliderText();
                Generate();
            }
        }
    }
}
