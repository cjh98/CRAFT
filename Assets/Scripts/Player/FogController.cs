using UnityEngine;

public class FogController : MonoBehaviour
{
    public Material fogMaterial;
    public float fogRadius = 40f;
    public Color fogColor = Color.red;

    void Start()
    {
        if (fogMaterial != null)
        {
            fogMaterial.SetFloat("_FogRadius", fogRadius);
            fogMaterial.SetColor("_FogColor", fogColor);
        }
        else
        {
            Debug.LogError("Fog material not assigned!");
        }
    }
}
