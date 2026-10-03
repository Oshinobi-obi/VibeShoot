# VibeShoot

Public booking site: photographer scheduling with GCash QR payments and printable receipts.
ASP.NET Core MVC (.NET 10) + MySQL (Entity Framework Core / Pomelo).

The admin console lives in a separate project, **[VibeShootAdmin](https://github.com/Oshinobi-obi/VibeShootAdmin)**.
Both apps use the same `VibeShootStudio` database and the same `VibeShoot/wwwroot/Uploads` folder.
This project owns the database schema (it runs the migrations), so start it at least once before the admin console.

## Getting started

1. Make sure MySQL 8 is running locally and the connection string in `VibeShoot/appsettings.json` has your credentials.
   The database name is `VibeShootStudio` — you don't need to create it.
2. Run the app:
   ```
   dotnet run --project VibeShoot --launch-profile http
   ```
   On first start the app automatically:
   - creates the database and all tables (EF Core migrations),
   - adds the three photographers and their packages,
   - imports every photo in `wwwroot/Uploads/Album/<Photographer>/<Category>/` into the `GalleryImages` table.
3. Open http://localhost:5041. For the admin console, run VibeShootAdmin (http://localhost:5018).
   The faint π link in the corner points to `AdminSiteUrl` in `appsettings.json`.

## Features

**Clients**
- Photographer selection, About and Album pages (photos served from the database)
- Booking wizard: availability calendar → package & time slot → details → GCash QR payment (reference no. + receipt screenshot)
- Instant booking statement / receipt — print or download as PDF
- *Track booking* with Transaction ID + mobile number, and pay the remaining balance via GCash

## Business rules
- Max 2 bookings per photographer per day, with a 2-hour preparation interval between sessions
- Sessions run within 8:00 AM – 8:00 PM and must be booked at least 1 day ahead
- 50% down payment, price always computed on the server from the package
- A verified down payment automatically confirms the booking
- Packages priced at ₱0 are "custom quote" requests (no online payment)

## Adding photos
Either upload them in **VibeShootAdmin → Gallery**, or copy files into
`wwwroot/Uploads/Album/<MediaFolder>/<Birthday|Baptism|Wedding|Highlights>/` and restart this app
(or click **Sync from Uploads folder** in the admin Gallery).

## Schema changes
```
dotnet ef migrations add <Name> --project VibeShoot
```
Migrations are applied automatically on start-up. The entity classes are copied in VibeShootAdmin
(`Models/Entities`, `Data/ApplicationDbContext.cs`), so apply the same change there.
