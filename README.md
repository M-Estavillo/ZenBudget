# ZenBudget 🪙

ZenBudget is an Android expense and budget tracker built using **C# .NET 10**, **ASP.NET Core**, and **PostgreSQL**. It offers a minimalist user experience designed for fast visual scanning, personal budgeting control, and advanced currency flexibility.

---

## 🚀 Key Features

*   **📊 Dynamic Weekly & Monthly Analytics**:
    *   **Weekly Spending Graph**: Focuses entirely on the current day's progress with clear daily highlights.
    *   **12-Month Trend Horizontal View**: An overflow-scrolling yearly layout that locks tracking to the current calendar year (January to December).
*   **💱 Global Multi-Currency Support**:
    *   Supports dynamic runtime currency conversions across **PHP**, **USD**, **CNY**, **JPY**, and **KRW** which are currently using fixed mathematical translation.
    *   Forces absolute backend database integrity by standardizing all underlying storage calculations to a single base currency (PHP), then gracefully formats display values and parses inputs in the user's preferred layout.
*   **🏷️ Smart Category Icons & Budgets**:
    *   Native Android vector icons map automatically to system standard categories (Food, Transport, Groceries, Housing, Health, and Entertainment).
    *   Smart fallback automatically resolves to a generic icon (`ic_default_category.xml`) for user-created custom additions.
    *   Color-coded progress visualizers map budget remaining thresholds dynamically.
*   **🛠️ Full CRUD Transaction Management**:
    *   Fully synchronized dialog interfaces for Adding, Editing, and Deleting transactions with instant backend API and local chart updates.
*   **⏱️ Recently Added Ordering**:
    *   Sorts dashboard recent feeds descending by insertion time (`CreatedAt`) so new inputs are never buried under future-dated expenses, while preserving absolute chronological ledger order on the main transactions list.

---

## 🛠️ Technology Stack

*   **Mobile Frontend**: Native Android built on **.NET 10 (C#)**, targeting Android SDK 36, utilizing modern vector XML resources.
*   **Backend API**: **ASP.NET Core Web API** using the latest C# features.
*   **Database**: Relational **PostgreSQL** cloud-hosted on **Neon Tech** serverless clusters.
*   **ORM**: **Entity Framework Core (EF Core)** using code-first migrations.
*   **Deployment Ready**: Fully configured with a root `Dockerfile` targeting .NET 10 runtimes for instant containerized cloud deployment (Render, Railway, etc.).

---

## 🖥️ Backend API Setup & Local Running

If you wish to host the API locally on your computer:

### 1. Database Connection Configuration
Since private settings containing live database passwords are excluded from Git for security, follow these steps to configure your local setup:

1. Locate the template file `ZenBudget.Api/appsettings.Development.json.example`.
2. Duplicate or rename the file to `ZenBudget.Api/appsettings.Development.json` in the same directory.
3. Open `appsettings.Development.json` and replace the placeholder fields with your live PostgreSQL connection string and secure JWT signing keys.


### 2. Apply EF Core Migrations
To build the database schema locally, run:
```bash
dotnet ef database update --project ZenBudget.Api
```

### 3. Run the API Server
Start the local ASP.NET Core server:
```bash
dotnet run --project ZenBudget.Api
```
The API will run locally, and you can point your mobile client's `ApiService.cs` base URL to your machine's local network IP or emulator routing address (`http://10.0.2.2:5000/api`).

---

## 🔒 Security Notice & Clean Repository Layout
The project is configured with a comprehensive [.gitignore](.gitignore) that prevents leaking personal local data:
*   Excludes local compilation outputs (`bin/`, `obj/`).
*   Excludes private developer IDE assets (`.idea/`, `.vs/`, `.rider/`).
*   Protects secure configuration settings (`appsettings.Development.json`, `secrets.json`) containing active cloud credentials from being pushed to public source control.
