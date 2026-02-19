# Chat App

A full-stack real-time chat application built with **.NET 10** and **React 19 / TypeScript**, demonstrating production-grade architectural patterns including Clean Architecture, CQRS, and Domain-Driven Design.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Backend Setup](#backend-setup)
  - [Frontend Setup](#frontend-setup)
- [API Overview](#api-overview)
- [Environment Variables](#environment-variables)

---

## Features

- User registration and login with JWT authentication
- Public group chat rooms and private 1-to-1 chats
- Real-time messaging via SignalR (WebSocket)
- Cursor-based paginated message history
- Read receipts
- User search
- Scaffolding for end-to-end encryption (public/private key management)

---

## Tech Stack

### Backend
| Category | Technology |
|---|---|
| Runtime | .NET 10 |
| Framework | ASP.NET Core 10 (Minimal APIs) |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL (via Npgsql) |
| Real-time | ASP.NET Core SignalR |
| Messaging | MediatR 14 (CQRS) |
| Validation | FluentValidation 12 |
| Authentication | JWT Bearer + BCrypt |
| API Docs | Swagger / OpenAPI |

### Frontend
| Category | Technology |
|---|---|
| Framework | React 19 + TypeScript |
| Build Tool | Vite 7 |
| State Management | Zustand 5 |
| Routing | React Router DOM 7 |
| HTTP Client | Axios |
| Real-time | @microsoft/signalr |
| Styling | Tailwind CSS 4 |
| Icons | Lucide React |
| Notifications | react-hot-toast |

---

## Architecture

The backend follows **Clean Architecture** with four layers and a strict inward dependency rule:

```
ChatApp.Domain          ← Entities, Value Objects, Domain Events, Interfaces
ChatApp.Application     ← CQRS Commands/Queries, MediatR Handlers, Validators
ChatApp.Infrastructure  ← EF Core DbContext, Repositories, JWT, SignalR Hub
ChatApp.API             ← Minimal API Endpoints, Middleware, DI Composition
```

### Key Patterns

- **CQRS** — every business operation is a MediatR `IRequest` command or query
- **Domain-Driven Design** — aggregate roots (`User`, `ChatRoom`), value objects (`EmailAddress`, `MessageContent`), and domain events (`MessageSentEvent`)
- **Domain Events** — dispatched post-persistence inside `SaveChangesAsync` via MediatR
- **Pipeline Behaviours** — `ValidationBehaviour<,>` auto-validates all commands/queries with FluentValidation before handler execution
- **Repository Pattern + Unit of Work** — domain interfaces implemented in the Infrastructure layer

---

## Project Structure

```
├── backend/
│   ├── ChatApp.API/            # Minimal API endpoints, middleware, app entry point
│   │   ├── Endpoints/          # AuthEndpoints, ChatEndpoints
│   │   ├── Extensions/         # Swagger configuration
│   │   └── Middleware/         # Global exception handling
│   ├── ChatApp.Application/    # CQRS features, validators, behaviours
│   │   └── Features/
│   │       ├── ChatRoom/       # Commands & queries for rooms and messages
│   │       └── Users/          # Commands & queries for auth and user management
│   ├── ChatApp.Domain/         # Core business logic (no dependencies)
│   │   ├── Entities/           # User, ChatRoom, Message, ReadReceipt
│   │   ├── ValueObjects/       # EmailAddress, MessageContent
│   │   ├── Events/             # MessageSentEvent
│   │   └── Interfaces/         # Repository and UoW contracts
│   └── ChatApp.Infrastructure/ # EF Core, PostgreSQL, JWT, SignalR, BCrypt
│       ├── Authentication/     # JwtProvider, PasswordHasher
│       ├── Persistence/        # ApplicationDbContext, Repositories, Migrations
│       └── RealTime/           # SignalR Hub
└── frontend/
    └── src/
        ├── pages/              # LoginPage, RegisterPage, ChatPage
        ├── components/         # Sidebar, ChatArea, ProtectedRoute, ErrorBoundary
        ├── store/              # useAuthStore, useChatStore (Zustand)
        └── types/              # TypeScript type definitions
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/)

### Backend Setup

1. Clone the repository and navigate to the backend folder:
   ```bash
   cd backend
   ```

2. Update the connection string and JWT settings in `ChatApp.API/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=chatapp;Username=your_user;Password=your_password"
     },
     "JwtSettings": {
       "Issuer": "ChatApp",
       "Audience": "ChatApp",
       "SecretKey": "your-secret-key-min-32-characters"
     }
   }
   ```

3. Apply database migrations:
   ```bash
   dotnet ef database update --project ChatApp.Infrastructure --startup-project ChatApp.API
   ```

4. Run the API:
   ```bash
   dotnet run --project ChatApp.API
   ```

The API will be available at `https://localhost:5000`. Swagger UI is available at `/swagger`.

### Frontend Setup

1. Navigate to the frontend folder:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   ```

The frontend will be available at `http://localhost:5001`.

---

## API Overview

### Auth
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Register a new user |
| `POST` | `/api/auth/login` | Login and receive a JWT |

### Chat
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/chat/rooms/public` | Create a public room |
| `POST` | `/api/chat/rooms/private` | Create a private chat |
| `POST` | `/api/chat/rooms/{id}/join` | Join a public room |
| `POST` | `/api/chat/rooms/{id}/users` | Add a user to a room |
| `POST` | `/api/chat/rooms/{id}/messages` | Send a message |
| `POST` | `/api/chat/rooms/{id}/read` | Mark room as read |
| `GET` | `/api/chat/rooms` | Get all rooms for current user |
| `GET` | `/api/chat/rooms/{id}/messages` | Get paginated message history |

### Real-time Hub
| Endpoint | Description |
|---|---|
| `/chatHub` | SignalR WebSocket hub (JWT via `?access_token=`) |

---

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `JwtSettings__SecretKey` | — | HMAC-SHA256 signing key (min 32 chars) |
| `JwtSettings__Issuer` | `ChatApp` | JWT issuer |
| `JwtSettings__Audience` | `ChatApp` | JWT audience |
| `ConnectionStrings__DefaultConnection` | — | PostgreSQL connection string |
