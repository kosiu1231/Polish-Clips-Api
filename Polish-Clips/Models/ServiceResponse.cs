namespace Polish_Clips.Models
{
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public int? NoOfElements { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}
