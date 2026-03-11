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
        upgrades = parent.GetComponentsInChildren<UpgradeUI>();
    }

    public void Init(string id, Rare rare, Sprite icon, Sprite background, int level, int upgradeNumber, PartySetupControllerUI controller)
    {
        base.Setup(id, rare, icon, background);
        txtLevel.text = level.ToString();

        for (int i = 0; i < upgrades.Length; i++)
        {
            if (i < upgradeNumber)
            {
                upgrades[i].ActiveLayer(1);
            }
            else
            {
                upgrades[i].ActiveLayer(0);
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
        _controller.OnCharacterIconClicked(this.ID);
    }
}
