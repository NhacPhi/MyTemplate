using deVoid.Utils;
using System.Collections.Generic;

public class StartDialogueSignal : ASignal<DialogueConfig> {}
public class OpenDialogueSignal : ASignal<string, ActorConfig> {}
public class EndDialogueSignal : ASignal<DialogueType> {}
public class ShowChoiceUISignal : ASignal<List<ChoiceComponent>> {}
public class MakeChoiceUISignal : ASignal<ChoiceComponent> {}
