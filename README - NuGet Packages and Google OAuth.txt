Install the following NuGet packages:
1. Microsoft.EntityFrameworkCore
2. Microsoft.EntityFrameworkCore.Relational
3. Microsoft.EntityFrameworkCore.Abstractions
4. Microsoft.EntityFrameworkCore.Design
5. Microsoft.EntityFrameworkCore.SqlServer
6. Microsoft.EntityFrameworkCore.Tools <-- very important for migrations!
7. Microsoft.AspNetCore.Identity.EntityFrameworkCore
8. Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
9. Microsoft.EntityFramework.Proxies
10. Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore <-- very important if you intend to use scaffolded Identity Razor pages.
11. Microsoft.AspNetCore.Authentication.Google <-- very important for to access Google's external OAuth 2.0 workflow via ASP.NET web application

All must target the same version as the ones initially installed when the ASP.NET MVC Web Application project was created.
To verify, you can double click on the project where the web application resides (typically named Presentation if you are following a three-tier architecture)
which opens the .csproj highlighting which NuGet packages have been installed, as well as the target framework (for instance, .NET 8.0).

It is vital to note that even-numbered editions of .NET Core offer long-term support.

How to store Google OAuth Client ID and Client Secrets?
=======================================================
1. dotnet user-secrets init --project ./<name-of-web-application> (for example: ./Presentation)
2. dotnet user-secrets set "Authentication:Google:ClientId" "316982294478-5qem9ngrt4mtt6ptslt7nb42gedpdqqo.apps.googleusercontent.com" --project ./<name-of-web-application>
3. dotnet user-secrets set "Authentication:Google:ClientSecret" "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" --project ./<name-of-web-application>

How to issue migrations?
========================
1. add-migration "<name-of-migration>" -Context <name-of-migration-class-without-cs-extension> <-- name of migration must have every word start with a capital letter since a C# class will be created based on said name.
2. update-database