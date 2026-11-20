using UnityEngine;

public class DemoEndController : MonoBehaviour
{
    [Header("Painel de encerramento")]
    [SerializeField] private GameObject demoEndPanel;

    public static DemoEndController Instance { get; private set; }

    private bool demoFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (demoEndPanel != null)
            demoEndPanel.SetActive(false);
    }

    public void EndDemo()
    {
        if (demoFinished)
            return;

        demoFinished = true;

        if (demoEndPanel != null)
            demoEndPanel.SetActive(true);

        Debug.Log("DEMO FINALIZADA!");
    }
}
