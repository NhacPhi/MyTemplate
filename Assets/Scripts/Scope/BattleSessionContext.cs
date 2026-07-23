using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSessionContext
{
    public string PendingBattleID { get; set; }
    public GameSceneSO PreviousLocation { get; set; }
    public Vector3? ReturnPosition { get; set; }
    public Vector3? ReturnCameraPosition { get; set; }
    public string PreviousLocationName { get; set; }
}
