namespace EstudoDividas.Contracts
{
    // Esta é a reponse minima.
    // Outras responses devem derivar dela.
    // Eu poderia ter definido como abstract, mas são campos que funcionam por si só.
    public class MinimalResponseContract
    {
        public string status { get; set; }
        public string message { get; set; }
        
    }
}
