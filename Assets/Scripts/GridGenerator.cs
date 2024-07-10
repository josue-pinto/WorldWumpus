using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;
using System.Linq;
using WorldWumpus.Assets;

public class GridGenerator : MonoBehaviour
{
    // Referências existentes
    public GameObject gridPrefab;
    public GameObject Wumpus;
    public GameObject Pit;
    public GameObject Gold;
    public GameObject Player;
    public GameObject Breeze;
    public GameObject Stench;
    public GameObject Shine; // Prefab de percepção de brilho
    public GameObject InitialGame; // Prefab inicial do jogo
    public float pitDensity = 0.1875f;
    private int rows;
    private int columns;
    public float spacing = 1.0f;
    public float minSpacing = 0.5f;
    public int count = 0;
    public int iterations = 0;
    private Camera mainCamera;
    public Text alertText; // Texto das ações
    public Text countGold; // Contador de ouro
    public Text countArrow; // Contador Flechas
    public Text countWumpus; // Contador de Wumpus mortos
    private int numberWumpus;
    private CameraController cameraController;
    public AudioSource audiosource;
    public AudioClip wumpusDeathSound;

    // Adicione esta linha para referenciar o Canvas a ser ativado
    public Canvas canvasToActivate;
    public Canvas canvasReset; // Canvas do botão resetar

    // Prefab selecionado
    private GameObject selectedPrefab;

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

        // Adicionar prefab na posição inicial (0,0)
        Instantiate(InitialGame, CalculateElementPosition(new Vector2Int(0, 0)), Quaternion.identity, transform);

        //AddPlayer();
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

    public void AddPlayer(int option)
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

        

                    playerMovement.playerId = Convert.ToString($"Player {count}");
                    playerMovement.Initialize(rows, columns, spacing);
                    playerMovement.alertText = alertText;
                    playerMovement.countGold = countGold;
                    playerMovement.countArrow = countArrow;
                    Debug.Log("Agent1");
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
        Instantiate(Shine, CalculateElementPosition(goldPosition), Quaternion.identity, transform); // Adicionar percepção de brilho
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

        if (elementPrefab == Wumpus)
        {
            Instantiate(Stench, elementPosition, Quaternion.identity, transform); // Adicionar percepção de fedor na mesma casa do Wumpus
        }

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
        if (audiosource != null && wumpusDeathSound != null)
        {
            audiosource.PlayOneShot(wumpusDeathSound);
            Arrow arrow = FindAnyObjectByType<Arrow>();
            arrow.countWumpus = countWumpus;
        }
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

    // Adicione métodos para seleção e deseleção de prefabs
    public void SelectPrefab(GameObject prefab)
    {
        selectedPrefab = prefab;
    }

    public void DeselectPrefab()
    {
        selectedPrefab = null;
    }

    public void SelectWumpus()
    {
        SelectPrefab(Wumpus);
    }

    public void SelectPit()
    {
        SelectPrefab(Pit);
    }

    public void SelectGold()
    {
        SelectPrefab(Gold);
    }

    Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        float gridWidth = (columns - 1) * spacing;
        float gridHeight = (rows - 1) * spacing;
        float startX = -gridWidth / 2;
        float startY = -gridHeight / 2;

        int x = Mathf.RoundToInt((worldPosition.x - startX) / spacing);
        int y = Mathf.RoundToInt((worldPosition.y - startY) / spacing);

        return new Vector2Int(x, y);
    }

    bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < columns && gridPosition.y >= 0 && gridPosition.y < rows;
    }

    void CreateElementAtPosition(Vector2Int position, GameObject elementPrefab)
    {
        GameObject perceptionPrefab = null;

        if (elementPrefab == Wumpus)
        {
            perceptionPrefab = Stench;
        }
        else if (elementPrefab == Pit)
        {
            perceptionPrefab = Breeze;
        }

        Instantiate(elementPrefab, CalculateElementPosition(position), Quaternion.identity, transform);

        if (perceptionPrefab != null)
        {
            AddPerceptions(position, perceptionPrefab);
        }

        if (elementPrefab == Gold)
        {
            Instantiate(Shine, CalculateElementPosition(position), Quaternion.identity, transform); // Adicionar percepção de brilho
        }
    }
}
