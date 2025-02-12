using platform.Models.Account;

namespace platform.Models.Coaches
{
    public class PendingCoach
    {
        public int PendingCoachId { get; set; }

        public string PhoneNumber { get; set; }
        public string PicturePath { get; set; }
        public string Bio { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public City City { get; set; }
        public Level Level { get; set; }
        public ExpertiseArea ExpertiseArea { get; set; }

        //TODO: implement reviews
    }
}
