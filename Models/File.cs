using System;

namespace DigitalLockerSystem.Models;

public class File
{
    public int Id { get; set; }
    public string UserId { get; set; }  
    public string OriginalFileName { get; set; }
    public string StoredFileName { get; set; }
    public string ContentType { get; set; }
    public DateTime UploadDate { get; set; }
}