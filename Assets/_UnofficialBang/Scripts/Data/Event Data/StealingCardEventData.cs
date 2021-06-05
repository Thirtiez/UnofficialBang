namespace Thirties.UnofficialBang
{
    public class StealingCardEventData : BaseEventData
    {
        public int PlayerId { get; set; }
        public int TargetId { get; set; }
        public int CardId { get; set; }
        public bool IsFromHand { get; set; }
    }
}
