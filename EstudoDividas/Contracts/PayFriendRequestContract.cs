namespace EstudoDividas.Contracts
{
    public class PayFriendRequestContract
    {
        public string userPrivateId { get; set; }
        public string userPublicId { get; set;}
        public string friendPublicId { get; set; }
        public float value { get; set; }
        public string description{ get; set; }
    }
}
