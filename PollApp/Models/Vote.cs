using System;

namespace PollApp.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public int OptionId { get; set; }
        public Option Option { get; set; } // Навигационное свойство

        public string UserId { get; set; }

        // 🆕 ДОБАВЛЕНО: Навигационное свойство к пользователю
        public AppUser User { get; set; }

        public DateTime VoteDate { get; set; } = DateTime.UtcNow;
    }
}