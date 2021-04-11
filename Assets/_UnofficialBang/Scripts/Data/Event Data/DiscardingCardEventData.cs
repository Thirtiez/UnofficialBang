namespace Thirties.UnofficialBang
{
    public class DiscardingCardEventData : BaseEventData
    {
        public int PlayerId { get; set; }
        public int CardId { get; set; }
        public bool IsFromHand { get; set; }
    }
}
