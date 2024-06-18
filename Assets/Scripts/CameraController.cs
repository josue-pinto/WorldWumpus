using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // O objeto alvo que a câmera deve seguir
    public Vector3 offset; // Offset opcional para ajustar a posição da câmera em relação ao alvo
    public float smoothSpeed = 0.125f; // Velocidade de suavização do movimento da câmera

    public float zoomSpeed = 10f; // Velocidade do zoom
    public float minZoom = 5f; // Valor mínimo de zoom
    public float maxZoom = 20f; // Valor máximo de zoom

    private Camera cam;
    private float initialZ;

    void Start()
    {
        cam = GetComponent<Camera>();
        initialZ = transform.position.z;
    }

    void LateUpdate()
    {
        // Verifica se o alvo está disponível
        if (target != null)
        {
            // Função de seguir o objeto alvo
            Vector3 desiredPosition = target.position + offset;
            desiredPosition.z = initialZ; // Mantém a posição Z constante
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }

        // Função de zoom com o scroll do mouse
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scrollData * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }

    // Método para definir o alvo externamente
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}

