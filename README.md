# FaithFlow 🙏

**A beautiful mobile-first Progressive Web App to help build consistent prayer habits.**

FaithFlow makes daily prayer simple, encouraging, and meaningful — with streak tracking, AI-powered prompts, answered prayer celebrations, and daily Bible verses.


## ✨ Features

- 📝 Easy prayer logging with categories
- 🔥 Prayer streak counter + encouragement messages
- 🤖 AI Assistant (Grok) – prayer prompts & weekly summaries
- 📖 Daily Bible verse
- ✅ Mark prayers as answered with celebration
- 📊 Beautiful progress charts
- 📱 Fully offline capable PWA (installable on phone)
- 🔐 Secure authentication with AWS Cognito

## 🛠 Tech Stack

| Layer       | Technology |
|-------------|----------|
| **Frontend** | Vite + React + TypeScript + Tailwind CSS + PWA |
| **Backend**  | .NET 8 Web API + AWS Lambda |
| **Auth**     | AWS Cognito |
| **Hosting**  | AWS Amplify |
| **Storage**  | AWS S3 (voice notes & images) |
| **Database** | SQLite (dev) → Amazon RDS / DynamoDB |
| **AI**       | Grok API |

## 📁 Project Structure
```bash
faithflow/
├── frontend/  # React + Vite PWA
├── backend/   # .NET 8 Web API
├── docs/      # Architecture & notes
└── README.md
```

---
## 🚀 Quick Start

### Prerequisites
- Node.js (v20 or higher)
- .NET 8 SDK
- AWS Account (for Amplify & Cognito)

### 1. Clone the Repository
```base
git clone
cd faithflow
```

### 2. Frontend
```bash
cd frontend
npm install
npm run dev
```

Open this URL in browser: http://localhost:5173

### 3. Backend
```bash
cd backend
dotnet restore
dotnet run
```

API will run at: https://localhost:5001 (or similar port)

## 📋 Available Scripts

### Frontend
- `npm run dev` — Start development server
- `npm run build` — Create production build
- `npm run lint` — Run ESLint

### Backend
- `dotnet watch run` — Run with hot reload
- `dotnet build`

## 🏗 Project Phases

- [x] Phase 1: Setup & Architecture
- [ ] Phase 2: Backend Core + Authentication
- [ ] Phase 3: AI Integration
- [ ] Phase 4: Frontend UI
- [ ] Phase 5: Polish & Faith Features
- [ ] Phase 6: Deploy to AWS Amplify

## 📄 License

MIT License

---

**Made with ❤️ for growing in faith**