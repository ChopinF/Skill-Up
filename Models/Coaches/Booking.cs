using platform.Models.Account;

namespace platform.Models.Coaches
{
    public class Booking
    {
        // many to many between coach and users
        public int CoachId { get; set; }
        public Coach Coach { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public DateTime Date { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
