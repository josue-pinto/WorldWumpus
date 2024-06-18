using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    public GameObject gridPrefab;  // Prefab do elemento da grid
    public GameObject Wumpus; // Prefab do elemento Wumpus
    public GameObject Pit; // Prefab do elemento Pit
    public GameObject Gold; // Prefab do elemento Gold
    public GameObject Player; // Prefab do elemento Player
    public GameObject Breeze; // Prefab do elemento Breeze
    public GameObject Stench; // Prefab do elemento Stench
    public float pitDensity = 0.1875f; // Densidade de pits
    private int rows;  // Número de linhas na grid
    private int columns;  // Número de colunas na grid
    public float spacing = 1.0f;  // Espaçamento inicial entre os elementos da grid
    public float minSpacing = 0.5f;  // Espaçamento mínimo entre os elementos da grid
    private Camera mainCamera;
    public Text alertText;  // Referência ao texto de alerta da UI
    private CameraController cameraController;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    public void UpdateGridSize(int newRows, int newColumns)
    {
        // Atualiza as dimensões
        rows = newRows;
        columns = newColumns;

        // Limpa a grid existente
        ClearGrid();

        // Gera a nova grid
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Calcular a posição inicial para centralizar a grid
        Vector3 startPosition = CalculateStartPosition();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 position = new Vector3(startPosition.x + j * spacing, startPosition.y + i * spacing, startPosition.z);
                Instantiate(gridPrefab, position, Quaternion.identity, transform);
            }
        }

        // Adicionar o jogador na posição (0,0)
        AddPlayer();

        // Adicionar elementos aleatórios
        AddRandomElements();
    }


    Vector3 CalculateStartPosition()
    {
        // Calcular o tamanho total da grid
        float gridWidth = (columns - 1) * spacing;
        float gridHeight = (rows - 1) * spacing;

        // Calcular a posição inicial para centralizar a grid
        float startX = -gridWidth / 2;
        float startY = -gridHeight / 2;

        return new Vector3(startX, startY, 0);
    }

    void AddPlayer()
    {
        // Supondo que o objeto da câmera tenha o componente CameraController
        cameraController = Camera.main.GetComponent<CameraController>();

        float gridWidth = (columns - 1) * spacing;
        float gridHeight = (rows - 1) * spacing;

        float startX = -gridWidth / 2;
        float startY = -gridHeight / 2;

        Vector3 playerPosition = new Vector3(startX, startY, 0);
        GameObject playerObject = Instantiate(Player, playerPosition, Quaternion.identity, transform);
        // Define o jogador como o alvo da câmera
        cameraController.SetTarget(playerObject.transform);

        // Inicializar o script de movimento do jogador
        PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
        playerMovement.Initialize(rows, columns, spacing);
        playerMovement.alertText = alertText;  // Passar a referência do texto de alerta
    }

    void AddRandomElements()
    {
        System.Random rand = new System.Random();
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

        // Marcar a posição (0,0) como ocupada
        occupiedPositions.Add(new Vector2Int(0, 0));

        // Adicionar Wumpus
        AddElementRandomly(Wumpus, rand, occupiedPositions, Stench);

        // Calcular o número de poços com base na densidade
        int numberOfPits = Mathf.RoundToInt(rows * columns * pitDensity);

        // Adicionar Poços
        for (int i = 0; i < numberOfPits; i++)
        {
            AddElementRandomly(Pit, rand, occupiedPositions, Breeze);
        }

        // Adicionar Ouro
        AddElementRandomly(Gold, rand, occupiedPositions, null);
    }

    void AddElementRandomly(GameObject elementPrefab, System.Random rand, HashSet<Vector2Int> occupiedPositions, GameObject perceptionPrefab)
    {
        Vector2Int position;
        do
        {
            position = new Vector2Int(rand.Next(columns), rand.Next(rows));
        } while (occupiedPositions.Contains(position));

        occupiedPositions.Add(position);

        float gridWidth = (columns - 1) * spacing;
        float gridHeight = (rows - 1) * spacing;

        float startX = -gridWidth / 2;
        float startY = -gridHeight / 2;

        Vector3 elementPosition = new Vector3(startX + position.x * spacing, startY + position.y * spacing, 0);
        Instantiate(elementPrefab, elementPosition, Quaternion.identity, transform);

        // Adicionar percepções se o prefab de percepção não for nulo
        if (perceptionPrefab != null)
        {
            AddPerceptions(position, perceptionPrefab);
        }
    }

    void AddPerceptions(Vector2Int position, GameObject perceptionPrefab)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int perceptionPosition = position + direction;

            // Verificar se a posição está dentro dos limites da grid
            if (perceptionPosition.x >= 0 && perceptionPosition.x < columns && perceptionPosition.y >= 0 && perceptionPosition.y < rows)
            {
                float gridWidth = (columns - 1) * spacing;
                float gridHeight = (rows - 1) * spacing;

                float startX = -gridWidth / 2;
                float startY = -gridHeight / 2;

                Vector3 perceptionWorldPosition = new Vector3(startX + perceptionPosition.x * spacing, startY + perceptionPosition.y * spacing, 0);
                Instantiate(perceptionPrefab, perceptionWorldPosition, Quaternion.identity, transform);
            }
        }
    }

    void ClearGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
