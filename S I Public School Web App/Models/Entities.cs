using System.ComponentModel.DataAnnotations;
namespace SIPublicSchool.Models;

public static class SchoolData
{
    public static readonly string[] Classes = { "Nursery", "LKG", "UKG", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
    public static readonly string[] Sections = { "A", "B", "C" };
}

public class AppUser
{
    public int Id { get; set; }
    [Required] public string FullName { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; } // Admin / Teacher / Student
}

public class Student
{
    public int Id { get; set; }
    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; }
    public string RollNo { get; set; }
    public string ClassName { get; set; }
    public string Section { get; set; }
    public string ParentName { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
}

public class Teacher
{
    public int Id { get; set; }
    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; }
    public string Subject { get; set; }
    public string Qualification { get; set; }
    public string Phone { get; set; }
}

public class AttendanceRecord
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; }
    public DateTime Date { get; set; }
    public bool Present { get; set; }
}

public class Notice
{
    public int Id { get; set; }
    [Required] public string Title { get; set; }
    [Required] public string Body { get; set; }
    public DateTime PostedOn { get; set; } = DateTime.Now;
}

public class SchoolEvent
{
    public int Id { get; set; }
    [Required] public string Title { get; set; }
    public string Description { get; set; }
    [DataType(DataType.Date)] public DateTime Date { get; set; } = DateTime.Today;
}

public class ExamResult
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; }
    public string Exam { get; set; }
    public string Subject { get; set; }
    public int Marks { get; set; }
    public int MaxMarks { get; set; } = 100;
}

public class Homework
{
    public int Id { get; set; }
    public string ClassName { get; set; }
    public string Subject { get; set; }
    [Required] public string Title { get; set; }
    public string Description { get; set; }
    [DataType(DataType.Date)] public DateTime DueDate { get; set; } = DateTime.Today.AddDays(2);
    public string TeacherName { get; set; }
}

public class FeeRecord
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    [DataType(DataType.Date)] public DateTime DueDate { get; set; } = DateTime.Today.AddDays(15);
    public bool IsPaid { get; set; }
    public DateTime? PaidOn { get; set; }
}

public class Enquiry
{
    public int Id { get; set; }
    [Required] public string Name { get; set; }
    [Required] public string Phone { get; set; }
    public string Email { get; set; }
    public string ClassApplying { get; set; }
    public string Message { get; set; }
    public string Type { get; set; } // Admission / Contact
    public DateTime CreatedOn { get; set; } = DateTime.Now;
}
