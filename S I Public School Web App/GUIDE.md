# S.I Public School - Developer Guide (Hinglish)

## 1. Project structure
```
SIPublicSchool/
├─ Program.cs                 -> App start, DB + login (cookie) setup, routes
├─ appsettings.json           -> Database provider + connection strings
├─ Models/
│   ├─ Entities.cs            -> DB tables (AppUser, Student, Teacher, Fee, Notice...)
│   └─ ViewModels.cs          -> Form / dashboard ke data classes
├─ Data/
│   ├─ AppDbContext.cs        -> EF Core context (DbSet = table)
│   └─ DbSeeder.cs            -> Pehli baar demo data + demo logins
├─ Controllers/
│   ├─ HomeController.cs      -> Public pages, admission/contact form
│   ├─ AccountController.cs   -> Login / Logout
│   ├─ AdminController.cs     -> [Authorize(Roles="Admin")]
│   ├─ TeacherController.cs   -> [Authorize(Roles="Teacher")]
│   └─ StudentController.cs   -> [Authorize(Roles="Student")]
├─ Views/
│   ├─ Shared/_Layout.cshtml      -> Public navbar + footer
│   ├─ Shared/_DashLayout.cshtml  -> Dashboard sidebar + topbar
│   └─ Home | Account | Admin | Teacher | Student  (har action ka page)
└─ wwwroot/css/site.css       -> Colors, styling
```
Flow: Browser URL -> Controller action -> DB (EF Core) -> View (.cshtml) -> HTML.

## 2. Run karna
```
dotnet restore
dotnet run            # normal
dotnet watch run      # code change par auto reload (development ke liye best)
```
VS Code: folder open karo, extension "C# Dev Kit" install karo, F5 dabao.
Visual Studio 2022: .csproj double-click -> F5.

## 3. Daily customization (sabse common)
| Kya badalna hai | Kahan |
|---|---|
| School ka naam | Views/Shared/_Layout.cshtml, _DashLayout.cshtml, Login.cshtml (S.I Public School search karo) |
| Phone / email / address | _Layout.cshtml footer + Views/Home/Contact.cshtml |
| Colors | wwwroot/css/site.css top par `--brand`, `--brand2`, `--accent` |
| Hero image | site.css mein `.hero` ke andar url(...) badlo |
| Facilities list | Views/Home/Facilities.cshtml |
| About / Principal message | Views/Home/About.cshtml |
| Classes / Sections list | Models/Entities.cs -> `SchoolData` |
| Demo data | Data/DbSeeder.cs (DB delete karke dobara run) |
| Logo | wwwroot/images/logo.png daalo, navbar mein `<img src="~/images/logo.png" height="36">` lagao |

## 4. Naya feature kaise add karein (example: Gallery)
**Step 1 - Model** (Models/Entities.cs)
```csharp
public class GalleryItem { public int Id {get;set;} public string Title {get;set;} public string ImageUrl {get;set;} }
```
**Step 2 - DbSet** (Data/AppDbContext.cs)
```csharp
public DbSet<GalleryItem> Gallery { get; set; }
```
**Step 3 - Controller action** (HomeController.cs)
```csharp
public async Task<IActionResult> Gallery() => View(await _db.Gallery.ToListAsync());
```
**Step 4 - View** Views/Home/Gallery.cshtml
```html
@model List<GalleryItem>
@foreach (var g in Model) { <img src="@g.ImageUrl" class="img-fluid" /> }
```
**Step 5 - Menu link** (_Layout.cshtml)
```html
<li class="nav-item"><a class="nav-link" asp-controller="Home" asp-action="Gallery">Gallery</a></li>
```
**Step 6 - DB update**: Project mein `EnsureCreated()` use hua hai, to table schema purani DB mein add nahi hota.
- Development: `sipublicschool.db` delete karke `dotnet run`.
- Real data ke saath kaam karna ho to Migrations use karo (section 5).

## 5. Migrations (production-style, recommended jab data real ho)
1. `dotnet tool install --global dotnet-ef`
2. Program.cs mein `db.Database.EnsureCreated();` ko `db.Database.Migrate();` se replace karo
3. Terminal:
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```
Baad mein model badla to: `dotnet ef migrations add <Name>` phir `dotnet ef database update`.
(Provider badalne par migrations dobara banane padte hain.)

## 6. SQL Server use karna
appsettings.json:
```
"DatabaseProvider": "SqlServer"
```
Connection string edit karo (LocalDB ya SQL Express). Pehli run par DB khud ban jayegi.
Full SQL Server: `Server=.;Database=SIPublicSchoolDb;User Id=sa;Password=YOURPASS;TrustServerCertificate=True`

## 7. Roles / Security kaise kaam karta hai
- Login: AccountController -> email + password hash verify -> cookie claims (Id, Name, Role).
- `[Authorize(Roles="Admin")]` controller ke upar -> sirf wo role access kare.
- Naya role chahiye (e.g. Accountant): AppUser.Role = "Accountant", naya controller banao, `[Authorize(Roles="Accountant")]`, aur _DashLayout.cshtml mein menu add karo.
- Demo passwords production se pehle badal do (ya Admin se naye users banao aur seeder ke demo users delete karo).
- Password change feature: AccountController mein action banao, `PasswordHasher<AppUser>` se naya hash save karo.

## 8. Common errors & fix
| Error | Fix |
|---|---|
| `dotnet` not recognized | .NET 8 SDK install karo, terminal restart |
| Port already in use | `dotnet run --urls http://localhost:5050` |
| HTTPS certificate warning | `dotnet dev-certs https --trust` |
| Column / table not found | DB purani hai: .db file delete karo ya migration chalao |
| Login nahi ho raha | DB delete karke dobara run (seed phir se chalega) |
| Chart / icons nahi dikh rahe | Internet chahiye (Bootstrap, Chart.js CDN se aate hain) |
| NuGet restore fail | Internet check, `dotnet nuget locals all --clear` phir `dotnet restore` |

## 9. Deploy (live website)
1. `dotnet publish -c Release -o publish`
2. `publish` folder ko hosting par upload karo:
   - **Azure App Service** (easy), ya
   - **Windows IIS** (Hosting Bundle install karna hoga), ya
   - **Linux VPS** + Nginx + `dotnet SIPublicSchool.dll`
3. Production mein SQL Server use karo, connection string environment variable/secret se do.
4. Domain + HTTPS (Let's Encrypt / Azure managed cert) lagao.

## 10. Aage ke improvement ideas
- Student/Teacher edit page (abhi add + delete hai)
- Online fee payment (Razorpay), receipt PDF
- Report card PDF (QuestPDF library)
- Photo upload (student photo, gallery)
- SMS / email notifications (absent alert)
- Timetable module, library module, transport module
- Password change / forgot password
- Pagination aur export to Excel (ClosedXML)

## 11. Learning ke liye (aapke .NET + SQL Server study plan se match)
Is project mein practice karo: LINQ queries (AdminController), EF relationships (Entities.cs),
Authentication/Authorization (Program.cs + AccountController), Razor views, Chart.js integration.
