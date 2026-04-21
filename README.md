# net-api

A comprehensive .NET API project repository built with C# containing multiple projects for learning api and asynchronous programming implementations.

## 📋 Description

A collection of .NET projects demonstrating API development and asynchronous programming using C#. This repository serves as a learning resource.

## 🚀 Getting Started

### Prerequisites

- .NET SDK (latest stable version)
- Visual Studio or Visual Studio Code
- Git

### Installation

1. Clone the repository:
```bash
git clone https://github.com/massielcr/net-api.git
cd net-api
```

2. Install dependencies:
```bash
dotnet restore
```

3. Build all projects:
```bash
dotnet build
```

## 🔧 Development

### Running Individual APIs

Each project contains its own API and runs independently on different ports:

#### Books API
```bash
cd books-api
dotnet run
```
Available at: `http://localhost:5001`

#### Stock Analyzer API
```bash
cd stock-analyzer
dotnet run
```
Available at: `http://localhost:5002`

#### Stock Analyzer Task Runner
```bash
cd stock-analyzer-task-runner
dotnet run
```
Available at: `http://localhost:5003`

#### Learn Microsoft Async
```bash
cd learn-microsoft-async
dotnet run
```
Available at: `http://localhost:5004`

### Running All APIs (From Root)

To build and prepare all projects:
```bash
dotnet build
```

Then navigate to each project directory and run individually, or use a task runner/script to start all services.

### Running Tests

```bash
dotnet test
```

Or test a specific project:
```bash
cd [project-name]
dotnet test
```

## 📦 Project Structure

```
net-api/
├── books-api/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Data/
│   ├── appsettings.json
│   ├── Program.cs
│   ├── [project-name].csproj
│   └── README.md
│
├── stock-analyzer/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Data/
│   ├── appsettings.json
│   ├── Program.cs
│   ├── [project-name].csproj
│   └── README.md
│
├── stock-analyzer-task-runner/
│   ├── Services/
│   ├── Jobs/
│   ├── Models/
│   ├── appsettings.json
│   ├── Program.cs
│   ├── [project-name].csproj
│   └── README.md
│
├── learn-microsoft-async/
│   ├── Examples/
│   ├── Models/
│   ├── Program.cs
│   ├── [project-name].csproj
│   └── README.md
│
├── .gitignore
├── README.md
└── [Configuration files]
```

## 🎯 Projects Overview

### 📚 books-api
**Purpose:** RESTful API for book management

**Port:** `5001`

**Key Features:**
- Full CRUD operations for books
- Advanced search and filtering capabilities
- Author and genre categorization
- Book availability tracking

**Technology Stack:**
- ASP.NET Core
- Entity Framework Core
- SQL Server/SQLite

**API Endpoints:**
- `GET /api/books` - List all books
- `GET /api/books/{id}` - Get book by ID
- `POST /api/books` - Create new book
- `PUT /api/books/{id}` - Update book
- `DELETE /api/books/{id}` - Delete book
- `GET /api/authors` - List authors
- `GET /api/genres` - List genres

---

### 📈 stock-analyzer
**Purpose:** Financial data analysis and stock market insights

**Port:** `5002`

**Key Features:**
- Real-time stock price tracking
- Portfolio analysis and performance metrics
- Market trend analysis and predictions
- Multi-source data integration
- Risk assessment tools

**Technology Stack:**
- ASP.NET Core
- Machine Learning models (optional)
- Financial APIs integration
- Data visualization

**API Endpoints:**
- `GET /api/stocks` - Get stock data
- `GET /api/stocks/{symbol}` - Get specific stock
- `GET /api/portfolio` - Get portfolio analysis
- `GET /api/analysis/trends` - Get market trends
- `POST /api/analysis` - Perform analysis

---

### ⚙️ stock-analyzer-task-runner
**Purpose:** Background service for automated stock analysis

**Port:** `5003`

**Key Features:**
- Scheduled data collection and processing
- Asynchronous background tasks
- Job queue management
- Real-time alerts and notifications
- Error handling and retry logic

**Technology Stack:**
- .NET Worker Service / ASP.NET Core
- Hangfire or Quartz.NET
- Message queuing (RabbitMQ/Azure Service Bus)
- Logging and monitoring

**API Endpoints:**
- `GET /api/tasks` - List running tasks
- `GET /api/tasks/{id}` - Get task status
- `POST /api/tasks` - Create new task
- `GET /api/jobs/history` - View job history
- `POST /api/jobs/trigger` - Manually trigger job

---

### 🎓 learn-microsoft-async
**Purpose:** Educational resource for async programming patterns

**Port:** `5004`

**Key Features:**
- Practical async/await examples
- Task parallelism demonstrations
- Cancellation token usage
- Exception handling in async contexts
- Performance optimization techniques

**Technology Stack:**
- .NET Framework/Core
- TPL (Task Parallel Library)
- Example implementations

**API Endpoints:**
- `GET /api/examples` - List async examples
- `GET /api/examples/{name}` - Get specific example
- `POST /api/execute/{example}` - Execute example

---

## 🔐 Configuration

Each project has its own configuration:
- `appsettings.json` - Default configuration
- `appsettings.Development.json` - Development-specific settings
- `appsettings.Production.json` - Production settings

Update port numbers in `appsettings.json` if needed:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5001"
      }
    }
  }
}
```

## 📝 Contributing

1. Create a feature branch (`git checkout -b feature/AmazingFeature`)
2. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
3. Push to the branch (`git push origin feature/AmazingFeature`)
4. Open a Pull Request

## 🚀 Deployment

### Individual Project Build
```bash
cd [project-name]
dotnet publish -c Release -o ./publish
```

### Docker Support (if available)
Each project may include a Dockerfile for containerization:
```bash
docker build -t [project-name] .
docker run -p [external-port]:5000 [project-name]
```

## 📊 Quick Reference - Running All Projects

Start each in a separate terminal:

```bash
# Terminal 1 - Books API (Port 5001)
cd books-api && dotnet run

# Terminal 2 - Stock Analyzer (Port 5002)
cd stock-analyzer && dotnet run

# Terminal 3 - Stock Analyzer Task Runner (Port 5003)
cd stock-analyzer-task-runner && dotnet run

# Terminal 4 - Learn Microsoft Async (Port 5004)
cd learn-microsoft-async && dotnet run
```

All APIs will be running simultaneously on their respective ports.

## 📝 License

This project is open source and available under the MIT License.

## 👤 Author

- **massielcr** - [@massielcr](https://github.com/massielcr)

## 🤝 Support

For support, please open an issue in the repository or contact the maintainers.

## 🙏 Acknowledgments

- .NET Community
- Microsoft Documentation
- Contributors and users

---

**Last Updated:** April 21, 2026
