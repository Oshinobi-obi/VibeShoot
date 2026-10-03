-- =============================================================================
-- VibeShoot database (MySQL 8)
-- -----------------------------------------------------------------------------
-- Creates every table used by the public
-- VibeShoot site and the VibeShootAdmin console, plus the starting data:
--   * 3 photographers (profiles, social links, logo / GCash QR paths)
--   * 15 service packages
--   * 79 gallery photos (file paths under VibeShoot/wwwroot/Uploads/Album)
--   * 1 admin account:  admin / Admin123!   (change it after signing in)
--
-- Bookings, Payments and BlockedDates start empty.
-- Images live in the `MediaFiles` table. The rows below still point at the photo
-- files bundled in VibeShoot/wwwroot/Uploads; the first time the VibeShoot site
-- starts, it copies those files into `MediaFiles` and updates the paths.
--
-- HOW TO IMPORT
--   * Shared hosting / phpMyAdmin (e.g. MonsterASP): click your database in the
--     left panel first, then use the Import tab. The tables go into whichever
--     database is selected - this file does not create or switch databases.
--   * Local MySQL: create an empty database first, then import into it:
--       mysql -u root -p -e "CREATE DATABASE VibeShootStudio CHARACTER SET utf8mb4"
--       mysql -u root -p VibeShootStudio < Database/VibeShootStudio.sql
--
-- Run it on an EMPTY database. If the tables already exist the import fails.
-- =============================================================================

SET NAMES utf8mb4;

-- -----------------------------------------------------------------------------
-- Schema (generated from the EF Core migrations, so the app sees them as applied)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

CREATE TABLE `Photographers` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Slug` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `Tagline` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
    `Bio` longtext CHARACTER SET utf8mb4 NOT NULL,
    `MediaFolder` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `LogoPath` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
    `GCashQrPath` varchar(512) CHARACTER SET utf8mb4 NULL,
    `GCashAccountName` varchar(128) CHARACTER SET utf8mb4 NULL,
    `GCashNumber` varchar(32) CHARACTER SET utf8mb4 NULL,
    `FacebookUrl` varchar(512) CHARACTER SET utf8mb4 NULL,
    `InstagramUrl` varchar(512) CHARACTER SET utf8mb4 NULL,
    `TikTokUrl` varchar(512) CHARACTER SET utf8mb4 NULL,
    `XUrl` varchar(512) CHARACTER SET utf8mb4 NULL,
    `SortOrder` int NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Photographers` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Admins` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Username` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Role` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `PhotographerId` int NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Admins` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Admins_Photographers_PhotographerId` FOREIGN KEY (`PhotographerId`) REFERENCES `Photographers` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE `BlockedDates` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PhotographerId` int NOT NULL,
    `Date` datetime(6) NOT NULL,
    `Reason` varchar(256) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_BlockedDates` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_BlockedDates_Photographers_PhotographerId` FOREIGN KEY (`PhotographerId`) REFERENCES `Photographers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `GalleryImages` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PhotographerId` int NOT NULL,
    `Category` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `FilePath` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
    `Caption` varchar(256) CHARACTER SET utf8mb4 NULL,
    `FileSizeBytes` bigint NOT NULL,
    `SortOrder` int NOT NULL,
    `IsFeatured` tinyint(1) NOT NULL,
    `UploadedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_GalleryImages` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_GalleryImages_Photographers_PhotographerId` FOREIGN KEY (`PhotographerId`) REFERENCES `Photographers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Packages` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PhotographerId` int NOT NULL,
    `Category` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `Price` decimal(12,2) NOT NULL,
    `DurationHours` int NOT NULL,
    `Inclusions` longtext CHARACTER SET utf8mb4 NOT NULL,
    `SortOrder` int NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Packages` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Packages_Photographers_PhotographerId` FOREIGN KEY (`PhotographerId`) REFERENCES `Photographers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Bookings` (
    `TransactionId` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `AccessToken` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `PhotographerId` int NOT NULL,
    `PackageId` int NULL,
    `ClientName` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `ContactNumber` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Email` varchar(128) CHARACTER SET utf8mb4 NULL,
    `SocialLink` varchar(512) CHARACTER SET utf8mb4 NULL,
    `TargetDate` datetime(6) NOT NULL,
    `StartTime` time(6) NOT NULL,
    `EndTime` time(6) NOT NULL,
    `Venue` varchar(256) CHARACTER SET utf8mb4 NOT NULL,
    `Category` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `PackageName` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
    `TotalPrice` decimal(12,2) NOT NULL,
    `DownPaymentRequired` decimal(12,2) NOT NULL,
    `Notes` longtext CHARACTER SET utf8mb4 NULL,
    `AdminRemarks` longtext CHARACTER SET utf8mb4 NULL,
    `Status` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_Bookings` PRIMARY KEY (`TransactionId`),
    CONSTRAINT `FK_Bookings_Packages_PackageId` FOREIGN KEY (`PackageId`) REFERENCES `Packages` (`Id`) ON DELETE SET NULL,
    CONSTRAINT `FK_Bookings_Photographers_PhotographerId` FOREIGN KEY (`PhotographerId`) REFERENCES `Photographers` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `Payments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `BookingTransactionId` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `ReceiptNumber` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Amount` decimal(12,2) NOT NULL,
    `Method` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Type` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `ReferenceNumber` varchar(64) CHARACTER SET utf8mb4 NULL,
    `ProofImagePath` varchar(512) CHARACTER SET utf8mb4 NULL,
    `Status` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Remarks` longtext CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `VerifiedAt` datetime(6) NULL,
    `VerifiedBy` varchar(64) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Payments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Payments_Bookings_BookingTransactionId` FOREIGN KEY (`BookingTransactionId`) REFERENCES `Bookings` (`TransactionId`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Admins_PhotographerId` ON `Admins` (`PhotographerId`);

CREATE UNIQUE INDEX `IX_Admins_Username` ON `Admins` (`Username`);

CREATE UNIQUE INDEX `IX_BlockedDates_PhotographerId_Date` ON `BlockedDates` (`PhotographerId`, `Date`);

CREATE INDEX `IX_Bookings_PackageId` ON `Bookings` (`PackageId`);

CREATE INDEX `IX_Bookings_PhotographerId_TargetDate` ON `Bookings` (`PhotographerId`, `TargetDate`);

CREATE INDEX `IX_Bookings_Status` ON `Bookings` (`Status`);

CREATE UNIQUE INDEX `IX_GalleryImages_FilePath` ON `GalleryImages` (`FilePath`);

CREATE INDEX `IX_GalleryImages_PhotographerId_Category` ON `GalleryImages` (`PhotographerId`, `Category`);

CREATE INDEX `IX_Packages_PhotographerId` ON `Packages` (`PhotographerId`);

CREATE INDEX `IX_Payments_BookingTransactionId` ON `Payments` (`BookingTransactionId`);

CREATE UNIQUE INDEX `IX_Payments_ReceiptNumber` ON `Payments` (`ReceiptNumber`);

CREATE INDEX `IX_Payments_ReferenceNumber` ON `Payments` (`ReferenceNumber`);

CREATE INDEX `IX_Payments_Status` ON `Payments` (`Status`);

CREATE UNIQUE INDEX `IX_Photographers_Slug` ON `Photographers` (`Slug`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261003071255_InitialVibeShootSchema', '9.0.0');

CREATE TABLE `MediaFiles` (
    `Id` char(36) COLLATE ascii_general_ci NOT NULL,
    `FileName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ContentType` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Data` longblob NOT NULL,
    `SizeBytes` bigint NOT NULL,
    `Kind` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `SourcePath` varchar(512) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_MediaFiles` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_MediaFiles_SourcePath` ON `MediaFiles` (`SourcePath`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261003120735_AddMediaFiles', '9.0.0');

ALTER TABLE `Packages` ADD `DiscountEnd` datetime(6) NULL;

ALTER TABLE `Packages` ADD `DiscountLabel` varchar(64) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Packages` ADD `DiscountStart` datetime(6) NULL;

ALTER TABLE `Packages` ADD `DiscountType` varchar(16) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Packages` ADD `DiscountValue` decimal(12,2) NOT NULL DEFAULT 0.0;

ALTER TABLE `Bookings` ADD `DiscountAmount` decimal(12,2) NOT NULL DEFAULT 0.0;

ALTER TABLE `Bookings` ADD `DiscountLabel` varchar(64) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Bookings` ADD `OriginalPrice` decimal(12,2) NOT NULL DEFAULT 0.0;

UPDATE `Bookings` SET `OriginalPrice` = `TotalPrice`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261003133926_AddPackageDiscounts', '9.0.0');

COMMIT;

-- -----------------------------------------------------------------------------
-- Starting data
-- -----------------------------------------------------------------------------
SET FOREIGN_KEY_CHECKS = 0;
START TRANSACTION;

INSERT INTO `Photographers` (`Id`, `Slug`, `Name`, `Tagline`, `Bio`, `MediaFolder`, `LogoPath`, `GCashQrPath`, `GCashAccountName`, `GCashNumber`, `FacebookUrl`, `InstagramUrl`, `TikTokUrl`, `XUrl`, `SortOrder`, `IsActive`, `CreatedAt`) VALUES (1,'sulyap-films','Sulyap Films','Cinematic films and photographs for life\'s quiet glances.','Sulyap Films captures the fleeting glances that make every celebration yours — candid, cinematic and full of feeling.','SulyapFilms','/Uploads/Logos/SulyapFilms/SulyapFilms.png',NULL,NULL,NULL,'https://www.facebook.com/share/1DbuUDU2iv/','https://www.instagram.com/sulyapfilms?igsh=a3FuamY0Zzk2eDlz',NULL,NULL,1,1,'2026-10-03 07:13:16.572101');
INSERT INTO `Photographers` (`Id`, `Slug`, `Name`, `Tagline`, `Bio`, `MediaFolder`, `LogoPath`, `GCashQrPath`, `GCashAccountName`, `GCashNumber`, `FacebookUrl`, `InstagramUrl`, `TikTokUrl`, `XUrl`, `SortOrder`, `IsActive`, `CreatedAt`) VALUES (2,'ginger-snaps','Ginger Snaps Photography','Genuine moments, rich color, timeless stories.','Hi! I\'m Mcrey Ronquillo, a photographer based in Quezon City and a graduate of Bachelor of Science in Information Technology from Quezon City University. With a keen eye for detail and a passion for visual storytelling, I enjoy experimenting with new approaches to photography, especially through color grading. I specialize in capturing genuine moments at birthdays, baptisms, debuts, and coffee shops, while also pursuing my love for street photography, where I find beauty in everyday life and authentic human moments. My goal is to create timeless images that preserve emotions and telling narratives through visuals.','GingerSnaps','/Uploads/Logos/GingerSnaps/GingerSnaps.png','/Uploads/QRCodes/GingerSnaps/GSGCash.png',NULL,NULL,'https://www.facebook.com/profile.php?id=61554942684841',NULL,'https://www.tiktok.com/@gngrsnpsptgrpy?_r=1',NULL,2,1,'2026-10-03 07:13:16.572655');
INSERT INTO `Photographers` (`Id`, `Slug`, `Name`, `Tagline`, `Bio`, `MediaFolder`, `LogoPath`, `GCashQrPath`, `GCashAccountName`, `GCashNumber`, `FacebookUrl`, `InstagramUrl`, `TikTokUrl`, `XUrl`, `SortOrder`, `IsActive`, `CreatedAt`) VALUES (3,'chiyos-folder','Chiyos Folder','Portraits, places, cats and coffee — told with an artist\'s eye.','Hi, I\'m Jerick Ybarreta, but most people know me as Chiyo especially my artist friends. I\'m a creative individual with a passion for photography, graphic design, and a little bit of drawing. I love working on projects that allow me to express my artistic side while exploring different subjects and ideas. I enjoy taking portraits, capturing the beauty of places, and focusing on unique subjects like cats and coffee shops. Whether I\'m behind the camera or designing something new, I always aim to create work that feels authentic and meaningful. My goal is to bring ideas to life and connect with people through my art.','ChiyosFolder','/Uploads/Logos/ChiyosFolder/ChiyosFolder.png',NULL,NULL,NULL,'https://www.facebook.com/share/186kdJyk7R/','https://www.instagram.com/Definitely_not_jerick','https://www.tiktok.com/@matchiiyoooo_?_r=1&_t=ZS-98kzNSnNaxY','https://x.com/JerickYbarreta',3,1,'2026-10-03 07:13:16.572701');
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (1,1,'Birthday','Custom Package',0.00,3,'Package details coming soon!\nSubmit a request and we will send you a quote.',1,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (2,1,'Baptism','Custom Package',0.00,3,'Package details coming soon!\nSubmit a request and we will send you a quote.',1,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (3,1,'Photoshoot','Custom Package',0.00,3,'Package details coming soon!\nSubmit a request and we will send you a quote.',1,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (4,1,'Wedding','Custom Package',0.00,5,'Package details coming soon!\nSubmit a request and we will send you a quote.',1,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (5,2,'Birthday','Basic Package',2999.00,3,'1 Photographer\n1 Assistant\n2-3hrs. Photo Coverage\nUnlimited Shots\n150 minimum Photos\n7-9 Days Editing Process',1,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (6,2,'Birthday','Combo Package',6499.00,3,'1 Photographer, 1 Videographer, 1 Assistant\n2-3hrs. Photo & Video Coverage\nUnlimited Shots\n3-5 min. Video Highlights\n150 minimum Photos\n7-9 Days Editing Process',2,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (7,2,'Baptism','Basic Package',2999.00,3,'1 Photographer\n1 Assistant\n2-3hrs. Photo Coverage\nUnlimited Shots\n150 minimum Photos\n7-9 Days Editing Process',1,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (8,2,'Baptism','Combo Package',6499.00,3,'1 Photographer, 1 Videographer, 1 Assistant\n2-3hrs. Photo & Video Coverage\nUnlimited Shots\n3-5 min. Video Highlights\n150 minimum Photos\n7-9 Days Editing Process',2,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (9,2,'Photoshoot','Classic Package',3499.00,2,'1 Photographer\n1 Assistant\n1 Location\n2hrs. Photo Session\nUnlimited Shots\n50 minimum Composed Edited Photos\n1-2 Weeks Editing Process',1,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (10,2,'Photoshoot','Deluxe Package',7499.00,2,'1 Photographer, 1 Videographer, 1 Assistant\n1 Location\n2hrs. Photo & Video Session\nUnlimited Shots\n2-4 min. Video Shoot\n50 minimum Composed Edited Photos\n1-2 Weeks Editing Process',2,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (11,2,'Wedding','Wedding Package',0.00,5,'Packages for Weddings are currently custom tailored.\nSubmit a request and we will contact you for a quote!',1,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (12,3,'Birthday','Custom Package',0.00,3,'Package details coming soon!\nSubmit a request and we will send you a quote.',1,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (13,3,'Baptism','Custom Package',0.00,3,'Package details coming soon!\nSubmit a request and we will send you a quote.',1,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (14,3,'Photoshoot','Custom Package',0.00,3,'Package details coming soon!\nSubmit a request and we will send you a quote.',1,1);
INSERT INTO `Packages` (`Id`, `PhotographerId`, `Category`, `Name`, `Price`, `DurationHours`, `Inclusions`, `SortOrder`, `IsActive`) VALUES (15,3,'Wedding','Custom Package',0.00,5,'Package details coming soon!\nSubmit a request and we will send you a quote.',1,1);
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (1,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS1Bday.jpg',NULL,101718,1,0,'2026-10-03 07:13:17.491660');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (2,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS2Bday.jpg',NULL,174552,2,0,'2026-10-03 07:13:17.512455');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (3,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS3Bday.jpg',NULL,249719,3,0,'2026-10-03 07:13:17.512717');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (4,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS4Bday.jpg',NULL,147376,4,0,'2026-10-03 07:13:17.512810');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (5,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS5Bday.jpg',NULL,127703,5,0,'2026-10-03 07:13:17.512885');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (6,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS6Bday.jpg',NULL,216284,6,0,'2026-10-03 07:13:17.512961');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (7,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS7Bday.jpg',NULL,277778,7,0,'2026-10-03 07:13:17.513025');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (8,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS8Bday.jpg',NULL,205149,8,0,'2026-10-03 07:13:17.513093');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (9,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS9Bday.jpg',NULL,201881,9,0,'2026-10-03 07:13:17.513160');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (10,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS10Bday.jpg',NULL,155357,10,0,'2026-10-03 07:13:17.513228');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (11,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS11Bday.jpg',NULL,173447,11,0,'2026-10-03 07:13:17.513293');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (12,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS12Bday.jpg',NULL,260310,12,0,'2026-10-03 07:13:17.513355');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (13,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS13Bday.jpg',NULL,261459,13,0,'2026-10-03 07:13:17.513415');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (14,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS14Bday.jpg',NULL,285023,14,0,'2026-10-03 07:13:17.513476');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (15,2,'Birthday','/Uploads/Album/GingerSnaps/Birthday/GS15Bday.jpg',NULL,193760,15,0,'2026-10-03 07:13:17.513536');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (16,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS1Baptism.jpg',NULL,136988,1,0,'2026-10-03 07:13:17.515181');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (17,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS2Baptism.jpg',NULL,193891,2,0,'2026-10-03 07:13:17.515388');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (18,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS3Baptism.jpg',NULL,287607,3,0,'2026-10-03 07:13:17.515476');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (19,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS4Baptism.jpg',NULL,240997,4,0,'2026-10-03 07:13:17.515550');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (20,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS5Baptism.jpg',NULL,356375,5,0,'2026-10-03 07:13:17.515616');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (21,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS6Baptism.jpg',NULL,343686,6,0,'2026-10-03 07:13:17.515684');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (22,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS7Baptism.jpg',NULL,172750,7,0,'2026-10-03 07:13:17.515756');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (23,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS8Baptism.jpg',NULL,139917,8,0,'2026-10-03 07:13:17.515823');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (24,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS9Baptism.jpg',NULL,335030,9,0,'2026-10-03 07:13:17.515925');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (25,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS10Baptism.jpg',NULL,230284,10,0,'2026-10-03 07:13:17.516004');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (26,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS11Baptism.jpg',NULL,227405,11,0,'2026-10-03 07:13:17.516074');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (27,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS12Baptism.jpg',NULL,183393,12,0,'2026-10-03 07:13:17.516153');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (28,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS13Baptism.jpg',NULL,163903,13,0,'2026-10-03 07:13:17.516224');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (29,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS14Baptism.jpg',NULL,176056,14,0,'2026-10-03 07:13:17.516292');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (30,2,'Baptism','/Uploads/Album/GingerSnaps/Baptism/GS15Baptism.jpg',NULL,314054,15,0,'2026-10-03 07:13:17.516359');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (31,2,'Wedding','/Uploads/Album/GingerSnaps/Wedding/GS1Wedding.jpg',NULL,159594,1,0,'2026-10-03 07:13:17.517847');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (32,2,'Wedding','/Uploads/Album/GingerSnaps/Wedding/GS2Wedding.jpg',NULL,439972,2,0,'2026-10-03 07:13:17.518067');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (33,2,'Wedding','/Uploads/Album/GingerSnaps/Wedding/GS3Wedding.jpg',NULL,96121,3,0,'2026-10-03 07:13:17.518153');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (34,2,'Wedding','/Uploads/Album/GingerSnaps/Wedding/GS4Wedding.jpg',NULL,96850,4,0,'2026-10-03 07:13:17.518231');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (35,2,'Wedding','/Uploads/Album/GingerSnaps/Wedding/GS5Wedding.jpg',NULL,224719,5,0,'2026-10-03 07:13:17.518307');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (36,2,'Wedding','/Uploads/Album/GingerSnaps/Wedding/GS6Wedding.jpg',NULL,133214,6,0,'2026-10-03 07:13:17.518378');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (37,2,'Wedding','/Uploads/Album/GingerSnaps/Wedding/GS7Wedding.jpg',NULL,192049,7,0,'2026-10-03 07:13:17.518448');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (38,2,'Wedding','/Uploads/Album/GingerSnaps/Wedding/GS8Wedding.jpg',NULL,161862,8,0,'2026-10-03 07:13:17.518534');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (39,2,'Wedding','/Uploads/Album/GingerSnaps/Wedding/GS9Wedding.jpg',NULL,165411,9,0,'2026-10-03 07:13:17.518614');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (40,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS1Highlights.jpg',NULL,172052,1,1,'2026-10-03 07:13:17.520981');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (41,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS2Highlights.jpg',NULL,207584,2,1,'2026-10-03 07:13:17.521287');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (42,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS3Highlights.jpg',NULL,135601,3,1,'2026-10-03 07:13:17.521412');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (43,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS4Highlights.jpg',NULL,176827,4,0,'2026-10-03 07:13:17.521518');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (44,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS5Highlights.jpg',NULL,133692,5,0,'2026-10-03 07:13:17.521606');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (45,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS6Highlights.jpg',NULL,180919,6,0,'2026-10-03 07:13:17.521717');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (46,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS7Highlights.jpg',NULL,140603,7,0,'2026-10-03 07:13:17.521815');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (47,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS8Highlights.jpg',NULL,125535,8,0,'2026-10-03 07:13:17.521910');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (48,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS9Highlights.jpg',NULL,175426,9,0,'2026-10-03 07:13:17.522005');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (49,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS10Highlights.jpg',NULL,160283,10,0,'2026-10-03 07:13:17.522100');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (50,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS11Highlights.jpg',NULL,285786,11,0,'2026-10-03 07:13:17.522204');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (51,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS12Highlights.jpg',NULL,110130,12,0,'2026-10-03 07:13:17.522314');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (52,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS13Highlights.jpg',NULL,141553,13,0,'2026-10-03 07:13:17.522449');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (53,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS14Highlights.jpg',NULL,167101,14,0,'2026-10-03 07:13:17.522550');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (54,2,'Highlights','/Uploads/Album/GingerSnaps/Highlights/GS15Highlights.jpg',NULL,133992,15,0,'2026-10-03 07:13:17.522646');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (55,3,'Birthday','/Uploads/Album/ChiyosFolder/Birthday/CF1Bday.jpg',NULL,307582,1,0,'2026-10-03 07:13:17.524200');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (56,3,'Birthday','/Uploads/Album/ChiyosFolder/Birthday/CF2Bday.jpg',NULL,261902,2,0,'2026-10-03 07:13:17.524434');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (57,3,'Birthday','/Uploads/Album/ChiyosFolder/Birthday/CF3Bday.jpg',NULL,324171,3,0,'2026-10-03 07:13:17.524531');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (58,3,'Birthday','/Uploads/Album/ChiyosFolder/Birthday/CF4Bday.jpg',NULL,352419,4,0,'2026-10-03 07:13:17.524609');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (59,3,'Birthday','/Uploads/Album/ChiyosFolder/Birthday/CF5Bday.jpg',NULL,413064,5,0,'2026-10-03 07:13:17.524684');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (60,3,'Baptism','/Uploads/Album/ChiyosFolder/Baptism/CF1Baptism.jpg',NULL,248172,1,0,'2026-10-03 07:13:17.526332');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (61,3,'Baptism','/Uploads/Album/ChiyosFolder/Baptism/CF2Baptism.jpg',NULL,266360,2,0,'2026-10-03 07:13:17.526607');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (62,3,'Baptism','/Uploads/Album/ChiyosFolder/Baptism/CF3Baptism.jpg',NULL,287612,3,0,'2026-10-03 07:13:17.526701');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (63,3,'Baptism','/Uploads/Album/ChiyosFolder/Baptism/CF4Baptism.jpg',NULL,354104,4,0,'2026-10-03 07:13:17.526785');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (64,3,'Baptism','/Uploads/Album/ChiyosFolder/Baptism/CF5Baptism.jpg',NULL,390782,5,0,'2026-10-03 07:13:17.526865');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (65,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF1Highlights.jpg',NULL,348615,1,1,'2026-10-03 07:13:17.529458');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (66,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF2Highlights.jpg',NULL,292060,2,1,'2026-10-03 07:13:17.529685');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (67,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF3Highlights.jpg',NULL,336452,3,1,'2026-10-03 07:13:17.529782');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (68,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF4Highlights.jpg',NULL,354116,4,0,'2026-10-03 07:13:17.529864');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (69,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF5Highlights.jpg',NULL,236281,5,0,'2026-10-03 07:13:17.529942');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (70,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF6Highlights.jpg',NULL,179976,6,0,'2026-10-03 07:13:17.530019');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (71,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF7Highlights.jpg',NULL,178134,7,0,'2026-10-03 07:13:17.530097');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (72,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF8Highlights.jpg',NULL,246831,8,0,'2026-10-03 07:13:17.530174');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (73,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF9Highlights.jpg',NULL,192725,9,0,'2026-10-03 07:13:17.530256');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (74,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF10Highlights.jpg',NULL,175207,10,0,'2026-10-03 07:13:17.530333');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (75,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF11Highlights.jpg',NULL,154375,11,0,'2026-10-03 07:13:17.530410');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (76,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF12Highlights.jpg',NULL,128292,12,0,'2026-10-03 07:13:17.530488');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (77,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF13Highlights.jpg',NULL,128165,13,0,'2026-10-03 07:13:17.530565');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (78,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF14Highlights.jpg',NULL,204601,14,0,'2026-10-03 07:13:17.530643');
INSERT INTO `GalleryImages` (`Id`, `PhotographerId`, `Category`, `FilePath`, `Caption`, `FileSizeBytes`, `SortOrder`, `IsFeatured`, `UploadedAt`) VALUES (79,3,'Highlights','/Uploads/Album/ChiyosFolder/Highlights/CF15Highlights.jpg',NULL,288905,15,0,'2026-10-03 07:13:17.530719');

INSERT INTO `Admins` (`Username`, `PasswordHash`, `Role`, `PhotographerId`, `CreatedAt`) VALUES
('admin', 'AQAAAAIAAYagAAAAEP3vy9Q62k58CbGcnDxF031lf8ozp2PkhMVYv9F+AaDevOzbO3ahUIoEdaTc8bPt7g==', 'SuperAdmin', NULL, UTC_TIMESTAMP(6));

COMMIT;
SET FOREIGN_KEY_CHECKS = 1;
