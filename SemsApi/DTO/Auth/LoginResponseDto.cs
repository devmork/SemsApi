using SemsApi.DTO.Student;
using SemsApi.DTO.Teacher;
using SemsApi.DTO.User;

namespace SemsApi.DTO.Auth
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? Token { get; set; }
        public UserProfileDto? User { get; set; }
        public StudentDto? Student { get; set; }
        public TeacherDto? Teacher { get; set; }
    }
}
