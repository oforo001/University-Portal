namespace University_Portal.ViewModels.Home
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public int MaxParticipants { get; set; }
        public int RegisteredCount { get; set; }
        public int FreeSlots => MaxParticipants - RegisteredCount;

        public string? ImagePath { get; set; } 
    }
}
