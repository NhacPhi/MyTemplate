using System;


[Serializable]
public class PlayerSave
{
    private string playerName;
    private int level;
    private int currentExp;
    private string avatarIcon;

    public string PlayerName { get { return playerName; } set { playerName = value; } }
    public int Level { get { return level; } set { level = value; } }
    public int CurrentExp { get { return currentExp; } set { currentExp = value; } }
    public string AvatarIcon { get { return avatarIcon; } set { avatarIcon = value; } }

    public PlayerSave() { }
}
