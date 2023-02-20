namespace EstudoDividas.Contracts.ReturnTypes
{
    public class PaymentReturnType
    {
        public string sender { get; set; }
        public string receiver { get; set; }
        public float value { get; set; }
        public string description{ get; set; }
        public string date { get; set; }
        public bool confirmed { get; set; }
    }
}
