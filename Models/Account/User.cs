using Microsoft.AspNetCore.Identity;
using platform.Models.Coaches;
using platform.Models.Courses;

namespace platform.Models.Account
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid ActivationCode { get; set; }
        public Guid ResetPasswordCode { get; set; }

        public ICollection<Course> Courses { get; } = new List<Course>();
        public ICollection<PendingCourse> PendingCourses { get; } = new List<PendingCourse>();
        public ICollection<Purchase> Purchases { get; } = new List<Purchase>();
        public ICollection<Booking> Bookings { get; } = new List<Booking>();

        public Coach? Coach { get; set; } = null;
        public PendingCoach? PendingCoach { get; set; } = null;
    }
}
