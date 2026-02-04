using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageInterceptorBuff : IBuff
{
    // handle recive damage
    void OnIncomingDamage(ref float damage, Entity source);

    //Handle inflict damage
    void OnOutgoingDamage(ref float damage, Entity target);
}
