using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 direction;

    public void Initialize(Vector2 direction)
    {
        this.direction = direction.normalized;
        Destroy(gameObject, 2f); // Destroi a flecha após 2 segundos
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wumpus"))
        {
            // Notificar o jogador que matou o Wumpus
            PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
            playerMovement.AddMessage("Você matou o Wumpus!");
            playerMovement.UpdateAlertText();
            Destroy(collision.gameObject);
            
        }
        // Destruir a flecha após colidir com qualquer objeto
        Destroy(gameObject);
    }
}
