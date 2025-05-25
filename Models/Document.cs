//Document
namespace DigitalLockerSystem.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Identity user ID
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
    }
}