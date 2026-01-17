# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Verses is a poetry sharing platform where users can write, publish, and discover poems from other users. It consists of a .NET 10 backend API and a React TypeScript frontend.

## Commands

### Backend (from `backend/` directory)
```bash
# Run the API server
dotnet run

# Run with watch mode
dotnet watch run

# Build
dotnet build

# Run EF migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### Frontend (from `frontend/` directory)
```bash
# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build

# Lint
npm run lint
```

## Architecture

### Backend Structure
- **Controllers/**: API endpoints (AuthController, PoemsController)
- **Services/**: Business logic (TokenService for JWT, PoemService for CRUD)
- **Models/**: Entity models (User extends IdentityUser, Poem)
- **DTOs/**: Request/response data transfer objects
- **Data/**: ApplicationDbContext with EF Core configuration

The backend uses:
- ASP.NET Core Identity for user management
- JWT Bearer authentication
- Entity Framework Core with PostgreSQL
- Swagger/OpenAPI documentation (available at `/swagger` in development)
- Auto-migration on startup in development mode

### Frontend Structure
- **pages/**: Route components (Landing, Login, Register, Write, Feed, MyPoems)
- **components/**: Reusable UI (Navbar, PoemCard, ProtectedRoute, RichTextEditor)
- **context/**: React context (AuthContext for auth state)
- **services/**: API client (axios instance with interceptors)
- **types/**: TypeScript interfaces

Key frontend features:
- Rich text editor (react-quill-new) for poem formatting with bold, italic, headers, lists
- Mural-style feed with paper-like poem cards
- Responsive design

### API Endpoints
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `GET /api/poems/feed` - Public poem feed (paginated)
- `GET /api/poems/{id}` - Get single poem
- `GET /api/poems/my-poems` - User's poems (auth required)
- `POST /api/poems` - Create poem (auth required)
- `PUT /api/poems/{id}` - Update poem (auth required, owner only)
- `DELETE /api/poems/{id}` - Delete poem (auth required, owner only)

API documentation available at `http://localhost:5000/swagger` when running in development.

## Database

PostgreSQL is required. Default connection string expects:
- Host: localhost
- Port: 5432
- Database: poetry_platform
- Username: postgres
- Password: postgres

Configure in `backend/appsettings.json` under `ConnectionStrings:DefaultConnection`.

## Configuration

### Backend (`appsettings.json`)
- `ConnectionStrings:DefaultConnection`: PostgreSQL connection
- `Jwt:Key`: Secret key for JWT signing (min 32 chars)
- `Jwt:Issuer`: JWT issuer
- `Jwt:Audience`: JWT audience
- `Frontend:Url`: CORS allowed origin

### Frontend (`.env`)
- `VITE_API_URL`: Backend API base URL

## Development Setup

1. Start PostgreSQL and create the `poetry_platform` database
2. Run backend: `cd backend && dotnet run` (runs on http://localhost:5000)
3. Run frontend: `cd frontend && npm run dev` (runs on http://localhost:5173)
4. Access Swagger UI: http://localhost:5000/swagger

The backend auto-applies migrations on startup in development mode.

## Content Storage

Poems support HTML content for rich text formatting. The PoemCard component auto-detects HTML content and renders it appropriately.
