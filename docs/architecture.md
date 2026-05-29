# FaithFlow Architecture

**Goal**: A clean, beautiful mobile-first PWA to help users build consistent prayer habits and grow in faith.

## High-Level Architecture

```mermaid
flowchart TD
    subgraph Frontend ["Frontend - Vite + React + TypeScript + Tailwind + PWA"]
        UI[Mobile-First UI\nBottom Navigation]
        PWA[PWA Service Worker\nOffline Support]
        State[TanStack Query + Zustand]
    end

    subgraph Auth ["AWS Cognito\nUser Pool + Hosted UI"]
        Login[Register / Login]
        JWT[JWT Tokens]
    end

    subgraph Backend [".NET 8 Web API + AWS Amplify"]
        API[REST API Endpoints\n/Prayers\n/Journal\n/AI-Suggest]
        AuthMW[Cognito JWT Auth Middleware]
        Lambda[AWS Lambda\nAI Summaries]
    end

    subgraph Data ["Data Layer"]
        DB[(Amazon RDS PostgreSQL\nor DynamoDB)]
        S3[AWS S3\nVoice Notes + Images]
    end

    subgraph AI ["Grok API"]
        Prompts[Prayer Prompts\nEncouragement\nWeekly Summaries]
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
    API --> Lambda
    Lambda --> Grok
    API --> Bible

    Frontend --> Amplify
    Backend --> Amplify
