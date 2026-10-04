-- =============================================================================
-- VibeShoot database (MySQL 8) - STRUCTURE ONLY
-- -----------------------------------------------------------------------------
-- Creates every table used by the public
-- VibeShoot site and the VibeShootAdmin console. No photographers, packages,
-- photos, bookings or admin accounts are inserted.
--
-- The rows written to `__EFMigrationsHistory` are not data: they tell the app
-- this schema is already in place, so it won't try to create the tables again.
--
-- Images (gallery photos, logos, GCash QR codes, payment screenshots) are stored
-- in the `MediaFiles` table and served by both sites at /media/{id}.
--
-- HOW TO IMPORT
--   * Shared hosting / phpMyAdmin (e.g. MonsterASP): click your database in the
--     left panel first, then use the Import tab. The tables go into whichever
--     database is selected - this file does not create or switch databases.
--   * Local MySQL: create an empty database first, then import into it:
--       mysql -u root -p -e "CREATE DATABASE VibeShootStudio CHARACTER SET utf8mb4"
--       mysql -u root -p VibeShootStudio < Database/VibeShootStudio_schema.sql
--
-- Run it on an EMPTY database. If the tables already exist the import fails.
-- =============================================================================

SET NAMES utf8mb4;

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

CREATE TABLE `Reviews` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PhotographerId` int NOT NULL,
    `BookingTransactionId` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    `Rating` int NOT NULL,
    `Tags` varchar(512) CHARACTER SET utf8mb4 NOT NULL,
    `Comment` varchar(600) CHARACTER SET utf8mb4 NULL,
    `DisplayName` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Category` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `IsHidden` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Reviews` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Reviews_Bookings_BookingTransactionId` FOREIGN KEY (`BookingTransactionId`) REFERENCES `Bookings` (`TransactionId`) ON DELETE CASCADE,
    CONSTRAINT `FK_Reviews_Photographers_PhotographerId` FOREIGN KEY (`PhotographerId`) REFERENCES `Photographers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_Reviews_BookingTransactionId` ON `Reviews` (`BookingTransactionId`);

CREATE INDEX `IX_Reviews_PhotographerId_IsHidden_CreatedAt` ON `Reviews` (`PhotographerId`, `IsHidden`, `CreatedAt`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261004020938_AddReviews', '9.0.0');

COMMIT;
