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

public class PlayerMovement : MonoBehaviour
{
    [Header("Config. Player")]
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private Vector3 previousPosition;
    public Vector3 initialPosition;
    private Vector3 lastCheckedPosition;
    private int rows;
    private int columns;
    private float spacing;

    [Header("Config. Dialog")]
    public Text alertText;
    public List<string> perceptionMessages = new List<string>();
    public GameObject arrowPrefab;
    private bool hasShotArrow = false;
    private bool isMoving = false;

    [Header("Contadores - Display")]
    public Text countGold;
    private int numberGold = 0;
    public Text countArrow;
    private int numberArrow = 1;
    private bool hasGold;

    [Header("Config. Logging")]
    public string playerId;
    private string dbFilePath;
    private string connectionString;
    private List<Task> pendingTasks = new List<Task>();

    void Start()
    {
        dbFilePath = Path.Combine(Application.dataPath, "PlayerActionsLog.db");
        connectionString = "URI=file:" + dbFilePath;
        InitializeDatabase();

        countGold.text = numberGold.ToString();
        countArrow.text = numberArrow.ToString();
        targetPosition = transform.position;
        initialPosition = transform.position;
        lastCheckedPosition = targetPosition;
        alertText.text = "";
        isMoving = false;
        hasGold = false;
        CheckForPerceptions(targetPosition);
        StartCoroutine(PlayerSimulation());
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
                if (hasGold && targetPosition == initialPosition)
                {
                    LogRegister("Winner", "Perception", "...");
                    AddMessage("Parabéns, você venceu o jogo!");
                    UpdateAlertText();
                    yield break;
                }

                RandomDirection();
                yield return new WaitForSeconds(.5f);
            }
            yield return null;
        }
    }

    public void Initialize(int rows, int columns, float spacing)
    {
        this.rows = rows;
        this.columns = rows;
        this.spacing = spacing;
    }

    void Move(Vector2Int direction, string directionName)
    {
        Vector3 newPosition = targetPosition + new Vector3(direction.x * spacing, direction.y * spacing, 0);

        if (IsWithinGrid(newPosition))
        {
            previousPosition = targetPosition;
            targetPosition = newPosition;
            LogRegister("...", "Move", directionName);
            isMoving = true;
        }
    }

    bool IsWithinGrid(Vector3 position)
    {
        float gridWidth = columns * spacing;
        float gridHeight = rows * spacing;

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
        bool live = true;

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
                live = false;
                perceptionFound = true;
                LogRegister("Pit", "Perception", "...");
                AddMessage("Você caiu em um poço!");
                UpdateAlertText();
                Destroy(gameObject);
            }
            else if (collider.CompareTag("Wumpus"))
            {
                live = false;
                perceptionFound = true;
                LogRegister("Wumpus", "Perception", "...");
                AddMessage("Você morreu para o Wumpus!");
                UpdateAlertText();
                Destroy(gameObject);
            }
            else if (collider.CompareTag("Gold"))
            {
                if (!hasGold)
                {
                    perceptionFound = true;
                    hasGold = true;
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
        if (live)
        {
            if (foundBreeze && foundStench)
            {
                LogRegister("Breeze and Stench", "Perception", "...");
                AddMessage("Você sente uma brisa e um fedor!");
                UpdateAlertText();
                RandomAction(2);
            }
            else if (foundStench)
            {
                LogRegister("Stench", "Perception", "...");
                AddMessage("Você sente um fedor!");
                UpdateAlertText();
                RandomAction(2);
            }
            else if (foundBreeze)
            {
                LogRegister("Breeze", "Perception", "...");
                AddMessage("Você sente uma brisa!");
                UpdateAlertText();
                RandomAction(0);
            }

            else if (hasGold)
            {
                LogRegister("Light", "Perception", "...");
                    AddMessage("Você percebe um brilho!");
                    UpdateAlertText();
                    LogRegister("Catch", "Perception", "...");
                    AddMessage("Você encontrou o ouro!");
                    UpdateAlertText();
                    //RandomAction(1); // Não deveria chamar RandomAction(1) aqui, a menos que haja uma razão específica
                    numberGold++;
                    countGold.text = numberGold.ToString();
            }
            else if (!perceptionFound)
            {
                LogRegister("Nothing", "Perception", "...");
                AddMessage("Você não sente nada.");
                UpdateAlertText();
                RandomAction(1);
            }
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

    void ShootArrow(Vector2 direction, string directionName)
    {
        Vector3 arrowPosition = transform.position + new Vector3(direction.x * spacing, direction.y * spacing, 0);
        GameObject arrow = Instantiate(arrowPrefab, arrowPosition, Quaternion.identity);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        arrowScript.Initialize(direction);

        LogRegister("...", "ShootArrow", directionName);
        UpdateArrowCount();
    }

    void UpdateArrowCount()
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
                    LogRegister("NoArrow", "Action", "...");
                    AddMessage("Você não possui mais flechas!");
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
        string directionName;
        switch (direction)
        {
            case 0:
                directionVector = Vector2Int.up;
                directionName = "N";
                break;
            case 1:
                directionVector = Vector2Int.down;
                directionName = "S";
                break;
            case 2:
                directionVector = Vector2Int.left;
                directionName = "O";
                break;
            case 3:
                directionVector = Vector2Int.right;
                directionName = "L";
                break;
            default:
                directionVector = Vector2Int.zero;
                directionName = "P";
                break;
        }
        Move(directionVector, directionName);
    }

    void BreezerCondition()
    {
        // Gera um número aleatório entre 0 e 100
        int chance = UnityEngine.Random.Range(0, 100);

        // 80% de chance de retornar à casa anterior
        if (chance < 80)
        {
            Move(Vector2Int.zero, "P"); // Move para a posição anterior
        }
        // 20% de chance de avançar para outra casa
        else
        {
            RandomDirection();
        }
    }


    void DirectionArrow()
    {
        if (numberArrow > 0)
        {
            int direction = UnityEngine.Random.Range(0, 4);
            Vector2 directionVector;
            string directionName;
            switch (direction)
            {
                case 0: directionVector = Vector2.up; directionName = "N"; break;
                case 1: directionVector = Vector2.down; directionName = "S"; break;
                case 2: directionVector = Vector2.left; directionName = "O"; break;
                case 3: directionVector = Vector2.right; directionName = "L"; break;
                default: directionVector = Vector2.zero; directionName = "P"; break;
            }

            ShootArrow(directionVector, directionName);
            UpdateArrowCount();
            hasShotArrow = true;
        }
        else
        {
            AddMessage("Você não possui mais flechas!");
            UpdateAlertText();
        }
    }

    void LogRegister(string perception, string action, string direction)
    {
        Task task = Task.Run(() =>
        {
            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO PlayerActions (PlayerId, Perception, Action, Direction, Timestamp) VALUES (@playerId, @perception, @action, @direction, @timestamp)";
                    using (SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@playerId", playerId);
                        command.Parameters.AddWithValue("@perception", perception);
                        command.Parameters.AddWithValue("@action", action);
                        command.Parameters.AddWithValue("@direction", direction);
                        command.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error logging player action: " + ex.Message);
            }
        });

        pendingTasks.Add(task);
    }

    void InitializeDatabase()
    {
        if (!File.Exists(dbFilePath))
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS PlayerActions (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PlayerId TEXT,
                        Perception TEXT,
                        Action TEXT,
                        Direction TEXT,
                        Timestamp TEXT
                    )";

                using (SqliteCommand command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    void OnDestroy()
    {
        if (pendingTasks.Count > 0)
        {
            Task.WhenAll(pendingTasks).ContinueWith(_ =>
            {
                Application.Quit();
            });
        }
        else
        {
            Application.Quit();
        }
    }
}
