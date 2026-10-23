namespace University.API.Models
{
    public class CourseV2Dto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int StudentsCount { get; set; }
    }
}