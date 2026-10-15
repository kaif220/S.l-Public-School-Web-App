# S.I Public School - ASP.NET Core MVC + Entity Framework Core

## Setup (Laptop)
1. Install .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
2. Install VS Code or Visual Studio 2022.
3. Zip extract karo, folder mein terminal kholo:
       dotnet restore
       dotnet run
4. Browser: URL jo terminal mein dikhe (e.g. http://localhost:5000 ya https://localhost:7xxx)
Database apne aap ban jata hai (SQLite file) aur demo data seed hota hai.

## SQL Server use karna ho
appsettings.json mein  "DatabaseProvider": "SqlServer"  kar do (LocalDB / SQL Express chahiye).
Connection string "SqlServer" wali edit kar sakte ho.

## Demo Logins
Admin   : admin@sipublicschool.com   / Admin@123
Teacher : teacher@sipublicschool.com / Teacher@123
Student : student@sipublicschool.com / Student@123

## Reset database
SQLite: sipublicschool.db file delete karke dobara dotnet run.
