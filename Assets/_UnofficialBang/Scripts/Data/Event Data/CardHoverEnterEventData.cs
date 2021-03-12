namespace Thirties.UnofficialBang
{
    public class CardHoverEnterEventData : BaseEventData
    {
        public CardView CardView { get; set; }

        public bool IsPlayable { get; set; }
    }
}
