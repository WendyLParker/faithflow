# FaithFlow 🙏

**A clean, beautiful mobile-first PWA to help build consistent prayer habits and grow in faith.**

Built with love to make prayer tracking simple, encouraging, and meaningful.

## ✨ Features (Planned)

- Track daily prayers with streaks
- Mark prayers as answered with celebration
- AI-powered prayer prompts (Grok)
- Daily Bible verse
- Prayer history & journal
- Offline support (PWA)
- Beautiful mobile experience

## 🛠 Tech Stack

- **Frontend**: Vite + React + TypeScript + Tailwind CSS + PWA
- **Backend**: .NET 8 Web API
- **Auth**: AWS Cognito
- **Hosting**: AWS Amplify
- **AI**: Grok API
- **Storage**: AWS S3 (optional)
- **Database**: SQLite (dev) → RDS / DynamoDB (prod)

## 📁 Project Structure

```bash
faithflow/
├── frontend/              # Vite + React + TS + Tailwind + PWA
├── backend/               # .NET 8 Web API
├── docs/
│   ├── architecture.md
├── README.md
├── .gitignore


```markdown
## 📋 Current Phase

**Phase 1** — Setup & Architecture → **In Progress**

- [x] Create GitHub repo + monorepo folders
- [x] Branching strategy (`main` + `develop`)
- [x] Frontend scaffolding + Tailwind + Bottom Navigation
- [x] Architecture diagram (Mermaid)
- [x] Basic README
- [ ] Setup AWS Amplify + Cognito User Pool
- [ ] Setup .NET 8 Backend

## 🚀 Quick Start (Frontend)

```bash
# From project root
cd frontend
npm install
npm run dev