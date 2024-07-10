using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;

public class Agent2 : MonoBehaviour
{
    [Header("Config. Player")]
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private Vector3 previousPosition;
    private Vector3 lastCheckedPosition;  // Nova variável
    private int rows;
    private int columns;
    private float spacing;

    [Header("Config. Dialog")]
    public Text alertText;
    public List<string> perceptionMessages = new List<string>();
    public GameObject arrowPrefab;
    public GameObject wumpus;
    private bool hasShotArrow = false;
    private bool isMoving = false;

    [Header("Contadores - Display")]
    public Text countGold;
    private int numberGold = 0;
    public Text countArrow;
    public int numberArrow = 1;
    bool hasGold;
    public bool hasWumpus;
    string inicio;

    [Header("Config. Logging")]
    public string playerId;
    private string dbFilePath;
    private string connectionString;
    private List<Task> pendingTasks = new List<Task>();  // Lista de tarefas pendentes

    void Start()
    {
        // Inicializa a conexão/criação do banco de dados
        dbFilePath = Path.Combine(Application.dataPath, "Agent2.db");
        connectionString = "URI=file:" + dbFilePath;
        InitializeDatabase();

        // Declara os contadores iniciais
        countGold.text = numberGold.ToString();
        countArrow.text = numberArrow.ToString();
        targetPosition = transform.position;
        lastCheckedPosition = targetPosition;  // Inicializa com a posição inicial do jogador
        alertText.text = "";
        isMoving = false;
        hasGold = false;
        hasWumpus = true;
        CheckForPerceptions(targetPosition);
        StartCoroutine(PlayerSimulation());  // Inicia a simulação do jogador
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (transform.position == targetPosition)
            {
                isMoving = false;
                CheckForPerceptions(targetPosition);
            }
        }
    }

    IEnumerator PlayerSimulation()
    {
        while (true)
        {
            if (!isMoving)
            {
                // Verifique se alguma condição específica foi atendida
                if (hasGold || !hasWumpus)
                {
                    yield break;  // Termina a corrotina se a condição for atendida
                }

                // Move-se aleatoriamente
                RandomDirection();

                // Aguarda um pouco antes de se mover novamente
                yield return new WaitForSeconds(1f);
            }
            yield return null;
        }
    }

    public void Initialize(int rows, int columns, float spacing)
    {
        this.rows = rows;
        this.columns = columns;
        this.spacing = spacing;
    }

    void Move(Vector2Int direction, string direcao)
    {
        Vector3 newPosition = targetPosition + new Vector3(direction.x * spacing, direction.y * spacing, 0);

        if (IsWithinGrid(newPosition))
        {
            previousPosition = targetPosition;
            targetPosition = newPosition;
            LogRegister("...", "Move", direcao);  // Registrar o movimento aqui
            isMoving = true;
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

    public bool CheckForPerceptions(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f);

        bool perceptionFound = false;
        bool foundBreeze = false;
        bool foundStench = false;
        bool hasPit = false;

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
                hasPit = true;
                perceptionFound = true;
            }
            else if (collider.CompareTag("Wumpus"))
            {
                if (hasWumpus)
                {
                    perceptionFound = true;
                }
                else
                {
                    LogRegister("Stench", "Perception", "...");
                    AddMessage("Você sente um fedor!");
                    UpdateAlertText();
                    RandomAction(1);
                }
            }
            else if (collider.CompareTag("Gold"))
            {
                if (!hasGold)
                {
                    perceptionFound = true;
                    LogRegister("Light", "Perception", "...");
                    AddMessage("Você percebe um brilho!");
                    UpdateAlertText();
                    LogRegister("Catch", "Perception", "...");
                    AddMessage("Você encontrou o ouro!");
                    UpdateAlertText();
                    hasGold = true;
                    RandomAction(1);
                    numberGold++;
                    countGold.text = numberGold.ToString();
                }
                else
                {
                    perceptionFound = false;
                    LogRegister("Nothing", "Perception", "...");
                    AddMessage("Você não sente nada.");
                    UpdateAlertText();
                    RandomAction(1);
                }
            }
        }

        if (foundBreeze && foundStench && !hasPit)
        {
            LogRegister("Breeze and Stench", "Perception", "...");
            AddMessage("Você sente uma brisa e um fedor!");
            UpdateAlertText();
            RandomAction(2);
        }
        else if (foundStench && !hasPit)
        {
            LogRegister("Stench", "Perception", "...");
            AddMessage("Você sente um fedor!");
            UpdateAlertText();
            RandomAction(2);
        }
        else if (foundBreeze && !hasPit)
        {
            LogRegister("Breeze", "Perception", "...");
            AddMessage("Você sente uma brisa!");
            UpdateAlertText();
            RandomAction(0);
        }
        else if (hasPit)
        {
            LogRegister("Pit", "Perception", "...");
            AddMessage("Você caiu em um poço!");
            UpdateAlertText();
            Destroy(gameObject);
        }
        else if (hasWumpus && perceptionFound)
        {
            LogRegister("Wumpus", "Perception", "...");
            AddMessage("Você morreu para o Wumpus!");
            UpdateAlertText();
            Destroy(gameObject);
        }
        else if (!perceptionFound)
        {
            LogRegister("Nothing", "Perception", "...");
            AddMessage("Você não sente nada.");
            UpdateAlertText();
            RandomAction(1);
        }
        return hasGold;
    }

    public void AddMessage(string message)
    {
        perceptionMessages.Add(message);

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
            if (i == perceptionMessages.Count - 1)
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

    void ShootArrow(Vector2 direction, string direcao)
    {
        Vector3 arrowPosition = transform.position + new Vector3(direction.x * spacing, direction.y * spacing, 0);
        GameObject arrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        arrowScript.Initialize(direction);

        LogRegister("...", "ShootArrow", direcao);
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

    void RandomAction(int number)
    {
        switch (number)
        {
            case 0:
                BreezerCondition();
                break;
            case 1:
                RandomDirection();
                break;
            case 2:
                if (!hasShotArrow)
                {
                    DirectionArrow();
                }
                else
                {
                    LogRegister("Random", "Perception", "...");
                    AddMessage("Você agiu de forma aleatória e tropeçou em seus próprios passos.");
                    UpdateAlertText();
                    RandomDirection();
                }
                break;
        }
    }

    void RandomDirection()
    {
        int direction = UnityEngine.Random.Range(0, 4);
        Vector2Int directionVector;
        string direcao;
        switch (direction)
        {
            case 0:
                directionVector = Vector2Int.up;
                direcao = "N";
                break;
            case 1:
                directionVector = Vector2Int.down;
                direcao = "S";
                break;
            case 2:
                directionVector = Vector2Int.left;
                direcao = "O";
                break;
            case 3:
                directionVector = Vector2Int.right;
                direcao = "L";
                break;
            default:
                directionVector = Vector2Int.zero;
                direcao = "P";
                break;
        }
        Move(directionVector, direcao);
    }

    void BreezerCondition()
    {
        RandomDirection();
    }

    void DirectionArrow()
    {
        int direction = UnityEngine.Random.Range(0, 4);
        Vector2 directionVector;
        string direcao;
        switch (direction)
        {
            case 0:
                directionVector = Vector2.up;
                direcao = "N";
                break;
            case 1:
                directionVector = Vector2.down;
                direcao = "S";
                break;
            case 2:
                directionVector = Vector2.left;
                direcao = "O";
                break;
            case 3:
                directionVector = Vector2.right;
                direcao = "L";
                break;
            default:
                directionVector = Vector2.zero;
                direcao = "P";
                break;
        }
        ShootArrow(directionVector, direcao);
        SituationArrow();
        hasShotArrow = true;
    }

    void isWumpus(GameObject wumpus)
    {
        if (wumpus != null)
        {
            LogRegister("Kill", "ShootArrow", "...");
            AddMessage("Você matou o Wumpus!");
            UpdateAlertText();
            hasWumpus = false;
            Destroy(wumpus);
        }
    }

    private void InitializeDatabase()
    {
        if (!File.Exists(dbFilePath))
        {
            SqliteConnection.CreateFile(dbFilePath);
        }

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS PlayerActions (Id INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId TEXT, Perception TEXT, Action TEXT, Direction TEXT)";
                command.ExecuteNonQuery();
            }
        }
    }

    public void LogRegister(string perception, string action, string direction)
    {
        Task logTask = Task.Run(() =>
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO PlayerActions (PlayerId, Perception, Action, Direction) VALUES (@playerId, @perception, @action, @direction)";
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@perception", perception);
                    command.Parameters.AddWithValue("@action", action);
                    command.Parameters.AddWithValue("@direction", direction);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        });

        pendingTasks.Add(logTask);

        try
        {
            logTask.Wait();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to log action to database. Error: {ex.Message}");
        }
        finally
        {
            pendingTasks.Remove(logTask);
        }
    }
}