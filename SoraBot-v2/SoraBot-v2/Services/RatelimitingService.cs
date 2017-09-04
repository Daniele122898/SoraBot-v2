namespace SoraBot_v2.Services
{
    public class RatelimitingService
    {
        
    }

    internal class Bucket
    {
        public sbyte Fill { get; set; }
        public sbyte Counter { get; set; }
    }
}