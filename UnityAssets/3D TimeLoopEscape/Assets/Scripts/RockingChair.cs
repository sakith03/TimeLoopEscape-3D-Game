using UnityEngine;

public class RockingChair : MonoBehaviour
{
    public float rockAngle = 12f;
    public float rockSpeed = 0.7f;
    public float facingAngle = 90f;

    void Update()
    {
        float angle = Mathf.Sin(Time.time * rockSpeed) * rockAngle;
        transform.localRotation = Quaternion.Euler(angle, facingAngle, 0);
    }
}