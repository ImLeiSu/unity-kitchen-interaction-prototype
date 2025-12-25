using UnityEngine;

public sealed class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    [Header("Pool")]
    [SerializeField] private ItemDef[] ingredientPool;
    [SerializeField] private int minCount = 1;
    [SerializeField] private int maxCount = 4;

    [Header("UI")]
    [SerializeField] private OrderBoardUI boardUI;

    [Header("Time Pressure")]
    [SerializeField] private float orderTimeLimit = 15f;
    [SerializeField] private int timeoutPenalty = 5;

    public ItemDef CurrentItem { get; private set; }
    public int CurrentNeed { get; private set; }
    public float TimeLeft => timeLeft;
    public float TimeLimit => orderTimeLimit;

    private bool active;
    private float timeLeft;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[OrderManager] Duplicate detected, disabling + destroying.");
            enabled = false;          
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        boardUI?.Clear();
    }

    private void Update()
    {
        if (Instance != this) return;

        if (!active) return;
        if (CurrentItem == null) return;

        timeLeft = Mathf.Max(0f, timeLeft - Time.unscaledDeltaTime);

        boardUI?.SetTimeSeconds(timeLeft, orderTimeLimit);

        if (timeLeft <= 0f)
        {
            OnOrderTimeout();
        }
    }

    public void BeginWorking()
    {
        active = true;

        if (CurrentItem == null || CurrentNeed <= 0)
        {
            GenerateNextOrder();
        }
        else
        {
            boardUI?.Show(CurrentItem, CurrentNeed);
            boardUI?.SetTimeSeconds(timeLeft, orderTimeLimit);
        }
    }

    public void EndWorking()
    {
        active = false;
        ClearOrder();
    }

    public void GenerateNextOrder()
    {
        if (!active) return;

        if (ingredientPool == null || ingredientPool.Length == 0)
        {
            Debug.LogError("[OrderManager] ingredientPool is empty.");
            ClearOrder();
            return;
        }

        int min = Mathf.Max(1, minCount);
        int max = Mathf.Max(min, maxCount);

        CurrentItem = ingredientPool[Random.Range(0, ingredientPool.Length)];
        CurrentNeed = Random.Range(min, max + 1);

        orderTimeLimit = Mathf.Max(1f, orderTimeLimit);
        timeLeft = orderTimeLimit;

        boardUI?.Show(CurrentItem, CurrentNeed);
        boardUI?.SetTimeSeconds(timeLeft, orderTimeLimit);
    }

    public bool TrySubmit(ItemDef item, int amount, out bool completedOrder)
    {
        completedOrder = false;

        if (!active) return false;
        if (CurrentItem == null) return false;
        if (item != CurrentItem) return false;

        amount = Mathf.Max(1, amount);
        CurrentNeed -= amount;

        if (CurrentNeed <= 0)
        {
            completedOrder = true;
            GenerateNextOrder();
            return true;
        }

        boardUI?.Show(CurrentItem, CurrentNeed);
        boardUI?.SetTimeSeconds(timeLeft, orderTimeLimit);
        return true;
    }

    private void OnOrderTimeout()
    {
        Debug.Log("[Order] Timeout");

        if (timeoutPenalty > 0)
            WorkSession.Instance?.AddWaste(timeoutPenalty);

        GenerateNextOrder();
    }

    private void ClearOrder()
    {
        CurrentItem = null;
        CurrentNeed = 0;
        timeLeft = 0f;

        boardUI?.Clear();
    }
}
