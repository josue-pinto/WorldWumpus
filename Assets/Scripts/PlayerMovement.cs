using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;  // Velocidade de movimento do jogador
    private Vector3 targetPosition;  // Posição alvo para o movimento
    private int rows;  // Número de linhas na grid
    private int columns;  // Número de colunas na grid
    private float spacing;  // Espaçamento entre as células
    public Text alertText;  // Referência ao texto de alerta da UI
    private List<string> perceptionMessages = new List<string>();  // Lista de mensagens de percepção
    public GameObject arrowPrefab; // Prefab da flecha
    private bool hasShotArrow = false; // Flag para verificar se o jogador já disparou a flecha
    private bool isMoving = false; // Inicia a movimentação randômica do personagem
    public Text countGold;
    private int numberGold = 0;
    public Text countArrow;
    public int numberArrow = 1;


    void Start()
    {
        // Inicia o contador de ouro como zero
        countGold.text = numberGold.ToString();

        // Inicia contador das flechas
        countArrow.text = numberArrow.ToString();
        // Inicializar a posição alvo como a posição inicial do jogador
        targetPosition = transform.position;
        // Limpar texto de alerta
        alertText.text = "";
        // Checar percepções na posição inicial
        CheckForPerceptions(targetPosition);
    }

    void Update()
    {
        //Faz a movimentação randômica do personagem
        if (!isMoving)
        {
            StartCoroutine(RandomMove());
        }

        // Movimento do jogador
        if (Input.GetKeyDown(KeyCode.W))
        {
            Move(Vector2Int.up);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Move(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Move(Vector2Int.right);
        }

        // Disparo da flecha (apenas se o jogador não tiver disparado ainda)
        if (!hasShotArrow)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ShootArrow(Vector2Int.up);
                AddMessage("Você disparou uma flecha para cima.");
                UpdateAlertText();
                hasShotArrow = true;
                SituationArrow();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ShootArrow(Vector2Int.down);
                AddMessage("Você disparou uma flecha para baixo.");
                UpdateAlertText();
                hasShotArrow = true;
                SituationArrow();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ShootArrow(Vector2Int.left);
                AddMessage("Você disparou uma flecha para a esquerda.");
                UpdateAlertText();
                hasShotArrow = true;
                SituationArrow();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ShootArrow(Vector2Int.right);
                AddMessage("Você disparou uma flecha para a direita.");
                UpdateAlertText();
                hasShotArrow = true;
                SituationArrow();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                AddMessage("Você não possui mais flechas.");
                UpdateAlertText();
            }
        }

        // Mover o jogador em direção à posição alvo
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    public void Initialize(int rows, int columns, float spacing)
    {
        this.rows = rows;
        this.columns = columns;
        this.spacing = spacing;
    }

    void Move(Vector2Int direction)
    {
        Vector3 newPosition = targetPosition + new Vector3(direction.x * spacing, direction.y * spacing, 0);

        // Verificar se a nova posição está dentro dos limites da grid
        if (IsWithinGrid(newPosition))
        {
            targetPosition = newPosition;
            CheckForPerceptions(newPosition);
        }
    }

    bool IsWithinGrid(Vector3 position)
    {
        float gridWidth = (columns) * spacing;
        float gridHeight = (rows) * spacing;

        float startX = -gridWidth / 2;
        float startY = -gridHeight / 2;

        float minX = startX;
        float maxX = startX + gridWidth;
        float minY = startY;
        float maxY = startY + gridHeight;

        return position.x >= minX && position.x <= maxX && position.y >= minY && position.y <= maxY;
    }

    void CheckForPerceptions(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f);
        bool perceptionFound = false;
        bool foundBreeze = false;
        bool foundStench = false;

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Breeze"))
            {
                foundBreeze = true;
                perceptionFound = true;
            }
            else if (collider.CompareTag("Stench"))
            {
                foundStench = true;
                perceptionFound = true;
            }
            else if (collider.CompareTag("Pit"))
            {
                AddMessage("Você caiu em um poço!");
                Destroy(gameObject);
                UpdateAlertText();
                return; // Game over, você caiu no poço
            }
            else if (collider.CompareTag("Wumpus"))
            {
                AddMessage("Você morreu para o Wumpus!");
                Destroy(gameObject);
                UpdateAlertText();
                return; // Game over, você foi morto pelo Wumpus
            }
            else if (collider.CompareTag("Gold"))
            {
                AddMessage("Você encontrou o ouro!");
                AddMessage("Você coletou o ouro!");
                Destroy(collider.gameObject);               // Remover o ouro da grid
                numberGold++;                          // Incrementa o ouro capturado
                countGold.text = numberGold.ToString();  // Atualiza o display
                perceptionFound = true;
            }
        }

        if (foundBreeze && foundStench)
        {
            AddMessage("Você sente uma brisa e um fedor!");
        }
        else if (foundBreeze)
        {
            AddMessage("Você sente uma brisa!");
        }
        else if (foundStench)
        {
            AddMessage("Você sente um fedor!");
        }
        else if (!perceptionFound)
        {
            AddMessage("Você não sente nada.");
        }

        // Atualizar o texto de alerta com todas as mensagens
        UpdateAlertText();
    }

    public void AddMessage(string message)
    {
        perceptionMessages.Add(message);

        // Manter apenas as últimas 5 mensagens
        if (perceptionMessages.Count > 5)
        {
            perceptionMessages.RemoveAt(0);
        }
    }
    public void UpdateAlertText()
    {
        string formattedText = "";
        for (int i = 0; i < perceptionMessages.Count; i++)
        {
            if (i == perceptionMessages.Count - 1) // Última mensagem
            {
                formattedText += $"<color=yellow><size=20>{perceptionMessages[i]}</size></color>";
            }
            else
            {
                formattedText += perceptionMessages[i];
            }
            if (i < perceptionMessages.Count - 1)
            {
                formattedText += "\n";
            }
        }
        alertText.text = formattedText;
    }

    void ShootArrow(Vector2 direction)
    {
        Vector3 arrowPosition = transform.position + new Vector3(direction.x * spacing, direction.y * spacing, 0);
        GameObject arrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        arrowScript.Initialize(direction);
    }

    // Courrotine Para fazer a movimentação randômica do personagem
    IEnumerator RandomMove()
    {
        isMoving = true;

        int direction = Random.Range(0, 4);

        switch (direction)
        {
            case 0:
                Move(Vector2Int.up);
                break;
            case 1:
                Move(Vector2Int.down);
                break;
            case 2:
                Move(Vector2Int.left);
                break;
            case 3:
                Move(Vector2Int.right);
                break;
        }

        yield return new WaitForSeconds(1f); // Espera de 1 segundo

        isMoving = false;
    }

    void SituationArrow()
    {
        if (numberArrow > 0)
        {
            numberArrow--;
            countArrow.text = numberArrow.ToString();
        }
        else
        {
            numberArrow = 0;
            countArrow.text = numberArrow.ToString();
        }
    }
}

