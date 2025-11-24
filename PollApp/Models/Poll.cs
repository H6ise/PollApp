using System;
using System.Collections.Generic;

namespace PollApp.Models
{
    public class Poll
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }

        // Свойства, добавленные ранее
        public string CreatorUserId { get; set; }
        public AppUser Creator { get; set; }

        // 🆕 ДОБАВЛЕНО: URL изображения для шапки/превью карточки
        public string HeaderImageUrl { get; set; }

        // 🆕 Поля для сортировки/отслеживания
        public DateTime? LastModified { get; set; }
        public DateTime? LastViewed { get; set; }
        public int Order { get; set; }

        // Навигационные свойства
        public ICollection<Option> Options { get; set; }
    }
}