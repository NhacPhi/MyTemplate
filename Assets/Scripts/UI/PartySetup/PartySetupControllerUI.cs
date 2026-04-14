using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using static UnityEditor.Progress;

public class PartySetupControllerUI : MonoBehaviour
{
    [Inject] SaveSystem _saveSystem;
    [Inject] GameDataBase _gameData;
    

    [SerializeField] private List<GameObject> _slotPositions;

    [SerializeField] private GameObject _characterPrefabs;

    [SerializeField] private GameObject _chacracterIconUI;

    [SerializeField] private Transform _contentCharacterUI;

    private float Offset = 100f;

    private Dictionary<int, CharacterSlotUI> _activeUISlots = new Dictionary<int, CharacterSlotUI>();

    private List<CharacterIconUI> _characterIconUI = new List<CharacterIconUI>();

    private void OnEnable()
    {
        UIEvent.OnPrepareBattleData += InitData;
    }

    private void OnDisable()
    {
        UIEvent.OnPrepareBattleData -= InitData;
    }
    public void InitData()
    {
        foreach(var slot in _saveSystem.Player.Roster.ActiveSlots)
        {
            if(slot.CharacterID != "")
            {
                GameObject character = Instantiate(_characterPrefabs, transform);

                character.transform.SetParent(transform, false);

                character.transform.localScale = Vector3.one;

                SetCharacterPosition(character.transform, slot.Position);

                var characterConfig = _gameData.GetCharacterConfig(slot.CharacterID);

                var characterSlotUI = character.GetComponent<CharacterSlotUI>();

                characterSlotUI.SetupCharacterSlotUI(characterConfig.Image, LocalizationManager.Instance.GetLocalizedValue(characterConfig.Name));

                characterSlotUI.Init(slot.Position, this, slot.CharacterID);

                _activeUISlots[slot.Position] = characterSlotUI;
            }
        }

        var characters = _saveSystem.Player.Roster.Characters;

        foreach(var character in characters)
        {
            GameObject characterUI = Instantiate(_chacracterIconUI, _contentCharacterUI);

            characterUI.transform.SetParent(_contentCharacterUI, false);

            var characterUIConfig = characterUI.GetComponent<CharacterIconUI>();

            var characterConfig = _gameData.GetItemConfig(character.ID);

            characterUIConfig.Init(character.ID, characterConfig.Rarity, characterConfig.Icon,
                _gameData.GetBGItemByRare(characterConfig.Rarity), character.Level, character.StarUp, this);

            if(_activeUISlots.Values.FirstOrDefault(ui => ui.CharacterID == character.ID))
            {
                characterUIConfig.ToggleSelected(true);
            }

            _characterIconUI.Add(characterUIConfig);
        }

        ReorderHierarchy();
    }

    public void CleanData()
    {
        foreach(var characterUI in _characterIconUI)
        {
            Destroy(characterUI.gameObject);
        }

        foreach (var characterslot in _activeUISlots)
        {
            Destroy(characterslot.Value.gameObject);
        }

        _characterIconUI.Clear();

        _activeUISlots.Clear();
    }

    private void SetCharacterPosition(Transform trans, int position)
    {
        Transform targetSlot = _slotPositions[position - 1].transform;

        // Cho nhân vật làm con của Slot
        trans.SetParent(targetSlot, false);

        // Reset vị trí về 0 (tâm của Slot) và cộng thêm Offset y
        trans.localPosition = new Vector3(0, Offset, 0);
    }

    public void SavePartySetup()
    {
        CleanData();
        ReorganizeActiveSlotsData();
        _saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
    }

    public void SwapActiveSlots(int posA, int posB)
    {
        // 1. Lấy dữ liệu Data hiện tại (có thể null nếu ô đó chưa bao giờ được tạo)
        var dataA = _saveSystem.Player.Roster.ActiveSlots.Find(s => s.Position == posA);
        var dataB = _saveSystem.Player.Roster.ActiveSlots.Find(s => s.Position == posB);

        // 2. Lấy UI hiện tại
        _activeUISlots.TryGetValue(posA, out CharacterSlotUI uiA);
        _activeUISlots.TryGetValue(posB, out CharacterSlotUI uiB);

        if (uiA == null && uiB == null) return;

        // --- CẬP NHẬT DATA (GIỮ SLOT, CHỈ ĐỔI UUID) ---

        // Lấy UUID hiện tại (nếu data null thì coi như UUID rỗng)
        string idA = dataA != null ? dataA.CharacterID : "";
        string idB = dataB != null ? dataB.CharacterID : "";

        // Cập nhật UUID cho ô A (nhận UUID của B)
        if (dataA != null)
        {
            dataA.CharacterID = idB;
        }
        else
        {
            // Nếu ô A chưa có trong Data, tạo mới nó với UUID của B
            _saveSystem.Player.Roster.ActiveSlots.Add(new ActiveSlotData{ Position = posA, CharacterID = idB });
        }

        // Cập nhật UUID cho ô B (nhận UUID của A)
        if (dataB != null)
        {
            dataB.CharacterID = idA;
        }
        else
        {
            // Nếu ô B chưa có trong Data, tạo mới nó với UUID của A
            _saveSystem.Player.Roster.ActiveSlots.Add(new ActiveSlotData { Position = posB, CharacterID = idA });
        }

        // --- CẬP NHẬT UI (Giữ nguyên logic di chuyển mượt mà của ông) ---
        Transform slotATrans = _slotPositions[posA - 1].transform;
        Transform slotBTrans = _slotPositions[posB - 1].transform;

        if (uiA != null)
        {
            uiA.CurrentPosition = posB;
            uiA.transform.SetParent(slotBTrans, true);
            uiA.transform.DOLocalMove(new Vector3(0, Offset, 0), 0.3f).SetEase(Ease.OutQuad);
        }

        if (uiB != null)
        {
            uiB.CurrentPosition = posA;
            uiB.transform.SetParent(slotATrans, true);
            uiB.transform.DOLocalMove(new Vector3(0, Offset, 0), 0.3f).SetEase(Ease.OutQuad);
        }

        // --- CẬP NHẬT DICTIONARY UI ---
        _activeUISlots[posA] = uiB;
        _activeUISlots[posB] = uiA;

        if (_activeUISlots[posA] == null) _activeUISlots.Remove(posA);
        if (_activeUISlots[posB] == null) _activeUISlots.Remove(posB);

        ReorderHierarchy();
    }

    public void SetAllCharactersRaycast(bool canBlock)
    {
        foreach (var uiSlot in _activeUISlots.Values)
        {
            // Sử dụng CanvasGroup để bật/tắt nhanh
            var cg = uiSlot.GetComponent<CanvasGroup>();
            if (cg != null) cg.blocksRaycasts = canBlock;
        }
    }

    private void ReorderHierarchy()
    {
        // Duyệt qua từ vị trí 1 đến 6 (hoặc số lượng slot bạn có)
        for (int i = Definition.MAX_SLOT_CHARACTER; i >=1 ; i--)
        {
            // Lấy transform của Slot tương ứng (Index trong List là i-1)
            Transform slotTransform = _slotPositions[i - 1].transform;

            // Đưa cả cái Slot này xuống cuối Hierarchy để nó đè lên các Slot khác
            slotTransform.SetAsLastSibling();
        }
    }

    public void OnCharacterIconClicked(string characterID)
    {
        // Tìm xem nhân vật này đã có trong UI chưa
        var existingUI = _activeUISlots.Values.FirstOrDefault(ui => ui.CharacterID == characterID);

        if (existingUI != null)
        {
            // TRƯỜNG HỢP: ĐÃ CÓ -> THU HỒI
            RemoveCharacterFromSlot(existingUI);
            Debug.Log("Đã thu hồi nhân vật: " + characterID);
        }
        else
        {
            // TRƯỜNG HỢP: CHƯA CÓ -> TÌM CHỖ TRỐNG ĐỂ THÊM
            int emptyPos = -1;
            for (int i = 1; i <= Definition.MAX_SLOT_CHARACTER; i++)
            {
                if (!_activeUISlots.ContainsKey(i) || _activeUISlots[i] == null)
                {
                    emptyPos = i;
                    break;
                }
            }

            if (emptyPos != -1)
            {
                AddCharacterToSlot(characterID, emptyPos);
            }
            else
            {
                Debug.Log("Đội hình đã đầy!");
            }
        }
    }

    private void AddCharacterToSlot(string characterID, int position)
    {
        // --- CẬP NHẬT DATA ---
        var activeList = _saveSystem.Player.Roster.ActiveSlots;
        var slotData = activeList.Find(s => s.Position == position);

        if (slotData != null)
        {
            slotData.CharacterID = characterID;
        }
        else
        {
            ActiveSlotData newSlot = new ActiveSlotData();
            newSlot.Position = position;
            newSlot.CharacterID = characterID;

            activeList.Add(newSlot);
        }

        // --- CẬP NHẬT UI ---
        GameObject character = Instantiate(_characterPrefabs, transform);
        var characterSlotUI = character.GetComponent<CharacterSlotUI>();
        var characterConfig = _gameData.GetCharacterConfig(characterID);

        // Setup thông tin hình ảnh
        characterSlotUI.SetupCharacterSlotUI(characterConfig.Image, LocalizationManager.Instance.GetLocalizedValue(characterConfig.Name));
        characterSlotUI.Init(position, this, characterID);

        // Đưa vào vị trí Slot (Dùng hàm SetParent bạn vừa đổi)
        SetCharacterPosition(character.transform, position);

        // Hiệu ứng xuất hiện cho "xịn" (Scale từ 0 lên 1)
        character.transform.localScale = Vector3.zero;
        character.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        // Lưu vào Dictionary quản lý
        _activeUISlots[position] = characterSlotUI;

        // Cập nhật trạng thái "Selected" trên Icon ở danh sách dưới
        UpdateIconSelectedState(characterID, true);

        ReorderHierarchy();
    }

    private void RemoveCharacterFromSlot(CharacterSlotUI characterUI)
    {
        int position = characterUI.CurrentPosition;
        string characterID = characterUI.CharacterID;

        var activeList = _saveSystem.Player.Roster.ActiveSlots;
        var dataToRemove = activeList.Find(s => s.Position == position);
        if (dataToRemove != null)
        {
            activeList.Remove(dataToRemove);
        }

        if (_activeUISlots.ContainsKey(position))
        {
            _activeUISlots.Remove(position);
        }


        UpdateIconSelectedState(characterID, false);


        characterUI.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(characterUI.gameObject);
        });

        ReorderHierarchy();
    }

    private void UpdateIconSelectedState(string characterID, bool isSelected)
    {
        var iconUI = _characterIconUI.FirstOrDefault(i => i.ID == characterID);
        if (iconUI != null)
        {
            iconUI.ToggleSelected(isSelected);
        }
    }

    public void RemoveAllCharacters()
    {

        if (_activeUISlots.Count == 0) return;


        var iconsToDeselect = _activeUISlots.Values.Select(ui => ui.CharacterID).ToList();


        foreach (var slot in _saveSystem.Player.Roster.ActiveSlots)
        {
            slot.CharacterID = "";
        }


        foreach (var uiSlot in _activeUISlots.Values)
        {
            uiSlot.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
            {
                Destroy(uiSlot.gameObject);
            });
        }

        _activeUISlots.Clear();


        foreach (var id in iconsToDeselect)
        {
            UpdateIconSelectedState(id, false);
        }

        ReorderHierarchy();

        Debug.Log("Đã xóa tất cả nhân vật khỏi đội hình!");
    }

    public void ReorganizeActiveSlotsData()
    {
        _saveSystem.Player.Roster.ActiveSlots.Sort((slotA, slotB) => slotA.Position.CompareTo(slotB.Position));
    }
}
