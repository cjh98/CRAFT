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

    public Vector2 Position;

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
        Continentalness =   new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Z, Allocator.Persistent);
        Erosion =           new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Z, Allocator.Persistent);
        Peaks =             new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Z, Allocator.Persistent);
        Temperature =       new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Z, Allocator.Persistent);
        Humidity =          new NativeArray<float>(Utility.CHUNK_X * Utility.CHUNK_Z, Allocator.Persistent);

        NoisesJob nj = new NoisesJob
        {
            position =          Position,
            continentalness =   Continentalness,
            erosion =           Erosion,
            peaks =             Peaks,
            temperature =       Temperature,
            humidity =          Humidity,
        };

        JobHandle jh = nj.Schedule(Utility.CHUNK_X * Utility.CHUNK_Z, 4);
        jh.Complete();
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

            float xCoord = (x + position.x * Utility.CHUNK_X); /// WorldNoiseSettings.CONT_SCALE;
            float zCoord = (z + position.y * Utility.CHUNK_Z); /// WorldNoiseSettings.CONT_SCALE;

            float cSample = GetNoiseValue(
                xCoord, 
                zCoord, 
                WorldNoiseSettings.CONT_OCTAVES, 
                WorldNoiseSettings.CONT_LACUNARITY, 
                WorldNoiseSettings.CONT_PERSISTENCE, 
                WorldNoiseSettings.CONT_SCALE
                );

            float eSample = GetNoiseValue(
                xCoord,
                zCoord, 
                WorldNoiseSettings.ERO_OCTAVES,
                WorldNoiseSettings.ERO_LACUNARITY,
                WorldNoiseSettings.ERO_PERSISTENCE,
                WorldNoiseSettings.ERO_SCALE
                );

            //float pSample = noise.pnoise(new float2(xCoord / WorldNoiseSettings.P_V_SCALE,      zCoord / WorldNoiseSettings.P_V_SCALE),      float.MaxValue);
            //float tSample = noise.pnoise(new float2(xCoord / WorldNoiseSettings.TEMP_SCALE,     zCoord / WorldNoiseSettings.TEMP_SCALE),    float.MaxValue);
            //float hSample = noise.pnoise(new float2(xCoord / WorldNoiseSettings.HUMID_SCALE,    zCoord / WorldNoiseSettings.HUMID_SCALE),  float.MaxValue);

            continentalness[index] =    cSample;
            erosion[index] =            eSample;
            //peaks[index] =              pSample;
            //temperature[index] =        tSample;
            //humidity[index] =           hSample;
        }

        private float GetNoiseValue(float xCoord, float zCoord, int octaves, float lacunarity, float persistence, float scale)
        {
            float weight = 1.0f;
            float noiseValue = 0.0f;

            for (int i = 0; i < octaves; i++)
            {
                noiseValue += noise.pnoise(new float2(xCoord / scale, zCoord / scale), float.MaxValue) * weight;
                scale /= lacunarity;
                weight *= persistence;
            }

            return noiseValue;
        }
    }
}
