# Devverse — Backend

This repository contains the backend for the Devverse application — a NestJS (TypeScript) API that provides authentication, user management, posts (with likes/comments), file uploads, real-time updates via Socket.IO, and Redis-based caching.

The backend uses:
- NestJS (modular architecture)
- MongoDB (Mongoose) for primary data storage
- Redis (ioredis) for caching and pub/sub
- Socket.IO for real-time post updates

Contents
- `src/` — application source (modules: `auth`, `user`, `post`, `task`, `redis`, etc.)
- `uploads/` — static file storage served at `/uploads/`
- `docker-compose.yml` — local services (MongoDB, Redis)

Prerequisites
- Node.js 18+ and npm
- Docker (optional, recommended for local MongoDB/Redis)

Environment
Create a `.env` file in the `backend/` folder (or export env vars). Common values used by the app:

- `PORT` — port to run the server (default 3000)
- `MONGO_URI` — MongoDB connection string (eg. `mongodb://localhost:27017/devverse`)
- `REDIS_HOST` — Redis host (eg. `localhost`)
- `REDIS_PORT` — Redis port (eg. `6379`)
- `JWT_SECRET` — secret used to sign JWTs
- `JWT_EXPIRES_IN` — JWT expiry (eg. `7d`)
- `MAIL_USER` / `MAIL_PASS` — SMTP credentials used for password recovery emails

Quick start (local)

1. Install dependencies

```bash
cd backend
npm install
```

2. Start MongoDB and Redis with Docker Compose (optional but recommended)

```bash
docker-compose up -d
```

3. Run in development mode (with hot reload)

```bash
npm run start:dev
```

The API will be available at `http://localhost:3000` (configurable via `PORT`).

Static files
- Uploaded files are served from the `uploads/` directory and exposed at `/uploads/` by the server. Example: `http://localhost:3000/uploads/avatars/123.png`.

Notable endpoints (high level)
- `POST /auth/register` — register a user
- `POST /auth/login` — authenticate and receive JWT
- `GET /users/me` — get authenticated user
- `GET /posts/feed` — paginated feed
- `POST /posts/:postId/like` — toggle like
- `POST /posts/:postId/comment` — add comment

Socket.IO
- The backend exposes a Socket.IO gateway used to emit `post.updated` events when posts are liked, commented, or changed. The frontend should connect and listen for `post.updated` to update real-time UI.

Testing
- Unit tests and e2e tests are configured with Jest.

```bash
npm run test
npm run test:e2e
npm run test:cov
```

Helpful scripts
- `npm run start:dev` — start in dev mode with watch
- `npm run build` — build the project
- `npm run start:prod` — run built project
- `npm run lint` — run eslint

Troubleshooting
- If the server cannot connect to MongoDB or Redis, verify `MONGO_URI`, `REDIS_HOST`, and `REDIS_PORT` and ensure the services are running (use `docker-compose ps` when running with Docker).
- For email issues, verify `MAIL_USER` / `MAIL_PASS` and network access to the SMTP provider.

Contributing
- The project follows a modular NestJS design. To add features, add a new module under `src/modules/` and register it in `AppModule`.

License
- This project is UNLICENSED (see `package.json`).

Contact
- For questions about this codebase, check the frontend `README` or reach out to the repository owner.
