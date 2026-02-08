# API Testing Mode with SQLite

## Overview

This API supports two database configurations:
- **SQL Server** (Default) - Uses the production database at sql.bsite.net
- **SQLite** (Testing) - Uses a local SQLite database for testing/CI environments

## Quick Start

### Default Mode (SQL Server)
```bash
cd APIPSI16/APIPSI16/APIPSI16
dotnet run
```
Uses SQL Server at `sql.bsite.net\MSSQL2016` with credentials from appsettings.json

### Testing Mode (SQLite)
```bash
cd APIPSI16/APIPSI16/APIPSI16
USE_SQLITE_FOR_TESTING=true dotnet run
```

This will:
1. Create a SQLite database at `/tmp/xcelerate_test.db`
2. Create all tables automatically
3. Seed test users:
   - **Email**: test@example.com | **Password**: test
   - **Email**: admin@example.com | **Password**: admin

## Testing the API

Once running in SQLite mode, you can test endpoints:

### Login Test
```bash
curl -k https://localhost:7263/api/auth/login \
  -X POST \
  -H "Content-Type: application/json" \
  -d '{"username":"test@example.com","password":"test"}'
```

Expected response:
```json
{
  "token": "eyJhbGci...",
  "expiresAt": "2026-02-08T02:06:32.411Z"
}
```

### Invalid Login Test
```bash
curl -k https://localhost:7263/api/auth/login \
  -X POST \
  -H "Content-Type: application/json" \
  -d '{"username":"wrong","password":"wrong"}'
```

Expected response: `401 Unauthorized`

## Configuration

The testing mode is controlled by the `USE_SQLITE_FOR_TESTING` environment variable:

**In Visual Studio:**
1. Right-click APIPSI16 project → Properties → Debug
2. Add environment variable: `USE_SQLITE_FOR_TESTING` = `true`

**In Docker/CI:**
```yaml
environment:
  - USE_SQLITE_FOR_TESTING=true
```

**In appsettings (alternative):**
You could also add this to appsettings.Development.json:
```json
{
  "UseSqliteForTesting": true
}
```
(Note: Currently using environment variable, but could be extended to support config file)

## Why SQLite for Testing?

- ✅ No external database dependencies
- ✅ Works in CI/CD environments
- ✅ Automatically seeds test data
- ✅ Fast and lightweight
- ✅ No network issues
- ✅ Easy to reset (just delete the .db file)

## Production Deployment

**IMPORTANT**: Never use SQLite mode in production!  
The default mode (without `USE_SQLITE_FOR_TESTING`) will always use SQL Server.

Production deployment should:
1. Not set `USE_SQLITE_FOR_TESTING` environment variable
2. Configure proper SQL Server connection string
3. Use secure password management (environment variables, Azure Key Vault, etc.)

## Troubleshooting

### Database is locked
SQLite database might be locked if a previous process didn't shut down cleanly:
```bash
rm /tmp/xcelerate_test.db
```

### Users not found after restart
The SQLite database file persists between runs. To reset:
```bash
rm /tmp/xcelerate_test.db
USE_SQLITE_FOR_TESTING=true dotnet run
```

### Port already in use
If you get "address already in use" errors:
```bash
# Find the process
lsof -i :5270
# Kill it
kill <PID>
```
