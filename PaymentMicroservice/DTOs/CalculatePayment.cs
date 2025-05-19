namespace PaymentMicroservice.DTOs
{
    public class CalculatePayment
    {
        public string UserId { get; set; }
        public string BookingId { get; set; }
        public string CabType { get; set; }
        public int Passengers { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        public DateTime BookingDateTime { get; set; }
    }
}
