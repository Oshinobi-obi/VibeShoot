# VibeShoot

Public booking site: photographer scheduling with GCash QR payments and printable receipts.
ASP.NET Core MVC (.NET 10) + MySQL (Entity Framework Core / Pomelo).

The admin console lives in a separate project, **[VibeShootAdmin](https://github.com/Oshinobi-obi/VibeShootAdmin)**.
Both apps use the same `VibeShootStudio` database. Every image (gallery photos, logos, GCash QR codes, payment
screenshots) is stored in the database's `MediaFiles` table and served at `/media/{id}`, so the two apps can be
hosted separately.
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
   - copies every photo in `wwwroot/Uploads/Album/<Photographer>/<Category>/` (plus the logos and QR codes) into the
     database. This happens once; it may take a little while on the first start.
3. Open http://localhost:5041. For the admin console, run VibeShootAdmin (http://localhost:5018).

### Setting up the database by hand (optional)
`Database/VibeShootStudio.sql` creates all the tables and the starting data
(photographers, packages, gallery photos and the `admin` / `Admin123!` account). The file doesn't create a database;
it fills whichever empty database you import it into:
- **phpMyAdmin / shared hosting (e.g. MonsterASP):** select your database in the left panel, then *Import*.
- **Local MySQL:**
  ```
  mysql -u root -p -e "CREATE DATABASE VibeShootStudio CHARACTER SET utf8mb4"
  mysql -u root -p VibeShootStudio < Database/VibeShootStudio.sql
  ```

The app recognises the imported schema and won't recreate it. Point `ConnectionStrings:DefaultConnection` in both apps
at that database (on hosting, use the server, database name, user and password from your hosting panel).

`Database/VibeShootStudio_schema.sql` is the same structure with no data. On first start, VibeShoot fills in the
photographers, packages and gallery photos, and VibeShootAdmin creates the `admin` account.

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
Upload them in **VibeShootAdmin → Gallery**. Large photos are resized in the browser before upload.
(Files dropped into `wwwroot/Uploads/Album/<MediaFolder>/<Category>/` are also copied into the database the next
time this app starts.)

## Schema changes
```
dotnet ef migrations add <Name> --project VibeShoot
```
Migrations are applied automatically on start-up. The entity classes are copied in VibeShootAdmin
(`Models/Entities`, `Data/ApplicationDbContext.cs`), so apply the same change there.
