namespace FaithFlow.Backend.Models
{
    public class ProgressNote
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        public int PrayerId { get; set; }               // Link to the prayer
        public string Content { get; set; } = string.Empty;

        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        public int? MoodRating { get; set; }            // 1-10 scale (optional)
        public Prayer Prayer { get; set; } = null!;
    }
}