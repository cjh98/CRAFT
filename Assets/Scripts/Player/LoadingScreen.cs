using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    private float time;

    private void Start()
    {
        //time = World.instance.range / 2f;
        time = 1.0f;
    }

    private void Update()
    {
        if (time - 0.0f > 0.0001f)
            time -= Time.deltaTime;

        if (time <= 0.0)
            gameObject.SetActive(false);
    }
}
