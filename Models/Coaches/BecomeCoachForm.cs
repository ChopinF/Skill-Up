namespace platform.Models.Coaches
{
    public class BecomeCoachForm
    {
        public string PhoneNumber { get; set; }
        public IFormFile PicturePath { get; set; }
        public string Bio { get; set; }

        public City City { get; set; }
        public Level Level { get; set; }
        public ExpertiseArea ExpertiseArea { get; set; }

        public override string ToString()
        {
            var pictureName = PicturePath?.FileName ?? "No file uploaded";
            return $"Phone Number: {PhoneNumber}, Bio: {Bio}, City: {City}, Level: {Level}, Expertise Area: {ExpertiseArea}, Profile Picture: {pictureName}";
        }
    }
}
