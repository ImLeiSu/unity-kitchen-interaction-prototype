using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class StoryIntroUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Images")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite[] storyImages;

    [Header("Player Control (optional)")]
    [SerializeField] private MonoBehaviour thirdPersonController;

    [Header("Auto Open")]
    [SerializeField] private bool openOnStart = true;

    private int index;
    private bool isOpen;

    private void Awake()
    {
        if (root == null) root = gameObject;
        root.SetActive(false);
    }

    private void Start()
    {
        if (openOnStart)
            Open();
    }

    private void Update()
    {
        if (!isOpen) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnClick();
        }
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        index = 0;

        WorkSession.Instance?.EnterModalUI();

        if (thirdPersonController != null)
            thirdPersonController.enabled = false;

        SetCursor(true);

        root.SetActive(true);
        Refresh();
    }

    private void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        root.SetActive(false);

        if (thirdPersonController != null)
            thirdPersonController.enabled = true;

        WorkSession.Instance?.ExitModalUI();
        SetCursor(false);
    }

    private void OnClick()
    {
        if (storyImages == null || storyImages.Length == 0)
        {
            Close();
            return;
        }

        if (index < storyImages.Length - 1)
        {
            index++;
            Refresh();
        }
        else
        {
            Close();
        }
    }

    private void Refresh()
    {
        if (backgroundImage != null && index < storyImages.Length)
            backgroundImage.sprite = storyImages[index];
    }

    private static void SetCursor(bool unlocked)
    {
        Cursor.visible = unlocked;
        Cursor.lockState = unlocked ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
