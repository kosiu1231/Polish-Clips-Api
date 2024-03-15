using System.Text.Json.Serialization;

namespace Polish_Clips.Models
{
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        User = 1,
        Admin = 2
    }
}
