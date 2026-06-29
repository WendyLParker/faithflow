# FaithFlow Architecture

**Goal**: A clean, beautiful mobile-first PWA to help users build consistent prayer habits and grow in faith.

## High-Level Architecture

```mermaid
flowchart TD
    subgraph Frontend ["Frontend - Vite + React + TypeScript + Tailwind + PWA"]
        UI[Mobile-First UI\nBottom Navigation]
        PWA[PWA Service Worker\nOffline Support]
        State[TanStack Query]
    end

    subgraph Auth ["AWS Cognito\nUser Pool"]
        Login[Register / Login]
        JWT[JWT Tokens]
    end

    subgraph Backend [".NET 8 Web API"]
        API[REST API Endpoints\n/Prayers\n/ProgressNotes\n/AI/chat]
        AuthMW[Cognito JWT Auth Middleware]
    end

    subgraph Data ["Data Layer"]
        DB[(SQLite dev → RDS PostgreSQL)]
        S3[AWS S3\nVoice Notes + Images]
    end

    subgraph AI ["OpenAI API"]
        Chat[Prayer Prompts\nEncouragement Chat]
    end

    subgraph External ["External Services"]
        Bible[Bible API\nDaily Verse]
    end

    UI --> PWA
    UI --> State
    State --> API
    API --> AuthMW
    AuthMW --> Auth
    API --> DB
    API --> S3
    API --> Chat
    API --> Bible

    Frontend --> Amplify[AWS Amplify\nplanned hosting]
```

## AI integration (planned)

OpenAI is called **only from the .NET API** — the API key stays on the server. The React app sends user messages to `POST /api/ai/chat`; the backend applies a FaithFlow system prompt and forwards the request to OpenAI (`gpt-4o-mini` or similar).
