# Shipment Desk API

Personal sample of an ASP.NET Core Web API. It is written from scratch for a public GitHub profile. It is not copied from any employer codebase and it is not a product.

## What it shows

- ASP.NET Core on .NET 9, controllers, and request validation
- A forward-only shipment milestone rule, covered by unit tests
- EF Core with SQLite by default, and a SQL Server provider switch
- API key check on shipment endpoints, health check left open
- GitHub Actions build and test

## Run

```bash
dotnet test
dotnet run --project src/ShipmentDesk.Api
```

Open `http://localhost:5075/swagger`.

Send header `X-Api-Key: demo-key-change-me` on `/api/shipments`. Change that value in `src/ShipmentDesk.Api/appsettings.json` before you publish the repository if you do not want the demo key in source.

`GET /health` does not need the key.

## Calls

Create a shipment:

```http
POST /api/shipments
Content-Type: application/json
X-Api-Key: demo-key-change-me

{
  "reference": "SD1001",
  "origin": "BOM",
  "destination": "FRA",
  "shipper": "Northwind Traders"
}
```

Record the next milestone. Status can move only one step: Booked, Accepted, Departed, Arrived, Delivered.

```http
POST /api/shipments/{id}/milestones
Content-Type: application/json
X-Api-Key: demo-key-change-me

{
  "status": "Accepted",
  "location": "BOM",
  "note": "Accepted at origin"
}
```

## SQL Server

Set `Database:Provider` to `SqlServer` and point `ConnectionStrings:Default` at your SQL Server instance. The API calls `EnsureCreated` on startup so a local demo does not need a migration project.

## Publish

Create an empty public repository on your personal GitHub account, then from this folder:

```bash
git init
git add .
git commit -m "Add a sample shipment milestone API."
git branch -M main
git remote add origin https://github.com/<your-user>/shipment-desk-api.git
git push -u origin main
```

Use that repository URL as your GitHub link on applications.
