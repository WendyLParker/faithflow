# FaithFlow 🙏

![CI](https://github.com/WendyLParker/faithflow/actions/workflows/ci.yml/badge.svg)

**A mobile-first Progressive Web App to help build consistent prayer habits.**

FaithFlow is being built to make daily prayer simple, encouraging, and meaningful — with prayer tracking, answered-prayer celebrations, streaks, AI prompts, and daily Scripture. **The core app is working in local development today; several roadmap features are still in progress.**

## ✅ What works today

### Authentication
- AWS Cognito registration, email confirmation, and login
- JWT-protected API endpoints
- Protected frontend routes

### Prayer requests
- Create, list, view, update, and delete prayer requests
- Category tags (Health, Family, Work, and more)
- Mark requests as answered (“Praise Reports”)
- Dashboard with active vs. answered counts and recent requests

### Backend API
- .NET 8 Web API with Swagger UI (development)
- Entity Framework Core + SQLite (local dev)
- Prayer and Progress Note REST endpoints
- FluentValidation and global exception handling

### Frontend
- Vite + React + TypeScript + Tailwind CSS
- TanStack Query for server state
- Mobile-first layout with bottom navigation
- PWA scaffold (`vite-plugin-pwa` — manifest in place; icons and offline caching still TODO)

### Tooling
- GitHub Actions CI — lint + build on push and pull requests to `main`

## 🚧 Coming soon

- 🔥 Prayer streak counter and encouragement messages
- 🤖 AI assistant (OpenAI) — prayer prompts and encouragement chat
- 📖 Daily Bible verse
- 📊 Progress charts and progress-note journaling UI
- 📎 Voice notes and image attachments (S3)
- 📱 Full offline-capable PWA (installable with proper icons and caching)
- ☁️ Deploy to AWS Amplify and production database (RDS)

## 🛠 Tech stack

| Layer | In use now | Planned |
|-------|------------|---------|
| **Frontend** | Vite, React, TypeScript, Tailwind CSS, TanStack Query | PWA polish, Recharts |
| **Backend** | .NET 8 Web API, EF Core, SQLite | OpenAI chat proxy endpoint |
| **Auth** | AWS Cognito | — |
| **Hosting** | Local dev | AWS Amplify |
| **Storage** | — | AWS S3 (voice notes & images) |
| **Database** | SQLite (dev) | Amazon RDS or DynamoDB |
| **AI** | — | OpenAI API (`gpt-4o-mini`, server-side) |

## 📁 Project structure

```bash
faithflow/
├── frontend/          # React + Vite PWA
├── backend/           # .NET 8 Web API
│   └── FaithFlow.Api/
├── docs/              # Architecture notes
└── .github/workflows/ # CI
```

## 🚀 Quick start

### Prerequisites
- Node.js 20+
- .NET 8 SDK
- AWS account (Cognito user pool configured for auth)

### 1. Clone the repository

```bash
git clone https://github.com/WendyLParker/faithflow.git
cd faithflow
```

### 2. Frontend

```bash
cd frontend
cp .env.example .env   # set VITE_API_BASE_URL if needed
npm install
npm run dev
```

Open [http://localhost:5173](http://localhost:5173)

### 3. Backend

```bash
cd backend/FaithFlow.Api
dotnet restore
dotnet run
```

- API: [http://localhost:5000](http://localhost:5000)
- Swagger (dev only): [http://localhost:5000/swagger](http://localhost:5000/swagger)

Configure Cognito settings in `appsettings.Development.json` before using auth endpoints.

## 📋 Available scripts

### Frontend
- `npm run dev` — Start development server
- `npm run build` — Production build
- `npm run lint` — Run ESLint

### Backend
- `dotnet watch run` — Run with hot reload
- `dotnet build` — Compile the API

## 🏗 Project phases

- [x] **Phase 1:** Setup & architecture
- [x] **Phase 2:** Backend core + Cognito authentication *(prayer & progress-note APIs)*
- [ ] **Phase 3:** AI integration *(OpenAI — prayer prompts & encouragement chat)*
- [ ] **Phase 4:** Frontend UI *(in progress — auth + prayer flows done; progress notes & AI tab pending)*
- [ ] **Phase 5:** Polish & faith features *(streaks, charts, daily verse, celebrations)*
- [ ] **Phase 6:** Deploy to AWS Amplify *(amplify.yml started; not live yet)*

See [docs/architecture.md](docs/architecture.md) for the full system design.

## 📄 License

MIT License

---

**Made with ❤️ for growing in faith**
