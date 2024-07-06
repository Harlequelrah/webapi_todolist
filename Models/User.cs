using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace webapi_todolist.Models
{
    public class User
    {
        [JsonIgnore]
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }


        [Required]
        public string Password { get; set; }

        [JsonIgnore]
        public string? RefreshToken { get; set; }
        [JsonIgnore]
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Autres propriétés si nécessaires
    }
}
