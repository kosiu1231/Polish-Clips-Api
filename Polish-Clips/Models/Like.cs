namespace Polish_Clips.Models
{
    public class Like
    {
        public int Id { get; set; }
        public User? User { get; set; }
        public Clip? Clip { get; set; }
    }
}
