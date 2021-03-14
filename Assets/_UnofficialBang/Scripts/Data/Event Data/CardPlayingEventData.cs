namespace Thirties.UnofficialBang
{
    public class CardPlayingEventData : BaseEventData
    {
        public int InstigatorId { get; set; }

        public int? TargetId { get; set; }

        public int CardId { get; set; }
    }
}
