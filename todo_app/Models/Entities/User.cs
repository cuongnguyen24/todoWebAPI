using System.Collections.Generic;

namespace todo_app.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        public ICollection<List> Lists { get; set; }
        public ICollection<Todo> Todos { get; set; }

    }
}
