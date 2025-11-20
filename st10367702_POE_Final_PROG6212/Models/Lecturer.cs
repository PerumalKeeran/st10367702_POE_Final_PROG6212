namespace st10367702_POE_Final_PROG6212.Models
{
    public class Lecturer
    {
        public int LecturerId { get; set; }      // PK
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal DefaultHourlyRate { get; set; }
    }
}
