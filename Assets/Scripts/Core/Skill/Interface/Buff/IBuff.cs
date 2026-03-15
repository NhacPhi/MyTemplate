using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuff 
{
    //string BuffID { get; }      
    //float Duration { get; }    
    //bool IsFinished { get; }

    void OnApply(Entity target);
    //void OnRemove(Entity Target);
    //void OnRefresh();
}
