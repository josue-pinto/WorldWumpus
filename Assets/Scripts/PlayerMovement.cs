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
<<<<<<< HEAD
using Random = UnityEngine.Random; // ou System.Random

=======
>>>>>>> upstream/main

public class PlayerMovement : MonoBehaviour
{
    [Header("Config. Player")]
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
<<<<<<< HEAD
    private Vector3 initialPosition;
    private Vector3 previousPosition;
=======
    private Vector3 previousPosition;
    private Vector3 lastCheckedPosition;  // Nova variável
>>>>>>> upstream/main
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
<<<<<<< HEAD
     //add os 170 para piscina de cruzamento
    //    if (playerId == "Player200")
      //  {
        //    Pool();
       // }
    

    public string gene;
=======
>>>>>>> upstream/main
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
<<<<<<< HEAD
        initialPosition = targetPosition;
        alertText.text = "";
        //CheckForPerceptions(targetPosition);
        isMoving = false;
        hasGold = false;
        hasWumpus = true;
        inicio = "I";
=======
        lastCheckedPosition = targetPosition;  // Inicializa com a posição inicial do jogador
        alertText.text = "";
        isMoving = false;
        hasGold = false;
        hasWumpus = true;
        CheckForPerceptions(targetPosition);
        StartCoroutine(PlayerSimulation());  // Inicia a simulação do jogador
>>>>>>> upstream/main
    }

    void FixedUpdate()
    {
<<<<<<< HEAD
        if (!isMoving)
        {
            CheckForPerceptions(targetPosition);
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed);
=======
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
>>>>>>> upstream/main
    }

    public void Initialize(int rows, int columns, float spacing)
    {
        this.rows = rows;
        this.columns = rows;
        this.spacing = spacing;
    }

    void Move(Vector2Int direction, string direcao)
    {
        Vector3 newPosition = targetPosition + new Vector3(direction.x * spacing, direction.y * spacing, 0);

        if (IsWithinGrid(newPosition))
        {
            previousPosition = targetPosition;
            targetPosition = newPosition;
<<<<<<< HEAD
            LogRegister("...", "Move", direcao); // Registrar o movimento aqui
            Chromosome(direcao,-1); //criar cromossomo de movimento de direção
            CheckForPerceptions(newPosition);
=======
            LogRegister("...", "Move", direcao);  // Registrar o movimento aqui
            isMoving = true;
>>>>>>> upstream/main
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
<<<<<<< HEAD
                   // RandomAction(1);
=======
                    RandomAction(1);
>>>>>>> upstream/main
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
<<<<<<< HEAD
                    Chromosome("TG",+1000); //took the gold
                    AgentTheBest(); //gravando o material genético do melhor agente até o momento
                  //  PoolMutationtoEvaluate(playerId);//Remover depois somente para testar
=======
>>>>>>> upstream/main
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
<<<<<<< HEAD
        //Não posso entrar aqui se for mutante, meter condicionamento
=======

>>>>>>> upstream/main
        if (foundBreeze && foundStench && !hasPit)
        {
            LogRegister("Breeze and Stench", "Perception", "...");
            AddMessage("Você sente uma brisa e um fedor!");
            UpdateAlertText();
<<<<<<< HEAD
          //  RandomAction(2);
=======
            RandomAction(2);
>>>>>>> upstream/main
        }
        else if (foundStench && !hasPit)
        {
            LogRegister("Stench", "Perception", "...");
            AddMessage("Você sente um fedor!");
            UpdateAlertText();
<<<<<<< HEAD
            //RandomAction(2);
=======
            RandomAction(2);
>>>>>>> upstream/main
        }
        else if (foundBreeze && !hasPit)
        {
            LogRegister("Breeze", "Perception", "...");
            AddMessage("Você sente uma brisa!");
            UpdateAlertText();
<<<<<<< HEAD
           // RandomAction(0);
=======
            RandomAction(0);
>>>>>>> upstream/main
        }
        else if (hasPit)
        {
            LogRegister("Pit", "Perception", "...");
            AddMessage("Você caiu em um poço!");
<<<<<<< HEAD
            Chromosome("FP",-1000); //Fell into the pit
            //mover para o fim dos 200 agentes deixei aqui para facilitar a análise
            CrossOver();
            Pool();
            Evaluate();
            Selection();
            PoolMutation();
            PoolMutationtoEvaluate(playerId);
=======
>>>>>>> upstream/main
            UpdateAlertText();
            Destroy(gameObject);
        }
        else if (hasWumpus && perceptionFound)
        {
            LogRegister("Wumpus", "Perception", "...");
<<<<<<< HEAD
            Chromosome("KbW",-1000); //Gravando cromossomo, KbW (killed by the Wumpus)
            AddMessage("Você morreu para o Wumpus!");
            //mover para o fim dos 200 agentes deixei aqui para facilitar a análise
            CrossOver();
            Pool();
            Evaluate();
            Selection();
            PoolMutation();

            // PoolMutationtoEvaluate(playerId);
=======
            AddMessage("Você morreu para o Wumpus!");
>>>>>>> upstream/main
            UpdateAlertText();
            Destroy(gameObject);
        }
        else if (!perceptionFound)
        {
            LogRegister("Nothing", "Perception", "...");
            AddMessage("Você não sente nada.");
            UpdateAlertText();
<<<<<<< HEAD
           // RandomAction(1);
        }
        isMoving = false;
=======
            RandomAction(1);
        }
>>>>>>> upstream/main
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
<<<<<<< HEAD
                    isWumpus(wumpus);
                }
                else
                {
                    LogRegister("Not Arrow", "DirectionArrow", "...");
                    Chromosome("NA",-1); //gravando cromossomo NA (Not Arrow)
                    AddMessage("Você não possui mais flechas.");
=======
                }
                else
                {
                    LogRegister("Random", "Perception", "...");
                    AddMessage("Você agiu de forma aleatória e tropeçou em seus próprios passos.");
>>>>>>> upstream/main
                    UpdateAlertText();
                    RandomDirection();
                }
                break;
        }
    }

    void RandomDirection()
    {
<<<<<<< HEAD
        Vector2Int directionVector;
        string direcao;
        //colocando a rota dos mutantes para avaliar parei aqui 29 de julho, preciso alimentar o vetor com os dados da base de dados
        if (playerId == "Player2000")
        {
            string[] genes = PoolMutationtoEvaluate(playerId);
            
            foreach (string gene in genes)
            {
                //Passando a rota do mutante
                switch (gene)
                {
                    case "N":
                        directionVector = Vector2Int.up;
                        direcao = "N";
                        break;

                    case "S":
                        directionVector = Vector2Int.down;
                        direcao = "S";
                        break;

                    case "O":
                        directionVector = Vector2Int.left;
                        direcao = "O";
                        break;
                    case "L":
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

        }

        else

        {
        //aqui ta pegando um caminho aleatório, somente pode ser usado na primeira rodada
        int direction = Random.Range(0, 4);
        
=======
        int direction = UnityEngine.Random.Range(0, 4);
        Vector2Int directionVector;
        string direcao;
>>>>>>> upstream/main
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
<<<<<<< HEAD
        }
=======
>>>>>>> upstream/main
    }

    void BreezerCondition()
    {
<<<<<<< HEAD
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
=======
         RandomDirection();           
>>>>>>> upstream/main
    }

    void DirectionArrow()
    {
<<<<<<< HEAD
        int direction = Random.Range(0, 4);
=======
        int direction = UnityEngine.Random.Range(0, 4);
>>>>>>> upstream/main
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
<<<<<<< HEAD
            Chromosome("KW", +1000); // gravando cromossomo KW (Killed the Wumpus)
            //AgentTheBest(); //gravando o material genético do melhor agente até o momento
            //Pool(); //TESTE PISCINA
            //Evaluate(); //gravando a avaliacaõa para posterior selecao
            //Selection(); //gravando a selecao para posterior cruzamento
=======
>>>>>>> upstream/main
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
<<<<<<< HEAD
            connection.Close();
            //FALTA CRIAR O METODO QUE SAIU COM SUCESSO OU SEJA VOLTOU A 0.0
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS Chromosome (Id INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId TEXT, Gene TEXT, Weight SINGLE, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
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

            /*
             * Falta criar a formula e ou estrategia de cruzamento
             * |
            */
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
                command.CommandText = "CREATE TABLE IF NOT EXISTS Evaluate (Id INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId TEXT,Fitness NUMBER, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
                command.ExecuteNonQuery();
            }
            connection.Close();

            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS Pool (Id INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId TEXT, Gene TEXT, Weight SINGLE, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
                command.ExecuteNonQuery();
            }
            connection.Close();
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS PoolMutation (Id INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId TEXT, Gene TEXT, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
                command.ExecuteNonQuery();
            }
            connection.Close();
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS Selection (Id INTEGER PRIMARY KEY AUTOINCREMENT, PlayerId TEXT, Fitness TEXT, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
                command.ExecuteNonQuery();
            }
            connection.Close();
=======
>>>>>>> upstream/main
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
<<<<<<< HEAD
        catch (Exception ex)  // Exceção adicionada aqui
=======
        catch (Exception ex)
>>>>>>> upstream/main
        {
            Debug.LogError($"Failed to log action to database. Error: {ex.Message}");
        }
        finally
        {
            pendingTasks.Remove(logTask);
        }
    }
<<<<<<< HEAD

    //Criando tabela do Cromossomo para fazer a reproducao e mutação posteriormente.
    public void Chromosome(string gene, Single weight)
    {
        Task createChromosome = Task.Run(() =>
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Chromosome (PlayerId, Gene, Weight) VALUES (@playerId, @gene, @weight)";
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@gene", gene);
                    command.Parameters.AddWithValue("@weight", weight);
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

    //Criando tabela de Melhores Agentes (AgentTheBest), quem pegou o ouro, matou o wumpus e conseguiu sair* (falta o sair) passo 5 melhor individuo I3 do slide
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

    // primeira rodada pega somente 100 pois no metodo de selecao descarta os piores 100
    //Criando tabela de Piscina para cruzamento dos 170(85%) (total de 200) os 15 % serao usados aula 11 1:41 passo 4 substituir os individuos pelos novos misturando 85% + 15% ordena e elimina

    public void Pool()
    {
        Task createPool = Task.Run(() =>
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    //add os 170 para piscina de cruzamento
                    //if (playerId == "Player2")
                    //{
                    // PlayerId limit 170 
                    command.CommandText = "INSERT INTO Pool (PlayerId, Gene , Weight )  SELECT  PlayerId,Gene , Weight from Chromosome WHERE PlayerId in (SELECT PlayerId from Selection group by PlayerId limit 170 )";
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.ExecuteNonQuery();
                    //}
                }
                connection.Close();
            }
        });

        pendingTasks.Add(createPool);

        try
        {
            createPool.Wait();
        }
        catch (Exception ex)  // Exceção adicionada aqui
        {
            Debug.LogError($"Failed to CreatePool to database. Error: {ex.Message}");
        }
        finally
        {
            pendingTasks.Remove(createPool);
        }
    }
    //Criando a tabela de CrossOver de 1 ponto, isto é trocar um Gene entre dois Pais e criar um novo ser sem erros (morreu ou caiu no poço) 3.2.1 - Aula 11 - 1:37
    public void CrossOver()
    {
        Task createCrossOver = Task.Run(() =>
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    //Ando dois passos para trás para pegar o Gen bom, isto é: "Não pegar o passo da morte"
                    command.CommandText = "INSERT INTO CrossOver(PlayerId, Gene)  SELECT PlayerId, Gene  FROM Pool where id in (SELECT id - 2 from Pool where Weight = '-1000'   ) ";
                    //command.CommandText = "DELETE CrossOver(PlayerId, Gene)  SELECT PlayerId, Gene  FROM Pool where id in (SELECT id - 1 from Pool where Weight = '-1000'   ) ";
                    //Permutar dois agentes Mutação 3.3.1 aula 11 1h37m

                    //command.Parameters.AddWithValue("@playerId", playerId);
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

    //Criando a tabela de piscina de mutacao de 1 ponto, Permutar dois agentes Mutação 3.3.1 aula 11 1h37m isto é trocar um Gene entre dois Pais 
    public void PoolMutation()
    {
        Task createPoolMutation = Task.Run(() =>
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO PoolMutation(PlayerId, Gene)  SELECT PlayerId, Gene  FROM CrossOver where id in (SELECT id - 1 from Pool where Weight = '-1000'   ) ";
                    // eliminar a metede depois da mutacao?

                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        });

        pendingTasks.Add(createPoolMutation);

        try
        {
            createPoolMutation.Wait();
        }
        catch (Exception ex)  // Exceção adicionada aqui
        {
            Debug.LogError($"Failed to PoolMutation to database. Error: {ex.Message}");
        }
        finally
        {
            pendingTasks.Remove(createPoolMutation);
        }
    }
    // Apos Mutacao avaliar novamente
    //pegando array do novo cromossomo após a mutação para rodar na table 4 x 4 e avaliar depois com fitness
    /* public void PoolMutationtoEvaluate(string playerId)
     {
         Task createPoolMutationtoEvaluate = Task.Run(() =>
         {
             using (var connection = new SqliteConnection(connectionString))
             {
                 connection.Open();
                 using (var command = connection.CreateCommand())
                 {
                     command.CommandText = "SELECT Gene FROM  PoolMutation WHERE PlayerId = @playerId  ";
                     command.Parameters.AddWithValue("@playerId", playerId);
                     command.ExecuteNonQuery();

                     // Array para armazenar os dados
                     string[] resultados;
                     var reader = command.ExecuteReader();
                     // Contar o número de linhas
                     int count = 0;
                     while (reader.Read())
                     {
                         count++;
                     }

                     // Inicializar o array com o tamanho correto
                     resultados = new string[count];
                     connection.Close();
                     //reader = command.ExecuteReader();
                     int index = 0;
                     while (reader.Read())
                     {
                         resultados[index] = reader["Gene"].ToString();
                         index++;
                     }

                     reader.Close();
                     // Exibir os resultados
                     foreach (var item in resultados)
                     {
                         Console.WriteLine(item);
                        // gene = item;

                     }
                 }

                 connection.Close();

             }
         });

         pendingTasks.Add(createPoolMutationtoEvaluate);

         try
         {
             createPoolMutationtoEvaluate.Wait();
         }
         catch (Exception ex)  // Exceção adicionada aqui
         {
             Debug.LogError($"Failed to PoolMutationtoEvaluate to database. Error: {ex.Message}");
         }
         finally
         {
             pendingTasks.Remove(createPoolMutationtoEvaluate);
         }
     }
    */
    /*
    public async Task<string[]> PoolMutationtoEvaluateAsync(string playerId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Gene FROM PoolMutation WHERE PlayerId = @playerId";
                command.Parameters.AddWithValue("@playerId", playerId);

                var reader = await command.ExecuteReaderAsync();
                var results = new List<string>();

                while (await reader.ReadAsync())
                {
                    results.Add(reader["Gene"].ToString());
                }

                reader.Close();
                return results.ToArray();
            }
        }
    }
    */

    public string[] PoolMutationtoEvaluate(string playerId)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Gene FROM PoolMutation WHERE PlayerId = @playerId";
                command.Parameters.AddWithValue("@playerId", playerId);

                var reader = command.ExecuteReader();
                var results = new List<string>();

                while (reader.Read())
                {
                    results.Add(reader["Gene"].ToString());
                }

                reader.Close();
                return results.ToArray();
            }
        }
    }


    //Criando a tabela de Avaliacao para posterior Seleção com  função fitness calculada (+4) + (-7) = 3 -- aula 11 time 1:24:14
    public void Evaluate()
    {
        Task createEvaluate = Task.Run(() =>
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Evaluate (PlayerId,Fitness)  SELECT PlayerId, sum(Weight) from Chromosome GROUP by PlayerId";
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        });

        pendingTasks.Add(createEvaluate);

        try
        {
            createEvaluate.Wait();
        }
        catch (Exception ex)  // Exceção adicionada aqui
        {
            Debug.LogError($"Failed to Evaluate to database. Error: {ex.Message}");
        }
        finally
        {
            pendingTasks.Remove(createEvaluate);
        }
    }

    //Criando a tabela de  Seleção para posterior cruzamento pega 2 e comparada, meti o corte de 100 e mata os 100 piores = 200
    public void Selection()
    {
        Task createSelection = Task.Run(() =>
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    //DESC limit 100
                    command.CommandText = "INSERT INTO Selection (PlayerId,Fitness)  SELECT PlayerId, Fitness from Evaluate GROUP by PlayerId ORDER by 2 DESC limit 100";
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
            Debug.LogError($"Failed to Selection to database. Error: {ex.Message}");
        }
        finally
        {
            pendingTasks.Remove(createSelection);
        }
    }
=======
>>>>>>> upstream/main
}
