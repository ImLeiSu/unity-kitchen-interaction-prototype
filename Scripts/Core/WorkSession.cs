using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class WorkSession : MonoBehaviour
{
    public static WorkSession Instance { get; private set; }

    // ========= State =========
    [field: SerializeField]
    public WorkState State { get; private set; } = WorkState.Idle;

    public event Action<WorkState, WorkState> OnStateChanged;

    // ========= Modal UI Gate =========
    public bool HasModalUIOpen { get; private set; }
    public event Action<bool> OnModalUIChanged;

    // ========= Pay (Demo) =========
    [Header("Pay (Demo)")]
    [SerializeField] private int basePay = 100;
    [SerializeField] private int payPerOrder = 5;
    [SerializeField] private int penaltyPerWaste = 1;

    [Header("UI (optional)")]
    [SerializeField] private SettlementUI settlementUI;

    public int BasePay => basePay;
    public int PayPerOrder => payPerOrder;
    public int PenaltyPerWaste => penaltyPerWaste;

    public int CompletedOrders { get; private set; }
    public int WastedIngredients { get; private set; }
    public int TotalPay => basePay + CompletedOrders * payPerOrder - WastedIngredients * penaltyPerWaste;

    public event Action OnPayChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ========= State Transitions =========
    public void StartWork()
    {
        ResetPay();

        SetState(WorkState.Working);

        if (OrderManager.Instance != null)
            OrderManager.Instance.BeginWorking();
    }

    public void StopWork()
    {
        SetState(WorkState.Idle);

        if (OrderManager.Instance != null)
            OrderManager.Instance.EndWorking();

        // Demo: show settlement summary at end of work
        if (settlementUI != null)
            settlementUI.Open(this);
    }

    public void EnterChopping() => SetState(WorkState.Chopping);
    public void ExitChoppingToWorking() => SetState(WorkState.Working);

    private void SetState(WorkState next)
    {
        if (State == next) return;

        var prev = State;
        State = next;

        OnStateChanged?.Invoke(prev, next);
    }

    // ========= Modal UI Control =========
    public void EnterModalUI() => SetModalUI(true);
    public void ExitModalUI() => SetModalUI(false);

    private void SetModalUI(bool open)
    {
        if (HasModalUIOpen == open) return;

        HasModalUIOpen = open;
        OnModalUIChanged?.Invoke(open);
    }

    // ========= Pay API =========
    public void ResetPay()
    {
        CompletedOrders = 0;
        WastedIngredients = 0;
        OnPayChanged?.Invoke();
    }

    public void AddCompletedOrder(int count = 1)
    {
        CompletedOrders += Mathf.Max(0, count);
        OnPayChanged?.Invoke();
    }

    public void AddWaste(int count = 1)
    {
        WastedIngredients += Mathf.Max(0, count);
        OnPayChanged?.Invoke();
    }
}
