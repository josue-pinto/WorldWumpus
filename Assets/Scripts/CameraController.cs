using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 10f; // Velocidade do zoom
    public float minZoom = 5f; // Valor mínimo de zoom
    public float maxZoom = 20f; // Valor máximo de zoom
    public float panSpeed = 20f; // Velocidade de pan da câmera

    private Camera cam;
    private Vector3 dragOrigin; // Ponto de origem do clique do mouse
    private bool isDragging = false; // Verifica se a câmera está sendo arrastada

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        HandleZoom();
        HandlePan();
    }

    void HandleZoom()
    {
        // Função de zoom com o scroll do mouse
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scrollData * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }

    void HandlePan()
    {
        // Detectar quando o botão do meio do mouse é pressionado
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        // Detectar quando o botão do meio do mouse é liberado
        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        // Mover a câmera enquanto o botão esquerdo do mouse é pressionado e arrastado
        if (isDragging)
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position += difference;
        }
    }
}

