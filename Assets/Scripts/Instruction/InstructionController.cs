using UnityEngine;
using UnityEngine.UI;

namespace Instruction
{
    public class InstructionController : MonoBehaviour
    {
        public GameObject pauseMenu;
        public Button closeButton;

        private void Start()
        {
            closeButton.onClick.AddListener(ResumeGame);
            pauseMenu.SetActive(false);
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (pauseMenu.activeSelf)
                {
                    Time.timeScale = 1;
                    pauseMenu.SetActive(false);
                }
                else
                {
                    Time.timeScale = 0;
                    pauseMenu.SetActive(true);
                }
            }
        }
        private void ResumeGame()
        {
            Time.timeScale = 1;
            pauseMenu.SetActive(false);
        }

        
    }
}
