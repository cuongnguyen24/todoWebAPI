using System.Collections.Generic;

namespace todo_app.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<List> Lists { get; set; }
        public ICollection<Todo> Todos { get; set; }

    }
}
