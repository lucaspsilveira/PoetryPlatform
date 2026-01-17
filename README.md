# Verses - Poetry Sharing Platform

A web application where users can write, share, and discover poetry. Built with .NET 10 and React.

## Features

- **Write & Publish**: Compose poems with a rich text editor supporting bold, italic, headers, lists, and more
- **Poetry Feed**: Browse a mural-style feed of published poems from the community
- **Manage Your Work**: Edit, publish/unpublish, or delete your poems
- **User Authentication**: Secure registration and login with JWT tokens

## Tech Stack

### Backend
- .NET 10 Web API
- Entity Framework Core with PostgreSQL
- ASP.NET Core Identity for authentication
- JWT Bearer tokens
- Swagger/OpenAPI documentation

### Frontend
- React 18 with TypeScript
- Vite for build tooling
- React Router for navigation
- Axios for API communication
- React Quill for rich text editing

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v20.19+ recommended)
- [PostgreSQL](https://www.postgresql.org/download/)

## Getting Started

### 1. Database Setup

Create a PostgreSQL database:
```sql
CREATE DATABASE poetry_platform;
```

### 2. Backend Setup

```bash
cd backend

# Restore dependencies
dotnet restore

# Run the API (migrations apply automatically in development)
dotnet run
```

The API runs at `http://localhost:5000`. Swagger UI is available at `http://localhost:5000/swagger`.

### 3. Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend runs at `http://localhost:5173`.

## Configuration

### Backend (`backend/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=poetry_platform;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "PoetryPlatform",
    "Audience": "PoetryPlatformUsers"
  }
}
```

### Frontend (`frontend/.env`)

```env
VITE_API_URL=http://localhost:5000/api
```

## API Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/login` | Login | No |
| GET | `/api/poems/feed` | Get published poems | No |
| GET | `/api/poems/{id}` | Get single poem | No |
| GET | `/api/poems/my-poems` | Get user's poems | Yes |
| POST | `/api/poems` | Create poem | Yes |
| PUT | `/api/poems/{id}` | Update poem | Yes |
| DELETE | `/api/poems/{id}` | Delete poem | Yes |

## Project Structure

```
PoetryPlatform/
├── backend/                 # .NET Web API
│   ├── Controllers/         # API endpoints
│   ├── Services/            # Business logic
│   ├── Models/              # Entity models
│   ├── DTOs/                # Data transfer objects
│   ├── Data/                # EF Core DbContext
│   └── Migrations/          # Database migrations
├── frontend/                # React application
│   └── src/
│       ├── pages/           # Route components
│       ├── components/      # Reusable UI components
│       ├── context/         # React context (auth)
│       ├── services/        # API client
│       └── types/           # TypeScript interfaces
├── CLAUDE.md                # AI assistant guidance
└── README.md                # This file
```

## License

MIT
