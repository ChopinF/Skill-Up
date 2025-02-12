using platform.Models.Account;

namespace platform.Models.Coaches
{
    public class Coach
    {
        public int CoachId { get; set; }

        public string PhoneNumber { get; set; }
        public string PicturePath { get; set; }
        public string Bio { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public City City { get; set; }
        public Level Level { get; set; }
        public ExpertiseArea ExpertiseArea { get; set; }

        public ICollection<Booking> Bookings { get; } = new List<Booking>();

        public override string ToString()
        {
            return $"Coach ID: {CoachId}, "
                + $"Name: {User?.FirstName} {User?.LastName}, "
                + $"Phone Number: {PhoneNumber}, "
                + $"City: {City}, "
                + $"Level: {Level}, "
                + $"Expertise Area: {ExpertiseArea}, "
                + $"Bio: {Bio}, "
                + $"Picture Path: {PicturePath ?? "No picture available"}";
        }

        //TODO: implement reviews
    }
}
