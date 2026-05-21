using UnityEngine;

public interface IEffectHandler
{
    /// <summary>
    /// Thực thi hiệu ứng lên mục tiêu
    /// </summary>
    /// <param name="target">Thực thể chịu tác động</param>
    /// <param name="effectValue">Giá trị (từ JSON config sau khi đã tính toán theo cấp độ)</param>
    /// <param name="context">Ngữ cảnh trận đấu (chứa Source, thông tin thêm...)</param>
    void Execute(Entity target, float effectValue, CombatContext context);
}
