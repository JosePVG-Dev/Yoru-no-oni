using System;
using UnityEngine;
using UnityEngine.UI;

public enum RewardType { RestoreHealth, BonusDamage, AttackSpeed }

public class RewardPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button healthButton;
    [SerializeField] private Button damageButton;
    [SerializeField] private Button speedButton;

    private Action<RewardType> callback;

    private void Awake()
    {
        healthButton.onClick.AddListener(() => Select(RewardType.RestoreHealth));
        damageButton.onClick.AddListener(() => Select(RewardType.BonusDamage));
        speedButton.onClick.AddListener(() => Select(RewardType.AttackSpeed));

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Show(Action<RewardType> onSelected)
    {
        callback = onSelected;
        panelRoot.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Select(RewardType type)
    {
        panelRoot.SetActive(false);
        Time.timeScale = 1f;
        callback?.Invoke(type);
    }
}
