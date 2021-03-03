namespace HybridBlazor.Server.Data.Models
{
    public class Counter
    {
        public int Id { get; set; }
        public string AnonymousId { get; set; }

        public int Count { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
