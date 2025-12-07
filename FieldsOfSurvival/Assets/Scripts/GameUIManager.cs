using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameUIPanel; //main panel to show/hide the UI
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject timerPanel;

    [Header("Phase Display Settings")]
    [SerializeField] private Color plantPhaseColor = Color.green;
    [SerializeField] private Color defensePhaseColor = Color.red;

    [Header("Timer Warning Settings")]
    [SerializeField] private float warningThreshold = 10f;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private bool enableTimerPulse = true;

    private bool isWarning = false;
    private bool isCritical = false;

    #region Manager_logic
    private void Start()
    {
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlantPhaseStart.AddListener(OnPlantPhaseStarted);
            GameManager.Instance.OnDefensePhaseStart.AddListener(OnDefensePhaseStarted);
            GameManager.Instance.OnRoundChanged.AddListener(OnRoundChanged);
        }

        UpdateUI();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlantPhase())
        {
            UpdateTimer();
        }
    }

    private void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        // Update round display
        if (roundText != null)
        {
            roundText.text = $"Round: {GameManager.Instance.CurrentRound}";
        }

        // Update phase display
        if (phaseText != null)
        {
            string phaseName = GameManager.Instance.IsPlantPhase() ? "PLANT PHASE" : "DEFENSE PHASE";
            phaseText.text = phaseName;
            phaseText.color = GameManager.Instance.IsPlantPhase() ? plantPhaseColor : defensePhaseColor;
        }

        // Show/hide timer panel
        if (timerPanel != null)
        {
            timerPanel.SetActive(GameManager.Instance.IsPlantPhase());
        }
    }

    private void UpdateTimer()
    {
        if (timerText == null) return;

        float timeRemaining = GameManager.Instance.PlantPhaseTimeRemaining;

        // Format timer as MM:SS
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        // Apply warning colors
        if (timeRemaining <= 5f && !isCritical)
        {
            isCritical = true;
            isWarning = false;
            timerText.color = criticalColor;
        }
        else if (timeRemaining <= warningThreshold && timeRemaining > 5f && !isWarning)
        {
            isWarning = true;
            isCritical = false;
            timerText.color = warningColor;
        }
        else if (timeRemaining > warningThreshold && (isWarning || isCritical))
        {
            isWarning = false;
            isCritical = false;
            timerText.color = Color.white;
        }

        // Optional pulse effect for critical time
        if (enableTimerPulse && isCritical)
        {
            float pulse = Mathf.PingPong(Time.time * 3f, 1f);
            timerText.transform.localScale = Vector3.one * (1f + pulse * 0.2f);
        }
        else
        {
            timerText.transform.localScale = Vector3.one;
        }
    }

    private void OnPlantPhaseStarted()
    {
        isWarning = false;
        isCritical = false;

        timerText.color = Color.white;
        timerText.transform.localScale = Vector3.one;

        UpdateUI();
    }

    private void OnDefensePhaseStarted()
    {
        UpdateUI();
    }

    private void OnRoundChanged(int newRound)
    {
        UpdateUI();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlantPhaseStart.RemoveListener(OnPlantPhaseStarted);
            GameManager.Instance.OnDefensePhaseStart.RemoveListener(OnDefensePhaseStarted);
            GameManager.Instance.OnRoundChanged.RemoveListener(OnRoundChanged);
        }
    }
    #endregion

    // Public methods to control UI visibility
    /// <summary>
    /// Shows the game UI overlay
    /// </summary>
    public void ShowGameUI()
    {
        if (gameUIPanel != null)
        {
            gameUIPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the game UI overlay (useful for main menu, pause menu, etc.)
    /// </summary>
    public void HideGameUI()
    {
        if (gameUIPanel != null)
        {
            gameUIPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles the game UI visibility
    /// </summary>
    public void ToggleGameUI()
    {
        if (gameUIPanel != null)
        {
            gameUIPanel.SetActive(!gameUIPanel.activeSelf);
        }
    }

    /// <summary>
    /// Checks if the game UI is currently visible
    /// </summary>
    public bool IsGameUIVisible()
    {
        return gameUIPanel != null && gameUIPanel.activeSelf;
    }
}