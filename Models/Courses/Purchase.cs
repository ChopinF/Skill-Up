using platform.Models.Account;

namespace platform.Models.Courses
{
    public class Purchase
    {
        public string UserId { get; set; }
        public int CourseId { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public int AmountPaid { get; set; }

        public User User { get; set; }
        public Course Course { get; set; }
    }
}
