using Microsoft.AspNetCore.Components.Web;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Polish_Clips.Models
{
    public class Clip
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string EmbedUrl { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string StreamerName { get; set; } = string.Empty;
        public int Duration { get; set; } = 0;
        public int LikeAmount { get; set; } = 0;
        public User? User { get; set; }
        public Game? Game { get; set; }
        public List<Comment>? Comments { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
