namespace WebApi429
{
    public class Api429
    {
        public int Count { get; set; } = 0;
        public DateTime Reset429 { get; set; } = DateTime.UtcNow;
        public DateTime LastRequest { get; set; } = DateTime.UtcNow;
    }
}