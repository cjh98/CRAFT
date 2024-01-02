using UnityEngine;
using TMPro;

public class DebugInfo : MonoBehaviour
{
    Transform playerT;
    public TextMeshProUGUI posText;

    private void Start()
    {
        playerT = Camera.main.transform;
    }

    private void Update()
    {
        posText.text = playerT.position.ToString();
    }
}
