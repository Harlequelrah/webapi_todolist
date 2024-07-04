using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace webapi_todolist.Models
{
    public class TodoItem
    {
        [JsonIgnore]
        [Key]
        public int Id { get; set; }
        public string  Title { get; set; }
        public bool IsCompleted { get; set; }
    }
}
