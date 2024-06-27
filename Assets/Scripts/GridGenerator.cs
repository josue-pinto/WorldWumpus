using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class GridGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    // Referências existentes
    public GameObject gridPrefab;
    public GameObject Wumpus;
    public GameObject Pit;
    public GameObject Gold;
    public GameObject Player;
    public GameObject Breeze;
    public GameObject Stench;

    [Header("Config. Grid")]
    public float pitDensity = 0.1875f;
    private int rows;
    private int columns;
    public float spacing = 1.0f;
    public float minSpacing = 0.5f;
    private Camera mainCamera;
    private CameraController cameraController;

    [Header("Config. Dialog")]
    // Adicione esta linha para referenciar o Canvas a ser ativado
    public Canvas canvasToActivate;
    public Canvas canvasReset; // Canvas do botão resetar
    public Text alertText; // Texto das ações
    public Text countGold; // Contador de ouro
    public Text countArrow; // Contador Flechas
    public Text countWumpus; // Contador de Wumpus mortos
    private int numberWumpus;
    int count = 0;

    [Header("Config. Audio")]
    public AudioSource audiosource;
    public AudioClip wumpusDeathSound;



    void Awake()
    {
        mainCamera = Camera.main;
        audiosource = GetComponent<AudioSource>();
    }

    public void UpdateGridSize(int newRows, int newColumns)
    {
        rows = newRows;
        columns = newColumns;
        ClearGrid();
        GenerateGrid();
    }

    void GenerateGrid()
    {
        Vector3 startPosition = CalculateStartPosition();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 position = new Vector3(startPosition.x + j * spacing, startPosition.y + i * spacing, startPosition.z);
                Instantiate(gridPrefab, position, Quaternion.identity, transform);
            }
        }

        AddPlayer();
        AddRandomElements();
    }

    Vector3 CalculateStartPosition()
    {
        float gridWidth = (columns - 1) * spacing;
        float gridHeight = (rows - 1) * spacing;
        float startX = -gridWidth / 2;
        float startY = -gridHeight / 2;
        return new Vector3(startX, startY, 0);
    }

    public void AddPlayer()
    {
        count++;
        cameraController = Camera.main.GetComponent<CameraController>();
        float gridWidth = (columns - 1) * spacing;
        float gridHeight = (rows - 1) * spacing;
        float startX = -gridWidth / 2;
        float startY = -gridHeight / 2;
        Vector3 playerPosition = new Vector3(startX, startY, 0);
        GameObject playerObject = Instantiate(Player, playerPosition, Quaternion.identity, transform);
        PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
        playerMovement.playerId = $"Player{count}";
        playerMovement.Initialize(rows, columns, spacing);
        playerMovement.alertText = alertText;
        playerMovement.countGold = countGold;
        playerMovement.countArrow = countArrow;
    }

    void AddRandomElements()
    {
        System.Random rand = new System.Random();
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> pitPositions = new HashSet<Vector2Int>();
        occupiedPositions.Add(new Vector2Int(0, 0));

        Vector2Int wumpusPosition = AddElementRandomly(Wumpus, rand, occupiedPositions, Stench, pitPositions);
        int numberOfPits = Mathf.RoundToInt(rows * columns * pitDensity);

        for (int i = 0; i < numberOfPits; i++)
        {
            pitPositions.Add(AddElementRandomly(Pit, rand, occupiedPositions, Breeze, pitPositions));
        }

        // Adicionar Gold com a chance de ocasionalmente ocupar a mesma posição que o Wumpus
        Vector2Int goldPosition;
        do
        {
            goldPosition = new Vector2Int(rand.Next(columns), rand.Next(rows));
        } while (occupiedPositions.Contains(goldPosition) || pitPositions.Contains(goldPosition) || (goldPosition == wumpusPosition && rand.NextDouble() > 0.5));

        Instantiate(Gold, CalculateElementPosition(goldPosition), Quaternion.identity, transform);
        occupiedPositions.Add(goldPosition);
    }

    Vector2Int AddElementRandomly(GameObject elementPrefab, System.Random rand, HashSet<Vector2Int> occupiedPositions, GameObject perceptionPrefab, HashSet<Vector2Int> pitPositions)
    {
        Vector2Int position;

        do
        {
            position = new Vector2Int(rand.Next(columns), rand.Next(rows));
        } while (occupiedPositions.Contains(position) || pitPositions.Contains(position));

        if (elementPrefab != Gold)
        {
            occupiedPositions.Add(position);
        }

        float gridWidth = (columns - 1) * spacing;
        float gridHeight = (rows - 1) * spacing;
        float startX = -gridWidth / 2;
        float startY = -gridHeight / 2;
        Vector3 elementPosition = new Vector3(startX + position.x * spacing, startY + position.y * spacing, 0);
        Instantiate(elementPrefab, elementPosition, Quaternion.identity, transform);

        if (perceptionPrefab != null)
        {
            AddPerceptions(position, perceptionPrefab);
        }

        return position;
    }

    Vector3 CalculateElementPosition(Vector2Int gridPosition)
    {
        float gridWidth = (columns - 1) * spacing;
        float gridHeight = (rows - 1) * spacing;
        float startX = -gridWidth / 2;
        float startY = -gridHeight / 2;
        return new Vector3(startX + gridPosition.x * spacing, startY + gridPosition.y * spacing, 0);
    }

    void AddPerceptions(Vector2Int position, GameObject perceptionPrefab)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int perceptionPosition = position + direction;

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
            numberWumpus = 0;
            countWumpus.text = numberWumpus.ToString();
        }
    }

    public void PlayWumpusDeathSound()
    {
        //if (audiosource != null && wumpusDeathSound != null)
        //{
        audiosource.PlayOneShot(wumpusDeathSound);
        Arrow arrow = FindAnyObjectByType<Arrow>();
        arrow.countWumpus = countWumpus;
        // }
    }

    // Adicione este método para ativar o Canvas
    public void ActivateCanvas()
    {
        if (canvasToActivate != null)
        {
            ClearGrid();
            canvasToActivate.gameObject.SetActive(true);
            canvasReset.gameObject.SetActive(false);
        }
    }
}


