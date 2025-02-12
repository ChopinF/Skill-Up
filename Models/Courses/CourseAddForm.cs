using System.ComponentModel.DataAnnotations;

namespace platform.Models.Courses
{
    public class CourseAddForm()
    {
        [Display(Name = "Title of the course")]
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Display(Name = "Price of the course")]
        [Required(ErrorMessage = "Price is required")]
        public int Price { get; set; }

        [Display(Name = "Description of the course")]
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Display(Name = "The PDF of the course")]
        [Required(ErrorMessage = "PDF is required")]
        public IFormFile File { get; set; }

        [Display(Name = "Genre of the course")]
        public Genre Genre { get; set; }
    }
}
