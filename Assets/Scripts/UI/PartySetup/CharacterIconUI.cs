using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class CharacterIconUI : GameItemUI, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject seleted;
    private UpgradeUI[] upgrades;
    public bool IsSelected;
    private PartySetupControllerUI _controller;
    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (upgrades == null || upgrades.Length == 0)
        {
            if (parent != null)
            {
                upgrades = parent.GetComponentsInChildren<UpgradeUI>(true);
            }
            else
            {
                upgrades = GetComponentsInChildren<UpgradeUI>(true);
            }
        }

        if (txtLevel == null)
        {
            txtLevel = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    public void Init(string id, Rare rare, Sprite icon, Sprite background, int level, int upgradeNumber, PartySetupControllerUI controller)
    {
        EnsureInitialized();
        base.Setup(id, rare, icon, background);

        if (txtLevel != null)
        {
            txtLevel.text = level.ToString();
        }

        if (upgrades != null)
        {
            for (int i = 0; i < upgrades.Length; i++)
            {
                if (upgrades[i] == null) continue;

                if (i < upgradeNumber)
                {
                    upgrades[i].ActiveLayer(1);
                }
                else
                {
                    upgrades[i].ActiveLayer(0);
                }
            }
        }

        _controller = controller;
    }

    public void ToggleSelected(bool isOn)
    {
        IsSelected = false;
        if (isOn)
        {
            seleted.gameObject.SetActive(true);
        }
        else
        {
            seleted.gameObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _controller?.OnCharacterIconClicked(this.ID);
    }
}
