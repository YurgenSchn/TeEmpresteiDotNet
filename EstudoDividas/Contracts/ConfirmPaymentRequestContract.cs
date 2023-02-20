namespace EstudoDividas.Contracts
{
    public class ConfirmPaymentRequestContract
    {
        public string userPrivateId { get; set; }
        public string userPublicId { get; set;}
        public int paymentId { get; set;}
    }
}
