using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
                target = player.transform;
        }

        if (target == null) return;

        transform.position = new Vector3(
            target.position.x,
            target.position.y,
            -10f
        );
    }
}