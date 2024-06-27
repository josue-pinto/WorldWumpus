using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 direction;
    public Text countWumpus;
    private int numberWumpus = 0;

    public void Initialize(Vector2 direction)
    {
        this.direction = direction.normalized;
        Destroy(gameObject, 1f); // Destroi a flecha após 2 segundos
    }

    void FixedUpdate()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Wumpus"))
        {
            // Notificar o jogador que matou o Wumpus
            PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
            playerMovement.LogRegister("Grito", "Perception", "...");
            playerMovement.AddMessage("Você ouve um grito!");
            playerMovement.UpdateAlertText();
            playerMovement.LogRegister("Wumpus Dead", "Perception", "...");
            playerMovement.AddMessage("Você matou o Wumpus!");
            playerMovement.UpdateAlertText();

            //Destroi o Wumpus
            Destroy(collision.gameObject);
            // Reproduzir som de colisão pela flecha
            playerMovement.hasWumpus = false;
            GridGenerator gridGenerator = FindAnyObjectByType<GridGenerator>();
            gridGenerator.PlayWumpusDeathSound();
            numberWumpus++;
            countWumpus.text = numberWumpus.ToString();
        }

        // Destruir a flecha após colidir com qualquer objeto
        Destroy(gameObject);
    }
}
