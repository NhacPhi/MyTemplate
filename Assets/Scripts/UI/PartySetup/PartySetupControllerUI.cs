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
        foreach(var slot in _saveSystem.Player.ActiveSlots)
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

        var characters = _saveSystem.Player.Characters;

        foreach(var character in characters)
        {
            GameObject characterUI = Instantiate(_chacracterIconUI, _contentCharacterUI);

            characterUI.transform.SetParent(_contentCharacterUI, false);

            var characterUIConfig = characterUI.GetComponent<CharacterIconUI>();

            var characterConfig = _gameData.GetItemConfig(character.ID);

            characterUIConfig.Init(character.ID, characterConfig.Rarity, characterConfig.Icon,
                _gameData.GetBGItemByRare(characterConfig.Rarity), character.Level, character.BoostStat, this);

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
        _saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
    }

    public void SwapActiveSlots(int posA, int posB)
    {
        // 1. Lấy dữ liệu Data và UI
        var dataA = _saveSystem.Player.ActiveSlots.Find(s => s.Position == posA);
        var dataB = _saveSystem.Player.ActiveSlots.Find(s => s.Position == posB);

        _activeUISlots.TryGetValue(posA, out CharacterSlotUI uiA);
        _activeUISlots.TryGetValue(posB, out CharacterSlotUI uiB);

        if (uiA == null && uiB == null) return;

        // --- CẬP NHẬT DATA ---
        string idA = dataA != null ? dataA.CharacterID : "";
        string idB = dataB != null ? dataB.CharacterID : "";

        if (dataA != null) dataA.CharacterID = idB;
        if (dataB != null) dataB.CharacterID = idA;

        // --- CẬP NHẬT UI ---
        Transform slotATrans = _slotPositions[posA - 1].transform;
        Transform slotBTrans = _slotPositions[posB - 1].transform;

        // Nếu có nhân vật ở ô A -> Di chuyển sang Slot B
        if (uiA != null)
        {
            uiA.CurrentPosition = posB;
            // Gán cha mới là Slot B
            uiA.transform.SetParent(slotBTrans, true);
            // Di chuyển mượt mà tới vị trí local (0, Offset, 0) của cha mới
            uiA.transform.DOLocalMove(new Vector3(0, Offset, 0), 0.3f).SetEase(Ease.OutQuad);
        }

        // Nếu có nhân vật ở ô B -> Di chuyển sang Slot A
        if (uiB != null)
        {
            uiB.CurrentPosition = posA;
            // Gán cha mới là Slot A
            uiB.transform.SetParent(slotATrans, true);
            // Di chuyển mượt mà tới vị trí local (0, Offset, 0) của cha mới
            uiB.transform.DOLocalMove(new Vector3(0, Offset, 0), 0.3f).SetEase(Ease.OutQuad);
        }

        // --- CẬP NHẬT DICTIONARY ---
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
        for (int i = 6; i >=1 ; i--)
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
            for (int i = 1; i <= 6; i++)
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
        var activeList = _saveSystem.Player.ActiveSlots;
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

        // 1. Xóa Data trong SaveSystem
        var activeList = _saveSystem.Player.ActiveSlots;
        var dataToRemove = activeList.Find(s => s.Position == position);
        if (dataToRemove != null)
        {
            activeList.Remove(dataToRemove);
        }

        // 2. Xóa khỏi Dictionary quản lý
        if (_activeUISlots.ContainsKey(position))
        {
            _activeUISlots.Remove(position);
        }

        // 3. Cập nhật trạng thái Icon ở danh sách dưới (Bỏ Highlight)
        UpdateIconSelectedState(characterID, false);

        // 4. Hiệu ứng thu nhỏ rồi biến mất
        characterUI.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(characterUI.gameObject);
        });

        ReorderHierarchy();
    }

    private void UpdateIconSelectedState(string characterID, bool isSelected)
    {
        // Tìm trong danh sách Icon UI (Bạn nên lưu danh sách này lúc Start)
        // Giả sử bạn lưu danh sách Icon vào List<CharacterIconUI> _allIcons;
        var iconUI = _characterIconUI.FirstOrDefault(i => i.ID == characterID);
        if (iconUI != null)
        {
            iconUI.ToggleSelected(isSelected);
        }
    }

    public void RemoveAllCharacters()
    {
        // 1. Kiểm tra xem có nhân vật nào không
        if (_activeUISlots.Count == 0) return;

        // 2. Lấy danh sách ID để cập nhật Icon phía dưới sau khi xóa
        // Chúng ta cần Copy danh sách Values ra một List tạm để tránh lỗi khi vừa duyệt vừa xóa Dictionary
        var iconsToDeselect = _activeUISlots.Values.Select(ui => ui.CharacterID).ToList();

        // 3. CẬP NHẬT DATA: Không Clear() mà duyệt qua các slot đang hoạt động để set trống
        foreach (var slot in _saveSystem.Player.ActiveSlots)
        {
            slot.CharacterID = "";
        }

        // 4. Hiệu ứng và Xóa toàn bộ Object UI nhân vật
        foreach (var uiSlot in _activeUISlots.Values)
        {
            uiSlot.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
            {
                Destroy(uiSlot.gameObject);
            });
        }

        // 5. Dọn dẹp Dictionary quản lý
        _activeUISlots.Clear();

        // 6. Cập nhật lại toàn bộ Icon ở danh sách dưới (Bỏ dấu tích/Highlight)
        foreach (var id in iconsToDeselect)
        {
            UpdateIconSelectedState(id, false);
        }

        // 7. Sắp xếp lại Layer và Lưu dữ liệu
        ReorderHierarchy();

        Debug.Log("Đã xóa tất cả nhân vật khỏi đội hình!");
    }
}
