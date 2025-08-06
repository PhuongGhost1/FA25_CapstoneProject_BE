CustomMapOSM - Installation Guide

1. Prerequisites:
- .NET 9 SDK
- Node.js 18.x
- MySQL 8.0+
- MinIO Server
- Redis

2. Backend Setup:
2.1 Clone repository:
git clone https://github.com/your-org/CustomMapOSM

2.2 Configure environment:
# .env
DB_CONNECTION="Server=localhost;Database=mapdb;Uid=admin;Pwd=pass123"
MINIO_ENDPOINT="localhost:9000"
MINIO_ACCESS_KEY="minioadmin"
MINIO_SECRET_KEY="minioadmin"

2.3 Apply migrations:
cd src/Infrastructure
dotnet ef database update

3. Frontend Setup:
3.1 Install dependencies:
cd src/Frontend
npm install

3.2 Configure environment:
# .env.local
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_MAP_TILE_URL=https://{s}.tile.openstreetmap.org

4. Run System:
4.1 Start backend:
dotnet run --project src/API

4.2 Start frontend:
npm run dev

5. Docker Deployment:
5.1 Build containers:
docker-compose -f docker-compose.prod.yml build

5.2 Start stack:
docker-compose -f docker-compose.prod.yml up -d

6. Services:
- API: http://localhost:5000
- Frontend: http://localhost:3000
- MinIO Console: http://localhost:9001
- Adminer (DB): http://localhost:8080

7. First-Time Setup:
1. Access http://localhost:3000/admin/setup
2. Create admin account
3. Seed initial data (templates, layer types)