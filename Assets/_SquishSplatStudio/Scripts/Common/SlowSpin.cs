using UnityEngine;

public class SlowSpin : MonoBehaviour
{
    public Vector3 spinDirection;
    public float spinSpeed;

    // Update is called once per frame
    void Update()
    {
        if (spinDirection == Vector3.zero) return;
        transform.Rotate(spinDirection, spinSpeed);
    }
}
