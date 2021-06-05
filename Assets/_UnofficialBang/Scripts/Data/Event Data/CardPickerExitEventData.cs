namespace Thirties.UnofficialBang
{
    public class CardPickerExitEventData : BaseEventData
    {
        public int CardId { get; set; }

        public bool IsFromHand { get; set; }
    }
}
