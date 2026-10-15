using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIPublicSchool.Data;
using SIPublicSchool.Models;
namespace SIPublicSchool.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) { _db = db; }

    public async Task<IActionResult> Index()
    {
        ViewBag.Notices = await _db.Notices.OrderByDescending(n => n.PostedOn).Take(4).ToListAsync();
        ViewBag.Events = await _db.Events.Where(e => e.Date >= DateTime.Today).OrderBy(e => e.Date).Take(3).ToListAsync();
        ViewBag.StudentCount = await _db.Students.CountAsync();
        ViewBag.TeacherCount = await _db.Teachers.CountAsync();
        return View();
    }
    public IActionResult About() => View();
    public IActionResult Facilities() => View();
    public async Task<IActionResult> Notices() => View(await _db.Notices.OrderByDescending(n => n.PostedOn).ToListAsync());
    public async Task<IActionResult> Events() => View(await _db.Events.OrderBy(e => e.Date).ToListAsync());

    public IActionResult Admissions() => View(new Enquiry());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Admissions(Enquiry e)
    {
        if (!ModelState.IsValid) return View(e);
        e.Type = "Admission"; e.CreatedOn = DateTime.Now;
        _db.Enquiries.Add(e); await _db.SaveChangesAsync();
        TempData["Msg"] = "Thank you! Your admission enquiry has been received. We will contact you soon.";
        return RedirectToAction(nameof(Admissions));
    }

    public IActionResult Contact() => View(new Enquiry());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(Enquiry e)
    {
        if (!ModelState.IsValid) return View(e);
        e.Type = "Contact"; e.CreatedOn = DateTime.Now;
        _db.Enquiries.Add(e); await _db.SaveChangesAsync();
        TempData["Msg"] = "Message sent successfully. We will get back to you shortly.";
        return RedirectToAction(nameof(Contact));
    }

    public IActionResult Error() => View();
}
