using System;

public static class WorldNoiseSettings
{
    public const float NOISE_SCALE = 50f;
    public const float SQUASH_FACTOR = 1f;
    public const float DEFAULT_HEIGHT_OFFSET = 0.5f;
    public const uint OCTAVES = 2;
    public const float LACUNARITY = 2.0f;
    public const float PERSISTENCE = 0.5f;

    // noise scales
    public const float CONT_SCALE = 50.0f;
    public const float ERO_SCALE = 25.0f;
    public const float P_V_SCALE = 20.0f;
    public const float TEMP_SCALE = 75.0f;
    public const float HUMID_SCALE = 30.0f;
}
