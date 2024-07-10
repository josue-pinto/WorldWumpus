using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

namespace WorldWumpus.Assets
{
    public class GridGeneratorManual : MonoBehaviour
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

        // Adicione referências aos radio buttons
        public Toggle wumpusToggle;
        public Toggle pitToggle;
        public Toggle goldToggle;

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

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int gridPosition = GetGridPosition(mousePosition);

                if (IsValidGridPosition(gridPosition))
                {
                    CheckSelectedPrefab();
                    if (selectedPrefab != null)
                    {
                        CreateElementAtPosition(gridPosition, selectedPrefab);
                        DeselectPrefab(); // Deselecionar o prefab após criar o elemento
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int gridPosition = GetGridPosition(mousePosition);

                if (IsValidGridPosition(gridPosition))
                {
                    RemoveElementAtPosition(gridPosition);
                }
            }
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
            playerMovement.playerId = Convert.ToString($"Player {count}");
            playerMovement.Initialize(rows, columns, spacing);
            playerMovement.alertText = alertText;
            playerMovement.countGold = countGold;
            playerMovement.countArrow = countArrow;
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
            GameObject element = Instantiate(elementPrefab, elementPosition, Quaternion.identity, transform);

            if (elementPrefab == Wumpus)
            {
                GameObject stench = Instantiate(Stench, elementPosition, Quaternion.identity, transform); // Adicionar percepção de fedor na mesma casa do Wumpus
                element.GetComponent<PerceptionTracker>().AddPerception(stench);
            }

            if (perceptionPrefab != null)
            {
                AddPerceptions(position, perceptionPrefab, element);
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

        void AddPerceptions(Vector2Int position, GameObject perceptionPrefab, GameObject element)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (Vector2Int direction in directions)
            {
                Vector2Int perceptionPosition = position + direction;

                if (IsValidGridPosition(perceptionPosition))
                {
                    GameObject perception = Instantiate(perceptionPrefab, CalculateElementPosition(perceptionPosition), Quaternion.identity, transform);
                    element.GetComponent<PerceptionTracker>().AddPerception(perception);
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

        // Este método será chamado quando o botão for clicado
    public void ResetScene()
    {
        // Obtém o nome da cena atual
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Carrega a cena atual novamente
        SceneManager.LoadScene(currentSceneName);
    }

        // Métodos para seleção de prefabs baseados em radio buttons
        void CheckSelectedPrefab()
        {
            if (wumpusToggle.isOn)
            {
                selectedPrefab = Wumpus;
            }
            else if (pitToggle.isOn)
            {
                selectedPrefab = Pit;
            }
            else if (goldToggle.isOn)
            {
                selectedPrefab = Gold;
            }
            else
            {
                selectedPrefab = null;
            }
        }

        void DeselectPrefab()
        {
            // Manter o último prefab selecionado
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
            GameObject element = Instantiate(elementPrefab, CalculateElementPosition(position), Quaternion.identity, transform);
            element.AddComponent<PerceptionTracker>();

            GameObject perceptionPrefab = null;

            if (elementPrefab == Wumpus)
            {
                perceptionPrefab = Stench;
                GameObject stench = Instantiate(Stench, CalculateElementPosition(position), Quaternion.identity, transform); // Adicionar percepção de fedor na casa do Wumpus
                element.GetComponent<PerceptionTracker>().AddPerception(stench);
            }
            else if (elementPrefab == Pit)
            {
                perceptionPrefab = Breeze;
            }

            if (perceptionPrefab != null)
            {
                AddPerceptions(position, perceptionPrefab, element);
            }

            if (elementPrefab == Gold)
            {
                GameObject shine = Instantiate(Shine, CalculateElementPosition(position), Quaternion.identity, transform); // Adicionar percepção de brilho
                element.GetComponent<PerceptionTracker>().AddPerception(shine);
            }
        }

        void RemoveElementAtPosition(Vector2Int position)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(CalculateElementPosition(position), spacing / 2);
            foreach (var collider in colliders)
            {
                GameObject obj = collider.gameObject;
                if (obj.CompareTag("Wumpus") || obj.CompareTag("Pit") || obj.CompareTag("Gold"))
                {
                    PerceptionTracker tracker = obj.GetComponent<PerceptionTracker>();
                    if (tracker != null)
                    {
                        tracker.RemoveAllPerceptions();
                    }
                    Destroy(obj);
                }
            }
        }
    }

    public class PerceptionTracker : MonoBehaviour
    {
        private List<GameObject> perceptions = new List<GameObject>();

        public void AddPerception(GameObject perception)
        {
            perceptions.Add(perception);
        }

        public void RemoveAllPerceptions()
        {
            foreach (var perception in perceptions)
            {
                if (perception != null)
                {
                    Destroy(perception);
                }
            }
            perceptions.Clear();
        }
    }
}
