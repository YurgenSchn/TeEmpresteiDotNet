namespace EstudoDividas.Contracts.ReturnTypes
{
    public class PaymentReturnType
    {
        public int id { get; set; }
        public string sender_id { get; set; }
        public string sender_name { get; set; }
        public string receiver_id { get; set; }
        public string receiver_name { get; set; }
        public float value { get; set; }
        public string description{ get; set; }
        public string date { get; set; }
        public bool confirmed { get; set; }
    }
}
