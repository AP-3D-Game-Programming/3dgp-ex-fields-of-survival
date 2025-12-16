using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject canvas;

    bool isOpen;

    void Start()
    {
        canvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isOpen = !isOpen;

            canvas.SetActive(isOpen);

            Cursor.visible = isOpen;
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
