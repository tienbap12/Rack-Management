namespace Rack.Contracts.Card.Request
{
    public class UpdateCardRequest
    {
        public Guid DeviceID { get; set; }
        public string? CardType { get; init; }
        public string? CardName { get; init; }
        public string? SerialNumber { get; set; }
    }
}