namespace PollApp.Models
{
    public class FormAnswer
    {
        public int Id { get; set; }
        public int FormResponseId { get; set; }
        public FormResponse FormResponse { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public int? QuestionOptionId { get; set; }
        public QuestionOption? QuestionOption { get; set; }
        public string? AnswerText { get; set; }

        // Добавленные свойства
        public int PollId { get; set; }
        public string? UserId { get; set; }
        public string? ResponderEmail { get; set; }
        public string? TextAnswer { get; set; }
        public DateTime AnsweredAt { get; set; }

        // Связь с OptionAnswers
        public List<OptionAnswer>? OptionAnswers { get; set; }
    }

    public class OptionAnswer
    {
        public int Id { get; set; }
        public int OptionId { get; set; }
        public FormAnswer FormAnswer { get; set; }
    }
}

