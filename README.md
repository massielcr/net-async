# net-async

A comprehensive .NET project repository built with C# containing multiple projects for learning asynchronous programming implementations.

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

# Project Structure

This project is organized into several folders representing different components:

## books-api

- **BookCovers.API**  
- **Books.API**  
- **Books.Legacy**  
- **Books.slnx**

## stock-analyzer

- **StockAnalyzer.Core**  
- **StockAnalyzer.Web**  
- **StockAnalyzer.Windows**  
- **StockAnalyzer.slnx**

## stock-analyzer-task-runner

- **StockAnalyzer.Core**  
- **StockAnalyzer.Web**  
- **StockAnalyzer.Windows**  
- **StockAnalyzer.slnx**

## learn-microsoft-async

- **AsyncCancelAfterPeriodOfTime**  
- **AsyncCancelListOfTasks**  
- **AsyncFileAccess**  
- **AsyncGenerateAndConsumeStreams**  
- **AsyncProcessTasksAsTheyComplete**  
- **AsyncProgrammingScenarios**  
- **AsyncReturnTypes**  
- **Breakfast**  
- **Breakfast.Tests**  
- **DotnetFoundation**  
- **LearnMicrosoft.sln**

## 👤 Author

- **massielcr** - [@massielcr](https://github.com/massielcr)

## 🤝 Support

For support, please open an issue in the repository or contact the maintainer.

## 🙏 Acknowledgments

- .NET Community
- Microsoft Documentation

---

**Last Updated:** April 21, 2026
