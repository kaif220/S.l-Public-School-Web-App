using Microsoft.AspNetCore.Identity;
using SIPublicSchool.Models;
namespace SIPublicSchool.Data;

public static class DbSeeder
{
    static AppUser Make(string name, string email, string pass, string role)
    {
        var u = new AppUser { FullName = name, Email = email, Role = role };
        u.PasswordHash = new PasswordHasher<AppUser>().HashPassword(u, pass);
        return u;
    }

    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any()) return;

        db.Users.Add(Make("School Admin", "admin@sipublicschool.com", "Admin@123", "Admin"));

        var t1 = new Teacher { AppUser = Make("Anita Sharma", "teacher@sipublicschool.com", "Teacher@123", "Teacher"), Subject = "Mathematics", Qualification = "M.Sc, B.Ed", Phone = "9876500001" };
        var t2 = new Teacher { AppUser = Make("Rahul Verma", "rahul@sipublicschool.com", "Teacher@123", "Teacher"), Subject = "English", Qualification = "M.A, B.Ed", Phone = "9876500002" };
        db.Teachers.AddRange(t1, t2);

        var s1 = new Student { AppUser = Make("Aarav Khan", "student@sipublicschool.com", "Student@123", "Student"), RollNo = "101", ClassName = "10", Section = "A", ParentName = "Imran Khan", Phone = "9876511111", Address = "Pune" };
        var s2 = new Student { AppUser = Make("Sara Patel", "sara@sipublicschool.com", "Student@123", "Student"), RollNo = "102", ClassName = "10", Section = "A", ParentName = "Nilesh Patel", Phone = "9876522222", Address = "Pune" };
        var s3 = new Student { AppUser = Make("Vihaan Singh", "vihaan@sipublicschool.com", "Student@123", "Student"), RollNo = "201", ClassName = "9", Section = "B", ParentName = "Raj Singh", Phone = "9876533333", Address = "Mumbai" };
        db.Students.AddRange(s1, s2, s3);
        db.SaveChanges();

        var rnd = new Random(7);
        foreach (var s in new[] { s1, s2, s3 })
        {
            for (int d = 1; d <= 10; d++)
                db.Attendance.Add(new AttendanceRecord { StudentId = s.Id, Date = DateTime.Today.AddDays(-d), Present = rnd.Next(10) > 1 });
            foreach (var sub in new[] { "Mathematics", "English", "Science", "Hindi", "Social Science" })
                db.Results.Add(new ExamResult { StudentId = s.Id, Exam = "Unit Test 1", Subject = sub, Marks = rnd.Next(55, 99), MaxMarks = 100 });
            db.Fees.Add(new FeeRecord { StudentId = s.Id, Description = "Tuition Fee - Q1", Amount = 12000, DueDate = DateTime.Today.AddDays(-30), IsPaid = true, PaidOn = DateTime.Today.AddDays(-35) });
            db.Fees.Add(new FeeRecord { StudentId = s.Id, Description = "Tuition Fee - Q2", Amount = 12000, DueDate = DateTime.Today.AddDays(10), IsPaid = false });
        }

        db.Notices.AddRange(
            new Notice { Title = "Admissions Open 2026-27", Body = "Admissions are open for Nursery to Class 9. Visit the school office or apply online.", PostedOn = DateTime.Today.AddDays(-2) },
            new Notice { Title = "Parent-Teacher Meeting", Body = "PTM for classes 6 to 10 will be held this Saturday from 10 AM to 1 PM.", PostedOn = DateTime.Today.AddDays(-5) },
            new Notice { Title = "Diwali Vacation", Body = "School will remain closed from 20 Oct to 26 Oct on account of Diwali.", PostedOn = DateTime.Today.AddDays(-7) });

        db.Events.AddRange(
            new SchoolEvent { Title = "Annual Sports Day", Description = "Track and field events, relay races and prize distribution.", Date = DateTime.Today.AddDays(20) },
            new SchoolEvent { Title = "Science Exhibition", Description = "Students showcase working models and innovations.", Date = DateTime.Today.AddDays(35) },
            new SchoolEvent { Title = "Annual Day Celebration", Description = "Cultural programs, dance, drama and music performances.", Date = DateTime.Today.AddDays(60) });

        db.Homeworks.Add(new Homework { ClassName = "10", Subject = "Mathematics", Title = "Quadratic Equations Exercise 4.2", Description = "Solve Q1 to Q10 from NCERT.", DueDate = DateTime.Today.AddDays(2), TeacherName = "Anita Sharma" });
        db.SaveChanges();
    }
}
