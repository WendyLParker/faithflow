using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Backend.Models
{
    public class Prayer
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;     // Cognito Sub ID

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Content { get; set; }

        public DateTime PrayerDate { get; set; } = DateTime.UtcNow;

        public bool IsAnswered { get; set; } = false;

        public DateTime? AnsweredDate { get; set; }

        public List<string> Categories { get; set; } = new();

        // Optional attachments (S3 URLs)
        public string? VoiceNoteUrl { get; set; }
        public string? ImageUrl { get; set; }

        public int StreakDays { get; set; } = 0;

        public ICollection<ProgressNote> ProgressNotes { get; set; } = new List<ProgressNote>();
    }
}