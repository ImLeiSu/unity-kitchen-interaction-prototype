using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public sealed class ChoppingController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text progressText;

    [Header("Cameras (toggle)")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera chopCamera;

    [Header("Player Control")]
    [SerializeField] private MonoBehaviour thirdPersonController;

    [Header("Food (uncut)")]
    [SerializeField] private Transform foodSpawn;
    [SerializeField] private GameObject defaultFoodPrefab;

    [Header("Output (sliced)")]
    [SerializeField] private Transform slicedSpawn;
    [SerializeField] private GameObject defaultSlicedPrefab;
    [SerializeField] private float slicedStaySeconds = 0.6f;
    [SerializeField] private float nextDelaySeconds = 0.35f;

    private bool isOpen;
    private GameObject currentFood;

    private int targetCuts;
    private int currentCuts;
    private bool transitioning;

    private bool isWaitingForItem;
    private ItemDef currentItem;

    private bool hasConsumedThisRound;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (root != null) root.SetActive(false);

        if (mainCamera == null) mainCamera = Camera.main;
        SetChopCameraActive(false);
    }

    private void Update()
    {
        if (!isOpen) return;

        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            Close();
            return;
        }

        if (isWaitingForItem)
        {
            if (PrepQueue.Instance != null && PrepQueue.Instance.Count > 0)
            {
                isWaitingForItem = false;
                NextFood();
            }
            return;
        }

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            TryCutOnce();
        }
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        WorkSession.Instance?.EnterChopping();

        if (root != null) root.SetActive(true);

        if (thirdPersonController != null)
            thirdPersonController.enabled = false;

        SetCursor(true);

        if (mainCamera == null) mainCamera = Camera.main;
        SetChopCameraActive(true);

        NextFood();
    }

    public void Close()
    {
        if (!isOpen) return;

        if (!transitioning && hasConsumedThisRound && currentItem != null && currentCuts < targetCuts)
        {
            WorkSession.Instance?.AddWaste(1);
            Debug.Log("[Chop] Exit mid-process => Waste +1");
        }

        isOpen = false;

        WorkSession.Instance?.ExitChoppingToWorking();

        if (root != null) root.SetActive(false);

        SetChopCameraActive(false);

        if (thirdPersonController != null)
            thirdPersonController.enabled = true;

        SetCursor(false);

        if (currentFood != null) Destroy(currentFood);
        currentFood = null;

        isWaitingForItem = false;
        currentItem = null;
        transitioning = false;
        currentCuts = 0;
        targetCuts = 0;
        hasConsumedThisRound = false;
    }

    private void SetChopCameraActive(bool chopping)
    {
        if (chopCamera != null) chopCamera.enabled = chopping;
        if (mainCamera != null) mainCamera.enabled = !chopping;
    }

    private GameObject GetPrefabFor(ItemDef item)
    {
        if (item != null)
        {
            if (item.choppingPrefab != null) return item.choppingPrefab;
            if (item.surfacePrefab != null) return item.surfacePrefab;
        }
        return defaultFoodPrefab;
    }

    private GameObject GetSlicedPrefabFor(ItemDef item)
    {
        if (item != null && item.slicedPrefab != null)
            return item.slicedPrefab;

        return defaultSlicedPrefab;
    }

    private static int GetRequiredCuts(ItemDef item)
    {
        if (item == null) return 1;
        return Mathf.Max(1, item.requiredCuts);
    }

    private void NextFood()
    {
        if (currentFood != null) Destroy(currentFood);
        currentFood = null;

        transitioning = false;
        currentCuts = 0;

        hasConsumedThisRound = false;
        currentItem = null;

        ItemDef item = null;
        bool hasItem = false;

        if (PrepQueue.Instance != null)
            hasItem = PrepQueue.Instance.TryDequeue(out item);

        Debug.Log($"[Chop] Dequeue hasItem={hasItem}, item={(item != null ? item.displayName : "NULL")}");

        if (!hasItem || item == null)
        {
            isWaitingForItem = true;

            if (titleText != null) titleText.text = "Chop: (no items)";
            if (progressText != null) progressText.text = "Waiting for ingredients... (Esc to exit)";
            return;
        }

        isWaitingForItem = false;
        currentItem = item;

        targetCuts = GetRequiredCuts(currentItem);

        bool consumed = PrepSurfaceStack.Instance != null && PrepSurfaceStack.Instance.ConsumeOne(item);
        Debug.Log($"[Chop] ConsumeOne({item.displayName}) => {consumed}");

        hasConsumedThisRound = true;

        var prefab = GetPrefabFor(item);
        if (prefab != null && foodSpawn != null)
        {
            currentFood = Instantiate(prefab, foodSpawn.position, foodSpawn.rotation);
        }
        else
        {
            Debug.LogError("[Chop] prefab or foodSpawn is null.");
        }

        if (titleText != null)
            titleText.text = $"Chop: {item.displayName}";

        RefreshProgress();
    }

    private void TryCutOnce()
    {
        if (isWaitingForItem || transitioning) return;
        if (currentFood == null) return;
        if (currentItem == null) return;

        currentCuts++;
        RefreshProgress();

        if (currentCuts < targetCuts) return;

        transitioning = true;

        Destroy(currentFood);
        currentFood = null;

        SpawnSlicedVisual(currentItem);

        bool completedOrder = false;
        bool ok = false;

        if (OrderManager.Instance != null)
        {
            ok = OrderManager.Instance.TrySubmit(currentItem, 1, out completedOrder);
        }

        if (ok && completedOrder)
        {
            WorkSession.Instance?.AddCompletedOrder(1);
            Debug.Log("[Chop] Order completed => +1 completed order");
        }
        else if (!ok)
        {
            WorkSession.Instance?.AddWaste(1);
            Debug.Log("[Chop] Not matching current order => Waste +1");
        }

        if (progressText != null)
            progressText.text = ok ? "Plated!" : "Not matching current order";

        StartCoroutine(NextFoodAfterDelay(nextDelaySeconds));
    }

    private void SpawnSlicedVisual(ItemDef item)
    {
        if (slicedSpawn == null)
        {
            Debug.LogWarning("[Chop] slicedSpawn is null (no plate/output point).");
            return;
        }

        var slicedPrefab = GetSlicedPrefabFor(item);
        if (slicedPrefab == null)
        {
            Debug.LogWarning("[Chop] slicedPrefab is null (set item.slicedPrefab or defaultSlicedPrefab).");
            return;
        }

        var go = Instantiate(slicedPrefab, slicedSpawn.position, slicedSpawn.rotation);

        if (slicedStaySeconds > 0f)
            Destroy(go, slicedStaySeconds);
    }

    private IEnumerator NextFoodAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        NextFood();
    }

    private void RefreshProgress()
    {
        if (progressText != null)
            progressText.text = $"Cuts: {currentCuts}/{targetCuts}";
    }

    private static void SetCursor(bool unlocked)
    {
        Cursor.visible = unlocked;
        Cursor.lockState = unlocked ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
