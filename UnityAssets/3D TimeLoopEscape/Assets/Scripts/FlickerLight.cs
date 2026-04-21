using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    public Light lightSource;
    public float minIntensity = 1.2f;
    public float maxIntensity = 2.5f;

    void Update()
    {
        lightSource.intensity = Random.Range(minIntensity, maxIntensity);
    }
}