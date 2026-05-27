<!-- ABOUT THE PROJECT -->
# Social Network

This project is a mini social network application built using ASP.NET Core MVC. It includes several key functionalities:

* **Login and Registration:** Allows users to log in or register a new account. Password reset functionality is also available.
* **Home (Publications):** Displays a feed of posts with comments, with options to create, edit, or delete posts.
* **Friends:** Allows users to manage their friends list, view friends' posts, and add or remove friends.
* **My Profile:** Users can edit their profile information, including name, email, profile picture, and password.

## Technologies Used

- **Backend**
  - C# ASP.NET Core MVC (6.0)
  - Microsoft Entity Framework Core (Code First approach)
      - Microsoft.EntityFrameworkCore.Relational
      - Microsoft.EntityFrameworkCore.SqlServer
      - Microsoft.EntityFrameworkCore.Tools
      - Microsoft.EntityFrameworkCore.Design
  - AutoMapper
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore
  - Microsoft.Extensions.DependencyInjection
  - Microsoft.Extensions.Options.ConfigurationExtensions

- **Frontend**
  - HTML
  - CSS
  - Bootstrap
  - ASP.NET Razor

- **ORM**
  - Entity Framework

- **Database**
  - SQL Server

## Getting Started

### Prerequisites
To run this project, you'll need:
- Visual Studio with ASP.NET Core SDK (6 onwards)
- SQL Server

### Installation
1. Clone the repository or download the project.
2. Open the project in Visual Studio.
3. Update the database connection string in `appsettings.json` to match your SQL Server setup.
4. Update the mail settings in `appsettings.json` with your SMTP server details for email functionalities:
   ```json
   "MailSettings": {
    "EmailFrom": "your-email@example.com",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "your-email@example.com",
    "SmtpPass": "your-smtp-password",
    "DisplayName": "Social Network mail"
   }
5. Open Package Manager Console in Visual Studio and run `Update-Database` to apply migrations.
6. Run the project and access it in your browser.

## Project Screenshots

* Login Page

![Login Page](https://github.com/AleGxrcia/social-network-app/blob/main/SocialNetwork/wwwroot/ProjectImages/Login.png)

* Home (Publications)

![Home Page](https://github.com/AleGxrcia/social-network-app/blob/main/SocialNetwork/wwwroot/ProjectImages/HomePage.png)

* My Profile

![Profile Page](https://github.com/AleGxrcia/social-network-app/blob/main/SocialNetwork/wwwroot/ProjectImages/MyProfile.png)

* Friends

![Friends Page](https://github.com/AleGxrcia/social-network-app/blob/main/SocialNetwork/wwwroot/ProjectImages/FriendPosts.png)

## Developer
- [Federico A. Garcia](https://github.com/AleGxrcia)

