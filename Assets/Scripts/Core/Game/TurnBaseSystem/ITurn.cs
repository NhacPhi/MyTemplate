using System;

public interface ITurn 
{
    public bool IsEndTurn { get; }
    public void HandleTurn(Entity target);
}
