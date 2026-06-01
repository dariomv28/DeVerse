# FookBace - Social Network Application

FookBace is a web-based social network application built with ASP.NET Core MVC. The project implements the core workflows of a small social platform: account registration, authentication, profile management, posting, liking, sharing, commenting, friend requests, friend feeds, and real-time private messaging.

The application is organized as a layered .NET solution that separates presentation, application logic, domain models, persistence, identity, and shared infrastructure concerns.

## Demo Video

https://www.youtube.com/watch?v=Ja2wGImaThw



## Project Goals

The main goal of FookBace is to provide a complete mini social networking experience using server-rendered ASP.NET Core MVC and a clean, maintainable architecture. The project demonstrates how to combine ASP.NET Core Identity, Entity Framework Core, Razor views, SQL Server, file uploads, SMTP email, and SignalR real-time communication in one application.

## Main Features

### Authentication and Account Management

- User registration with first name, last name, username, email, phone number, password, and optional profile picture.
- Login and logout using ASP.NET Core Identity.
- Email confirmation flow after registration.
- Forgot password and reset password flows through email tokens.
- Access control for authenticated pages through authorization filters and session validation.

### Profile Management

- View the current user's profile.
- View other users' profiles from posts, comments, friends, and friend requests.
- Edit personal information such as first name, last name, email, phone number, password, and profile picture.
- Upload and delete profile pictures.
- Display a user's own posts on their profile page.

### Posts and Feed

- Create text posts.
- Upload image attachments for posts.
- Edit and delete the current user's own posts.
- Display the authenticated user's posts on the home feed.
- Display friend posts in the Friends page.
- Reuse a shared post model to support post sharing.
- Preserve shared post references so users can see who originally created shared content.

### Likes, Comments, and Sharing

- Like and unlike posts with asynchronous UI updates.
- Store a unique like per user and post.
- Display live like counts on post cards.
- Add comments to posts.
- Share posts with optional additional text.
- Use a shared Razor partial for post cards across the home feed, friend feed, and profile page.

### Friend System

- Add friends by username.
- Prevent users from adding themselves.
- Prevent duplicate friend requests and duplicate friendships.
- Create friend requests with `Pending` status.
- Accept incoming friend requests.
- Show accepted friends separately from pending requests.
- Delete existing friendships.
- Display posts from accepted friends.

### Real-Time Messaging

- Display accepted friends as message targets on the home page.
- Load conversation history between the authenticated user and a selected friend.
- Send messages through SignalR.
- Deliver messages in real time using SignalR groups based on user IDs.
- Persist messages in the application database.

## Architecture

FookBace follows a layered architecture inspired by Clean Architecture and common ASP.NET Core MVC practices.

```text
FookBace.sln
|
|-- FookBace
|   Presentation layer: MVC controllers, Razor views, SignalR hub, middleware, static assets
|
|-- FookBace.Core.Application
|   Application layer: services, interfaces, DTOs, view models, AutoMapper profiles, helpers
|
|-- FookBace.Core.Domain
|   Domain layer: core entities, shared base entity, application settings models
|
|-- FookBace.Infrastructure.Persistence
|   Persistence layer: ApplicationDbContext, repositories, EF Core migrations
|
|-- FookBace.Infrastructure.Identity
|   Identity layer: IdentityContext, ApplicationUser, account service, identity migrations
|
|-- FookBace.Infrastructure.Shared
|   Shared infrastructure: SMTP email service and shared service registration
```

### Request Flow

```text
Browser
  -> MVC Controller / SignalR Hub
  -> Application Service
  -> Repository or Account Service
  -> EF Core DbContext
  -> SQL Server
```

For normal page interactions, controllers receive requests and delegate business operations to application services. Services use repositories, account services, AutoMapper, session data, and helper classes to perform application logic. Persistence is handled by Entity Framework Core through dedicated infrastructure projects.

For messaging, the browser connects to `/chatHub`. The `ChatHub` stores each connected user in a SignalR group named by the user's ID. When a user sends a message, the message is saved through `MessageService`, then delivered to the receiver's group and echoed to the sender.

## Project Layers

### Presentation Layer - `FookBace`

This is the ASP.NET Core MVC web application. It contains:

- `UserController` for login, registration, email confirmation, forgot password, reset password, and logout.
- `HomeController` for the main feed and message panel.
- `PostController` for creating, editing, deleting, liking, sharing, and commenting on posts.
- `FriendController` for friend requests, accepted friends, and friend feeds.
- `ProfileController` for viewing and editing user profiles.
- `MessageController` for conversation views.
- `ChatHub` for SignalR real-time messaging.
- Razor views, shared layout, reusable post card partials, CSS, JavaScript, Bootstrap, and static images.

### Application Layer - `FookBace.Core.Application`

This layer defines the application's use cases and contracts:

- Service interfaces such as `IPostService`, `IFriendService`, `IMessageService`, `IUserService`, and `IAccountService`.
- Service implementations such as `PostService`, `FriendService`, `MessageService`, `CommentService`, and `UserService`.
- Repository interfaces.
- DTOs for authentication, registration, password reset, and email.
- View models for users, posts, friends, comments, replies, and messages.
- AutoMapper mapping profile.
- Helpers for file uploads and session serialization.

### Domain Layer - `FookBace.Core.Domain`

This layer contains the main business entities:

- `Post`
- `Comment`
- `Reply`
- `Friend`
- `Message`
- `PostLike`
- `AuditableBaseEntity`
- `MailSettings`

The social entities inherit common audit fields such as `Created`, `CreatedBy`, `LastModified`, and `LastModifiedBy`.

### Persistence Layer - `FookBace.Infrastructure.Persistence`

This layer handles application data persistence:

- `ApplicationDbContext` manages posts, comments, replies, friends, messages, and likes.
- Generic repository support for common CRUD operations.
- Specialized repositories such as `PostRepository`, `FriendRepository`, `MessageRepository`, `CommentRepository`, and `ReplyRepository`.
- EF Core migrations for application tables.
- SQL Server support with optional in-memory database configuration.

### Identity Layer - `FookBace.Infrastructure.Identity`

This layer handles user identity and authentication:

- `IdentityContext` extends ASP.NET Core Identity's `IdentityDbContext`.
- `ApplicationUser` extends the default Identity user with first name, last name, and profile picture.
- `AccountService` wraps registration, authentication, email confirmation, password reset, profile lookup, and profile update logic.
- Identity tables use the `Identity` schema.

### Shared Infrastructure - `FookBace.Infrastructure.Shared`

This layer provides reusable infrastructure services:

- SMTP email sending through MailKit and MimeKit.
- Mail settings binding through `IOptions<MailSettings>`.

## Data Model Overview

| Entity | Purpose |
| --- | --- |
| `ApplicationUser` | Identity user with profile fields. |
| `Post` | User post with content, optional attachment, likes, comments, and optional shared post reference. |
| `PostLike` | Tracks which user liked which post and enforces one like per user per post. |
| `Comment` | Comment attached to a post. |
| `Reply` | Reply attached to a comment. |
| `Friend` | Friend request or friendship between two users with `Pending` or `Accepted` status. |
| `Message` | Private message between two users. |

## Technologies and Tools

### Backend

- C#
- ASP.NET Core MVC
- ASP.NET Core Identity
- ASP.NET Core SignalR
- Entity Framework Core
- SQL Server
- AutoMapper
- MailKit
- MimeKit
- Dependency Injection
- Session state
- Razor runtime compilation

### Frontend

- Razor Views
- HTML
- CSS
- Bootstrap
- Bootstrap Icons
- JavaScript
- jQuery
- SignalR JavaScript client

### Data and Infrastructure

- EF Core Code First migrations
- Separate application and identity DbContexts
- Repository pattern
- Service layer pattern
- SMTP email integration
- Local file storage for uploaded profile pictures and post images

### Development Environment

- .NET `net10.0`
- Visual Studio solution structure
- SQL Server / SQL Server Express
- Git



## Notable Implementation Details

- The default route opens the login page through `UserController.Login`.
- Authenticated pages are protected with ASP.NET Core authorization and custom session validation middleware.
- The authenticated user is stored in session as an `AuthenticationResponse`.
- User profile pictures and post attachments are stored under `wwwroot/Images`.
- Post cards are rendered through a shared partial view to keep feeds consistent.
- Like actions use a small JavaScript module and a JSON response from `PostController.ToggleLike`.
- SignalR enables real-time message delivery without refreshing the page.
- Application data and identity data are separated by DbContext while still supporting the same SQL Server database.

## Contributors

- [Bao](https://github.com/BaoAPCS)
- [Dang](https://github.com/dariomv28)
