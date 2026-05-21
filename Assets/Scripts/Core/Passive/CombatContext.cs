using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatContext
{
    // Người kích hoạt sự kiện / Chủ sở hữu passive
    public Entity Source { get; set; }

    // Mục tiêu chính liên quan đến sự kiện (Kẻ tấn công mình, hoặc kẻ mình đang tấn công)
    public Entity Target { get; set; }

    // Giá trị sát thương, hồi máu, hoặc giá trị liên quan của sự kiện
    public float Value { get; set; }

    // Thông tin bổ sung (Ví dụ: tên kỹ năng vừa dùng, ID buff, loại sát thương...)
    public string EventContextInfo { get; set; }

    // Tập hợp các Tags gắn liền với sự kiện (VD: "Critical", "SkillDamage", "Burn", "Heal")
    public HashSet<string> Tags { get; private set; }

    // Constructor tiện ích để khởi tạo nhanh Context
    public CombatContext(Entity source, Entity target = null, float value = 0f, string info = "")
    {
        Source = source;
        Target = target;
        Value = value;
        EventContextInfo = info;
        Tags = new HashSet<string>();
    }

    // --- Helper Methods cho Tags ---
    
    public CombatContext AddTag(string tag)
    {
        if (!string.IsNullOrEmpty(tag)) 
        {
            Tags.Add(tag);
        }
        return this; // Hỗ trợ viết chuỗi lệnh (fluent/chaining)
    }

    public bool HasTag(string tag)
    {
        return Tags.Contains(tag);
    }

    public CombatContext RemoveTag(string tag)
    {
        Tags.Remove(tag);
        return this;
    }
}
