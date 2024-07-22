using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothing = 5f;

    public Vector3 offset = new Vector3(0f, 10f, -10f); // Смещение камеры относительно цели

    void LateUpdate()
    {
        if (target == null)
        {
            FindPlayer();
        }
        else
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothing);
            transform.position = smoothedPosition;

            transform.LookAt(target.position); // Камера всегда направлена на цель
        }
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }
}
