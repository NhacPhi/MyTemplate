using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework;
using VContainer;

public class SevenDayLoginCharacterTargetUI : MonoBehaviour
{
    [Header("Trạng thái 1: Chưa chọn nhân vật (Empty)")]
    [SerializeField] private GameObject emptyStateRoot;          // Root GameObject trạng thái 1 (VD: "empty")
    [SerializeField] private Button btnSelectCharacter;           // Button mở popup chọn nhân vật (VD: "btnSelectCharacter")

    [Header("Trạng thái 2: Đã chọn nhân vật (Selected)")]
    [SerializeField] private GameObject selectedStateRoot;       // Root GameObject trạng thái 2 (VD: "view")
    [SerializeField] private Image imgCharacterPortrait;         // Image hiển thị hình ảnh của nhân vật đã chọn
    [SerializeField] private Button btnChangeSelection;          // Nút bấm cho phép đổi lại lựa chọn (nếu chưa nhận thưởng)

    [Header("Claim States")]
    [SerializeField] private GameObject claimedOverlay;          // Trạng thái đã nhận (imgOverlay)
    [SerializeField] private UIClaimHighlightEffect claimHighlightEffect; // Hiệu ứng lấp lánh trên root
    [SerializeField] private Button btnClaim;                    // Button để nhận nhân vật

    [Inject] private UIManager _uiManager;
    [Inject] private GameDataBase _gameDataBase;

    private int _dayNumber = 7;
    private string _selectedCharacterId = "";
    private List<string> _selectableCharacterIds = new List<string>();
    private SevenDayRewardState _state = SevenDayRewardState.Locked;
    private Action<string> _onCharacterSelectedCallback;
    private Action<int, string> _onClaimClicked;

    private void Awake()
    {
        AutoWire();

        if (btnSelectCharacter != null)
        {
            btnSelectCharacter.onClick.AddListener(OnBtnSelectCharacterClicked);
        }

        if (btnChangeSelection != null && btnChangeSelection != btnClaim && btnChangeSelection != btnSelectCharacter)
        {
            btnChangeSelection.onClick.AddListener(OnBtnSelectCharacterClicked);
        }

        if (btnClaim != null)
        {
            btnClaim.onClick.AddListener(OnClickClaim);
        }
    }

    public void AutoWire()
    {
        if (emptyStateRoot == null)
        {
            var emptyT = transform.Find("Frame/empty") ?? transform.Find("empty");
            if (emptyT != null) emptyStateRoot = emptyT.gameObject;
        }

        if (selectedStateRoot == null)
        {
            var viewT = transform.Find("Frame/view") ?? transform.Find("view");
            if (viewT != null) selectedStateRoot = viewT.gameObject;
        }

        if (btnSelectCharacter == null && emptyStateRoot != null)
        {
            btnSelectCharacter = emptyStateRoot.GetComponentInChildren<Button>(true);
        }

        if (imgCharacterPortrait == null && selectedStateRoot != null)
        {
            var allImgs = selectedStateRoot.GetComponentsInChildren<Image>(true);
            foreach (var img in allImgs)
            {
                if (img.name.Contains("content") || img.name.Contains("Icon") || img.name.Contains("Portrait") || img.name.Contains("image"))
                {
                    imgCharacterPortrait = img;
                    break;
                }
            }
        }


        if (claimedOverlay == null)
        {
            var overlayT = transform.Find("imgOverlay");
            if (overlayT != null) claimedOverlay = overlayT.gameObject;
        }

        if (claimHighlightEffect == null)
        {
            claimHighlightEffect = GetComponent<UIClaimHighlightEffect>() ?? gameObject.AddComponent<UIClaimHighlightEffect>();
        }

        if (btnClaim == null)
        {
            var claimBtnT = transform.Find("btnClaimReward");
            if (claimBtnT != null) btnClaim = claimBtnT.GetComponent<Button>();
            else btnClaim = GetComponentInChildren<Button>(true);
        }

        if (btnChangeSelection == btnClaim)
        {
            btnChangeSelection = null;
        }
    }

    private void OnDestroy()
    {
        if (btnSelectCharacter != null)
        {
            btnSelectCharacter.onClick.RemoveListener(OnBtnSelectCharacterClicked);
        }

        if (btnChangeSelection != null && btnChangeSelection != btnClaim && btnChangeSelection != btnSelectCharacter)
        {
            btnChangeSelection.onClick.RemoveListener(OnBtnSelectCharacterClicked);
        }

        if (btnClaim != null)
        {
            btnClaim.onClick.RemoveListener(OnClickClaim);
        }
    }

    public void InitDependencies(UIManager uiManager, GameDataBase db)
    {
        if (_uiManager == null) _uiManager = uiManager;
        if (_gameDataBase == null) _gameDataBase = db;
    }

    /// <summary>
    /// Setup dữ liệu cho thẻ nhân vật Ngày 7
    /// </summary>
    public void Setup(
        int dayNumber, 
        string selectedCharacterId, 
        List<string> selectableCharacterIds, 
        SevenDayRewardState state, 
        Action<string> onCharacterSelected, 
        Action<int, string> onClaimClicked, 
        UIManager uiManager = null, 
        GameDataBase db = null)
    {
        _dayNumber = dayNumber;
        _selectedCharacterId = selectedCharacterId;
        _selectableCharacterIds = selectableCharacterIds ?? new List<string>();
        _state = state;
        _onCharacterSelectedCallback = onCharacterSelected;
        _onClaimClicked = onClaimClicked;

        if (uiManager != null) _uiManager = uiManager;
        if (db != null) _gameDataBase = db;

        UpdateSelectionVisuals();
        SetState(_state);
    }

    /// <summary>
    /// Chuyển đổi hiển thị giữa Trạng thái 1 (Chưa chọn) và Trạng thái 2 (Đã chọn)
    /// </summary>
    private void UpdateSelectionVisuals()
    {
        bool hasSelected = !string.IsNullOrEmpty(_selectedCharacterId);

        // Trạng thái 1: Chưa chọn nhân vật
        if (emptyStateRoot != null)
        {
            emptyStateRoot.SetActive(!hasSelected);
        }

        // Trạng thái 2: Đã chọn nhân vật -> Ẩn trạng thái 1, show image character
        if (selectedStateRoot != null)
        {
            selectedStateRoot.SetActive(hasSelected);
        }

        if (hasSelected)
        {
            LoadCharacterData(_selectedCharacterId);
        }
        else
        {
           
        }
    }

    private void LoadCharacterData(string characterId)
    {
        var charConfig = _gameDataBase.GetCharacterConfig(characterId);
        if (charConfig != null)
        {
            // Hình ảnh nhân vật (ưu tiên BigIcon, nếu không có lấy Image hoặc Icon)
            if (imgCharacterPortrait != null)
            {
                Sprite portraitSprite = charConfig.BigIcon != null ? charConfig.BigIcon 
                    : (charConfig.Image != null ? charConfig.Image : charConfig.Icon);
                
                imgCharacterPortrait.sprite = portraitSprite;
                imgCharacterPortrait.gameObject.SetActive(portraitSprite != null);
            }

        }
        else
        {

        }
    }

    /// <summary>
    /// Mở popup chọn nhân vật tương tự trong Gacha Scene (PopupGachaSelectTarget)
    /// </summary>
    public void OpenCharacterSelectPopup()
    {
        if (_uiManager == null)
        {
            Debug.LogError("[SevenDayLoginCharacterTargetUI] UIManager is not injected or resolved!");
            return;
        }

        // Tạo danh sách ID có thể chọn (nếu danh sách trống, có thể lấy mặc định các SSR/UR từ GameDataBase)
        List<string> candidateIds = new List<string>(_selectableCharacterIds);
        if (candidateIds.Count == 0 && _gameDataBase != null)
        {
            candidateIds.AddRange(new[] { "hero_erlangshen", "hero_wukong", "hero_nezha", "hero_yangjian" });
        }

        var props = new GachaSelectTargetProperties(
            "Chọn Nhân Vật Ngày 7",
            "SevenDayLogin_Day07",
            candidateIds,
            OnSelectedFromPopup
        );

        _uiManager.OpenWindowScene(ScreenIds.PopupGachaSelectTarget, props);
    }

    private void OnBtnSelectCharacterClicked()
    {
        // Nếu đã nhận rồi thì không cho đổi
        if (_state == SevenDayRewardState.Claimed) return;

        OpenCharacterSelectPopup();
    }

    private void OnSelectedFromPopup(string chosenCharacterId)
    {
        if (string.IsNullOrEmpty(chosenCharacterId)) return;

        _selectedCharacterId = chosenCharacterId;
        UpdateSelectionVisuals();
        _onCharacterSelectedCallback?.Invoke(_selectedCharacterId);
    }

    /// <summary>
    /// Cập nhật trạng thái nhận thưởng
    /// </summary>
    public void SetState(SevenDayRewardState state)
    {
        _state = state;

        switch (state)
        {
            case SevenDayRewardState.CanClaim:
                if (claimedOverlay != null) claimedOverlay.SetActive(false);

                if (claimHighlightEffect != null) claimHighlightEffect.Play();


                if (btnClaim != null)
                {
                    btnClaim.interactable = true;
                    btnClaim.gameObject.SetActive(true);
                }
              
                break;

            case SevenDayRewardState.Claimed:
                if (claimedOverlay != null) claimedOverlay.SetActive(true);

                if (claimHighlightEffect != null) claimHighlightEffect.Stop();


                if (btnClaim != null)
                {
                    btnClaim.interactable = false;
                }

                break;

            case SevenDayRewardState.Locked:
            default:
                if (claimedOverlay != null) claimedOverlay.SetActive(false);

                if (claimHighlightEffect != null) claimHighlightEffect.Stop();


                if (btnClaim != null)
                {
                    btnClaim.interactable = false;
                }

                break;
        }
    }

    private void OnClickClaim()
    {
        // Nếu chưa chọn nhân vật, mở popup chọn trước
        if (string.IsNullOrEmpty(_selectedCharacterId))
        {
            OpenCharacterSelectPopup();
            return;
        }

        if (_state == SevenDayRewardState.CanClaim)
        {
            _onClaimClicked?.Invoke(_dayNumber, _selectedCharacterId);
        }
    }

    public string SelectedCharacterId => _selectedCharacterId;
    public SevenDayRewardState State => _state;
}
