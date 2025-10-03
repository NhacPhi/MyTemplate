using Newtonsoft.Json;

namespace Core.EventSystem
{
    public enum EvenType
    {
        Start,
        None
    }
    public abstract class EventBase
    {
        [JsonIgnore] public abstract EvenType Type { get; }

        [JsonProperty("ID")]
        public string ID { get; protected set; }

        [JsonProperty("Name")]
        public virtual string Name { get; protected set; } = string.Empty;

        [JsonProperty("Description")]
        public virtual string Description { get; protected set; } = string.Empty;

        public abstract void HandleEvent();
    }
}


