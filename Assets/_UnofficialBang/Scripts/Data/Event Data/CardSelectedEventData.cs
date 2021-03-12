namespace Thirties.UnofficialBang
{
    public class CardSelectedEventData : BaseEventData
    {
        public CardData CardData { get; set; }

        public int? Range { get; set; }
    }
}
