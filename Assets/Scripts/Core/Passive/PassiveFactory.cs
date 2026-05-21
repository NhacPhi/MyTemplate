using UnityEngine;

public static class PassiveFactory
{
    /// <summary>
    /// Factory để khởi tạo PassiveInstance.
    /// Trong tương lai, nếu có nhiều loại Passive phức tạp khác nhau, có thể áp dụng Builder Pattern tại đây.
    /// </summary>
    public static PassiveInstance CreatePassive(PassiveConfig config, int level, CharacterProfileModel owner)
    {
        return new PassiveInstance(config, level, owner);
    }
}
