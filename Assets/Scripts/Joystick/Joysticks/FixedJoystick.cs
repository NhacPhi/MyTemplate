using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class FixedJoystick : Joystick
{
    private void Update()
    {
        if (Direction.magnitude > 0)
        {
            // Gửi hướng di chuyển mỗi khi joystick thay đổi
            GameEvent.OnPlayerMove?.Invoke(Direction);
        }
    }

}