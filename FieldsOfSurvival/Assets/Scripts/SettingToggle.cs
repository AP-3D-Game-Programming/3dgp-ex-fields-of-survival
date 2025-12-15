using UnityEngine;

public class SettingsToggle : MonoBehaviour
{
    public GameObject panelA;
    public GameObject panelB;

    bool isOpen = false;

    void Start()
    {
        panelA.SetActive(false);
        panelB.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isOpen = !isOpen;

            panelA.SetActive(isOpen);
            panelB.SetActive(isOpen);

            Cursor.visible = isOpen;
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
