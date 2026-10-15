using Microsoft.EntityFrameworkCore;
using SIPublicSchool.Models;
namespace SIPublicSchool.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<AttendanceRecord> Attendance { get; set; }
    public DbSet<Notice> Notices { get; set; }
    public DbSet<SchoolEvent> Events { get; set; }
    public DbSet<ExamResult> Results { get; set; }
    public DbSet<Homework> Homeworks { get; set; }
    public DbSet<FeeRecord> Fees { get; set; }
    public DbSet<Enquiry> Enquiries { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();
        b.Entity<FeeRecord>().Property(f => f.Amount).HasColumnType("decimal(18,2)");
    }
}
