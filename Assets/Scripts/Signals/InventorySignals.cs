using deVoid.Utils;

public class CurrencyChangedSignal : ASignal<CurrencyType, int> {}
public class InventoryChangedSignal : ASignal {}
public class ItemChangedSignal : ASignal<string> {}
