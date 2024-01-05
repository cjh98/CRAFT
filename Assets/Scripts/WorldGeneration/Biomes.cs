using UnityEngine;

public class Biomes : MonoBehaviour
{
    public Biome[] biomes;

    [System.NonSerialized]
    public static Biomes instance;

    private void Awake()
    {
        instance = this;
    }
}

[System.Serializable]
public struct Biome
{
    public string name;
    public Utility.Blocks surfaceBlock;
    public Utility.Blocks subSurfaceBlock;
    public float squashFactor;
}