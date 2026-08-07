using deVoid.Utils;

public class EnemyKilledSignal : ASignal<string, int> {}
public class PickupItemSignal : ASignal<string, int> {}
public class CharacterUpgradedSignal : ASignal<string, int> {}
public class WeaponUpgradedSignal : ASignal<int> {}
public class WinBattleSignal : ASignal<string, int> {}
public class ShopPurchasedSignal : ASignal<string, int> {}
public class GachaSummonedSignal : ASignal<string, int> {}
public class DailyQuestUpdatedSignal : ASignal {}
