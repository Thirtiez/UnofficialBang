namespace Thirties.UnofficialBang
{
    public class PlayingCardEventData : BaseEventData
    {
        public int InstigatorId { get; set; }
        public int TargetId { get; set; }
        public int CardId { get; set; }
    }
}
