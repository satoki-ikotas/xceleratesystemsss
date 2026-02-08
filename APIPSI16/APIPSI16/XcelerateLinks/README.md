# XcelerateLinks MVC Application

## Running Locally

### Prerequisites
- .NET 8.0 SDK
- Running instance of the APIPSI16 API

### Quick Start

1. **Start the API** (in a separate terminal):
   ```bash
   cd ../APIPSI16
   dotnet run
   ```
   The API should start on `http://localhost:5270` by default.

2. **Start the MVC application**:
   ```bash
   dotnet run
   ```

### Configuration

#### API Base URL

The MVC application connects to the API using the `Api:BaseUrl` configuration setting.

**Default**: `http://localhost:5270`

**Override options**:

1. **Via appsettings.Development.json** (already configured):
   ```json
   {
     "Api": {
       "BaseUrl": "http://localhost:5270"
     }
   }
   ```

2. **Via environment variable**:
   ```bash
   Api__BaseUrl=http://localhost:5270 dotnet run
   ```

3. **Via Visual Studio Debug settings**:
   - Right-click the XcelerateLinks project → Properties → Debug
   - Select the appropriate profile (Project or Kestrel)
   - Add environment variable: `Api__BaseUrl` with value `http://localhost:5270`

### Visual Studio F5 Debugging

To run both the API and MVC projects simultaneously:

1. Right-click the solution → Set Startup Projects → Multiple startup projects
2. Set both APIPSI16 and XcelerateLinks to "Start"
3. Ensure each project uses its appropriate launch profile (Kestrel or Project)
4. Add the `Api__BaseUrl` environment variable to XcelerateLinks debug settings if needed

## Authentication

The MVC application authenticates users by:
1. Posting credentials to the API's `/api/auth/login` endpoint
2. Storing the returned JWT token in a secure, HttpOnly cookie named `ApiAccessToken`
3. Using cookie-based authentication for the MVC site itself

## Troubleshooting

- **Connection refused**: Ensure the API is running on the expected port
- **SSL/TLS errors**: In development, the MVC app is configured to accept self-signed certificates
- **Login failures**: Check the MVC console output for detailed error messages from the API
