using UnityEngine;

public class WorldNoiseSettings : MonoBehaviour
{
    public static WorldNoiseSettings Instance;

    public AnimationCurve ContinentalnessCurve;
    public AnimationCurve ErosionCurve;

    private void Awake()
    {
        Instance = this;
    }

    public const float NOISE_SCALE = 65.0f;
    public const float SQUASH_FACTOR = 3.5f;
    public const float DEFAULT_HEIGHT_OFFSET = 0.5f;
    public const int OCTAVES = 4;
    public const float LACUNARITY = 2.0f;
    public const float PERSISTENCE = 0.5f;

    // continentalness noise parameters
    public const float CONT_SCALE = 50.0f;
    public const int CONT_OCTAVES = 6;
    public const float CONT_LACUNARITY = 1.5f;
    public const float CONT_PERSISTENCE = 0.75f;

    // erosion noise parameters
    public const float ERO_SCALE = 200.0f;
    public const int ERO_OCTAVES = 3;
    public const float ERO_LACUNARITY = 3.0f;
    public const float ERO_PERSISTENCE = 0.33f;
}
