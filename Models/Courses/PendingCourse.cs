using platform.Models.Account;

namespace platform.Models.Courses
{
    public class PendingCourse
    {
        public string Title { get; set; }
        public int PendingCourseId { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public Genre Genre { get; set; }

        private List<string> Tags { get; set; } // only for the search queries, we set this up

        public string UserId { get; set; }
        public User User; // every course is uploaded by a user
    }
}
