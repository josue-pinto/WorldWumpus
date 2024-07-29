using UnityEngine;
using UnityEngine.UI;
using WorldWumpus.Assets;

public class Arrow : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 direction;
    public Text countWumpus;
    private int numberWumpus = 0;

    public void Initialize(Vector2 direction)
    {
        this.direction = direction.normalized;
        Destroy(gameObject, .5f); // Destroi a flecha após 0.5 segundos
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Wumpus"))
        {
            // Notificar o jogador que matou o Wumpus
            Agent1 agent1 = FindObjectOfType<Agent1>();
            if (agent1 != null)
            {
                agent1.AddMessage("Você matou o Wumpus!");
                agent1.UpdateAlertText();
            }

            // Destruir o Wumpus
            Destroy(collision.gameObject);

            // Reproduzir som de colisão pela flecha
            GridGenerator gridGenerator = FindObjectOfType<GridGenerator>();
            GridGeneratorManual gridGeneratorManual = FindObjectOfType<GridGeneratorManual>();
            if (gridGenerator != null)
            {
                gridGenerator.PlayWumpusDeathSound();
            }
            if (gridGeneratorManual != null)
            {
                gridGeneratorManual.PlayWumpusDeathSound();
            }
            numberWumpus++;
            if (countWumpus != null)
            {
                countWumpus.text = numberWumpus.ToString();
            }
        }

        // Destruir a flecha após colidir com qualquer objeto
        Destroy(gameObject);
    }

}
