using UnityEngine;
using UnityEngine.UI;

namespace WorldWumpus.Assets
{
    public class GridUIManagerScene : MonoBehaviour
    {
        public InputField rowsInput;
        public InputField columnsInput;
        public Button generateButton;
        public GridGeneratorManual gridGenerator;
        public Canvas uiCanvas;  // Referência ao Canvas da UI
        public Canvas canvasReset; // Canvas do botão reset Mapa

        void Start()
        {
            generateButton.onClick.AddListener(OnGenerateButtonClicked);
        }

        void OnGenerateButtonClicked()
        {
            int rows;
            int columns;

            // Validar e converter os valores de entrada
            if (int.TryParse(rowsInput.text, out rows) && int.TryParse(columnsInput.text, out columns))
            {
                gridGenerator.UpdateGridSize(rows, columns);
                // Desativar o Canvas após gerar a grid
                uiCanvas.gameObject.SetActive(false);
                canvasReset.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Invalid input for rows or columns.");
            }
        }
    }
}
