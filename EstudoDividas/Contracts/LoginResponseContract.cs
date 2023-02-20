namespace EstudoDividas.Contracts
{
    public class LoginResponseContract : MinimalResponseContract
    {
        public string idPublic  { get; set; }
        public string idPrivate { get; set; }
        public string name   { get; set; }
        public string token  { get; set; }
    }
}
