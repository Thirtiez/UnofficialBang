namespace Thirties.UnofficialBang
{
    public class CardMouseOverEnterEventData : BaseEventData
    {
        public CardView CardView { get; set; }

        public bool IsPlayable { get; set; }
    }
}
