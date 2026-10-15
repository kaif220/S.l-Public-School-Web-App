using System.ComponentModel.DataAnnotations;
namespace SIPublicSchool.Models;

public class LoginVm
{
    [Required, EmailAddress] public string Email { get; set; }
    [Required, DataType(DataType.Password)] public string Password { get; set; }
}

public class StudentForm
{
    [Required] public string FullName { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    [Required, MinLength(6)] public string Password { get; set; }
    [Required] public string RollNo { get; set; }
    [Required] public string ClassName { get; set; }
    [Required] public string Section { get; set; }
    public string ParentName { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
}

public class TeacherForm
{
    [Required] public string FullName { get; set; }
    [Required, EmailAddress] public string Email { get; set; }
    [Required, MinLength(6)] public string Password { get; set; }
    [Required] public string Subject { get; set; }
    public string Qualification { get; set; }
    public string Phone { get; set; }
}

public class AdminDashVm
{
    public int Students, Teachers, Enquiries, Notices;
    public decimal FeesCollected, FeesPending;
    public List<string> ClassLabels = new();
    public List<int> ClassCounts = new();
}

public class StudentDashVm
{
    public Student Student;
    public double AttendancePercent;
    public double AvgPercent;
    public decimal PendingFees;
    public int HomeworkCount;
    public List<string> SubjectLabels = new();
    public List<double> SubjectPercents = new();
    public List<Notice> Notices = new();
}

public class TeacherDashVm
{
    public Teacher Teacher;
    public int TotalStudents;
    public int HomeworkPosted;
    public int TodayPresent;
    public List<Notice> Notices = new();
    public List<SchoolEvent> Events = new();
}

public class AttendanceVm
{
    public string ClassName { get; set; }
    public string Section { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public List<Student> Students { get; set; } = new();
    public HashSet<int> PresentIds { get; set; } = new();
}
