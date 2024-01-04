using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

public class WorldNoiseGenerator : MonoBehaviour
{
    public NativeArray<float> Continentalness { get; private set; }
    public NativeArray<float> Erosion { get; private set; }
    public NativeArray<float> Peaks { get; private set; }
    public NativeArray<float> Temperature { get; private set; }
    public NativeArray<float> Humidity { get; private set; }

    public Vector2 Position { get; private set; }

    private void OnDestroy()
    {
        Continentalness.Dispose();
        Erosion.Dispose();
        Peaks.Dispose();
        Temperature.Dispose();
        Humidity.Dispose();
    }

    public void Init()
    {
        Continentalness = new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Z, Allocator.Persistent);
        Erosion = new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Z, Allocator.Persistent);
        Peaks = new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Z, Allocator.Persistent);
        Temperature = new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Z, Allocator.Persistent);
        Humidity = new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Z, Allocator.Persistent);

        NoisesJob nj = new NoisesJob
        {
            position = Position,
            continentalness = Continentalness,
            erosion = Erosion,
            peaks = Peaks,
            temperature = Temperature,
            humidity = Humidity,
        };

        JobHandle jh = nj.Schedule(Utility.CHUNK_X * Utility.CHUNK_Z, 16);
        jh.Complete();
    }

    public float GetNoiseArrayValue(string name, Vector2Int pos)
    {
        int index = pos.x * Utility.CHUNK_X + pos.y;

        switch (name)
        {
            case "Continentalness":
                return Continentalness[index];

            case "Erosion":
                return Erosion[index];

            case "Peaks/Valleys":
                return Peaks[index];

            case "Temperature":
                return Temperature[index];

            case "Humidity":
                return Humidity[index];

            default:
                Debug.LogError("Invalid noise type!");
                return -1000.0f;
        }
    }

    [BurstCompile]
    struct NoisesJob : IJobParallelFor
    {
        public Vector2 position;

        public NativeArray<float> continentalness;
        public NativeArray<float> erosion;
        public NativeArray<float> peaks;
        public NativeArray<float> temperature;
        public NativeArray<float> humidity;

        public void Execute(int index)
        {
            int x = index % Utility.CHUNK_X;
            int z = index / Utility.CHUNK_Z;

            float xCoord = x + position.x + 0.001f;
            float zCoord = z + position.y + 0.001f;

            float cSample = noise.pnoise(new float2(xCoord / WorldNoiseSettings.CONT_SCALE,     zCoord / WorldNoiseSettings.CONT_SCALE),    float.MaxValue);
            float eSample = noise.pnoise(new float2(xCoord / WorldNoiseSettings.ERO_SCALE,      zCoord / WorldNoiseSettings.ERO_SCALE),      float.MaxValue);
            float pSample = noise.pnoise(new float2(xCoord / WorldNoiseSettings.P_V_SCALE,      zCoord / WorldNoiseSettings.P_V_SCALE),      float.MaxValue);
            float tSample = noise.pnoise(new float2(xCoord / WorldNoiseSettings.TEMP_SCALE,     zCoord / WorldNoiseSettings.TEMP_SCALE),    float.MaxValue);
            float hSample = noise.pnoise(new float2(xCoord / WorldNoiseSettings.HUMID_SCALE,    zCoord / WorldNoiseSettings.HUMID_SCALE),  float.MaxValue);

            continentalness[index] =    cSample;
            erosion[index] =            eSample;
            peaks[index] =              pSample;
            temperature[index] =        tSample;
            humidity[index] =           hSample;
        }
    }
}
