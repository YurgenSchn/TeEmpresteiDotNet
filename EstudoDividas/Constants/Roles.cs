namespace EstudoDividas.Constants
{
    public class Roles
    {
        // atributos const já são estaticos, não precisarei instanciar para ter acesso

        public static List<string> roles = new(){ "usuario", "admin" };

        public const string usuario = "usuario";
        public const string admin   = "admin";
    }
}
