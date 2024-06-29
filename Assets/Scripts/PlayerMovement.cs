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
using Random = UnityEngine.Random; // ou System.Random


public class PlayerMovement : MonoBehaviour
{
    [Header("Config. Player")]
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private Vector3 initialPosition;
    private Vector3 previousPosition;
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
    public string gene;
    private string dbFilePath;
    private string connectionString;
    private List<Task> pendingTasks = new List<Task>();  // Lista de tarefas pendentes

    void Start()
    {
        // Inicializa a conexão/criação do banco de dados
        dbFilePath = Path.Combine(Application.dataPath, "PlayerActionsLog.db");
        connectionString = "URI=file:" + dbFilePath;
        InitializeDatabase();

        // Declara os contadores iniciais
        countGold.text = numberGold.ToString();
        countArrow.text = numberArrow.ToString();
        targetPosition = transform.position;
        initialPosition = targetPosition;
        alertText.text = "";
        //CheckForPerceptions(targetPosition);
        isMoving = false;
        hasGold = false;
        hasWumpus = true;
        inicio = "I";
    }

    void FixedUpdate()
    {
        if (!isMoving)
        {
            CheckForPerceptions(targetPosition);
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed);
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
            LogRegister("...", "Move", direcao); // Registrar o movimento aqui
            Chromosome(direcao); //criar cromossomo de movimento de direção
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

    public bool CheckForPerceptions(Vector3 position)
    {
        isMoving = true;

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
                    Chromosome("TG"); //took the gold
                    AgentTheBest(); //gravando o material genético do melhor agente até o momento
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
            Chromosome("FP"); //Fell into the pit

            UpdateAlertText();
            Destroy(gameObject);
        }
        else if (hasWumpus && perceptionFound)
        {
            LogRegister("Wumpus", "Perception", "...");
            Chromosome("KbW"); //Gravando cromossomo, KbW (killed by the Wumpus)
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
        isMoving = false;
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
                    isWumpus(wumpus);
                }
                else
                {
                    LogRegister("Not Arrow", "DirectionArrow", "...");
                    Chromosome("NA"); //gravando cromossomo NA (Not Arrow)
                    AddMessage("Você não possui mais flechas.");
                    UpdateAlertText();
                    RandomDirection();
                }
                break;
        }
    }

    void RandomDirection()
    {
        int direction = Random.Range(0, 4);
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
        int situation = Random.Range(0, 2);

        switch (situation)
        {
            case 0:
                LogRegister("Pause", "Perception", "...");
                AddMessage("Você parou.");
                UpdateAlertText();
                break;
            case 1:
                RandomDirection();
                break;
        }
    }

    void DirectionArrow()
    {
        int direction = Random.Range(0, 4);
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
            Chromosome("KW"); // gravando cromossomo KW (Killed the Wumpus)
            AgentTheBest(); //gravando o material genético do melhor agente até o momento
            Selection(); //gravando a melhor seleção, que deu menos passos
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
                command.CommandText = "CREATE TABLE IF NOT EXISTS PlayerActions (Id INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId TEXT, Perception TEXT, Action TEXT, Direction TEXT, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
                command.ExecuteNonQuery();
            }
            connection.Close();

            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS Chromosome (Id INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId TEXT, Gene TEXT, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
                command.ExecuteNonQuery();
            }
            connection.Close();

            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS AgentTheBest (Id INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId TEXT, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
                command.ExecuteNonQuery();
            }
            connection.Close();

            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS CrossOver (Id INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId TEXT, Gene TEXT, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
                command.ExecuteNonQuery();
            }
            connection.Close();

            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS Selection (Id INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId TEXT,Steps NUMBER, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
                command.ExecuteNonQuery();
            }
            connection.Close();
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
        catch (Exception ex)  // Exceção adicionada aqui
        {
            Debug.LogError($"Failed to log action to database. Error: {ex.Message}");
        }
        finally
        {
            pendingTasks.Remove(logTask);
        }
    }

    //Criando tabela do Cromossomo para fazer a reproducao e mutação posteriormente.
    public void Chromosome(string gene)
    {
        Task createChromosome = Task.Run(() =>
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Chromosome (PlayerId, Gene) VALUES (@playerId, @gene)";
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@gene", gene);  
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        });

        pendingTasks.Add(createChromosome);

        try
        {
            createChromosome.Wait();
        }
        catch (Exception ex)  // Exceção adicionada aqui
        {
            Debug.LogError($"Failed to Chromosome to database. Error: {ex.Message}");
        }
        finally
        {
            pendingTasks.Remove(createChromosome);
        }
    }

    //Criando tabela de Melhores Agentes (AgentTheBest), quem pegou o ouro, matou o wumpus e conseguiu sair* (falta o sair)
    public void AgentTheBest()
    {
        Task createAgentTheBest = Task.Run(() =>
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO AgentTheBest (PlayerId) VALUES (@playerId)";
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        });

        pendingTasks.Add(createAgentTheBest);

        try
        {
            createAgentTheBest.Wait();
        }
        catch (Exception ex)  // Exceção adicionada aqui
        {
            Debug.LogError($"Failed to AgentTheBest to database. Error: {ex.Message}");
        }
        finally
        {
            pendingTasks.Remove(createAgentTheBest);
        }
    }

    //Criando a tabela de CrossOver de 1 ponto, isto é trocar um Gene entre dois Pais e criar um novo ser
    public void CrossOver()
    {
        Task createCrossOver = Task.Run(() =>
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO CrossOver (PlayerId) VALUES (@playerId)";
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        });

        pendingTasks.Add(createCrossOver);

        try
        {
            createCrossOver.Wait();
        }
        catch (Exception ex)  // Exceção adicionada aqui
        {
            Debug.LogError($"Failed to CrossOver to database. Error: {ex.Message}");
        }
        finally
        {
            pendingTasks.Remove(createCrossOver);
        }
    }

    //Criando a tabela de Seleção dos menores passos, função fitness
    public void Selection()
    {
        Task createSelection = Task.Run(() =>
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Selection (PlayerId,Steps)  SELECT PlayerId, count(playerid)  from CrossOver group by PlayerId";
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        });

        pendingTasks.Add(createSelection);

        try
        {
            createSelection.Wait();
        }
        catch (Exception ex)  // Exceção adicionada aqui
        {
            Debug.LogError($"Failed to CrossOver to database. Error: {ex.Message}");
        }
        finally
        {
            pendingTasks.Remove(createSelection);
        }
    }
}
