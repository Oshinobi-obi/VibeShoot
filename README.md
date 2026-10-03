# VibeShoot

Photographer scheduling & booking with GCash QR payments, transaction monitoring and printable receipts.
ASP.NET Core MVC (.NET 10) + MySQL (Entity Framework Core / Pomelo).

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
   - imports every photo in `wwwroot/Uploads/Album/<Photographer>/<Category>/` into the `GalleryImages` table,
   - creates the admin account **admin / Admin123!** (change it under *Settings → Change your password*).
3. Open http://localhost:5041 — the admin console is at `/Admin/Login`.

## Features

**Clients**
- Photographer selection, About and Album pages (photos served from the database)
- Booking wizard: availability calendar → package & time slot → details → GCash QR payment (reference no. + receipt screenshot)
- Instant booking statement / receipt — print or download as PDF
- *Track booking* with Transaction ID + mobile number, and pay the remaining balance via GCash

**Admin console**
- **Overview** – collections this month, payments awaiting verification, pending requests, monthly chart
- **Schedule** – month calendar of all sessions; block/unblock days off
- **Bookings** – filter/search, confirm, decline, complete, cancel; record cash payments
- **Transactions** – view GCash proofs, verify or reject payments, export CSV
- **Receipts** – reprint official receipts, print collection reports by date range
- **Gallery** – upload/delete/feature portfolio photos per album
- **Settings** – studio profile, GCash QR upload, packages & prices, admin accounts (Super Admin vs. per-photographer)

## Business rules
- Max 2 bookings per photographer per day, with a 2-hour preparation interval between sessions
- Sessions run within 8:00 AM – 8:00 PM and must be booked at least 1 day ahead
- 50% down payment, price always computed on the server from the package
- A verified down payment automatically confirms the booking
- Packages priced at ₱0 are "custom quote" requests (no online payment)

## Adding photos
Either upload them in **Admin → Gallery**, or copy files into
`wwwroot/Uploads/Album/<MediaFolder>/<Birthday|Baptism|Wedding|Highlights>/` and click **Sync from Uploads folder**
(or just restart the app).

## Schema changes
```
dotnet ef migrations add <Name> --project VibeShoot
```
Migrations are applied automatically on start-up.
