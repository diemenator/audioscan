USE [master]
GO
CREATE DATABASE [Test]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Test', FILENAME = N'/var/opt/mssql/data/Test_Primary.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB ), 
 FILEGROUP [storage] 
( NAME = N'storage_57A4268A', FILENAME = N'/var/opt/mssql/data/Test_storage_57A4268A.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Test_log', FILENAME = N'/var/opt/mssql/data/Test_Primary.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [Test] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Test].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Test] SET ANSI_NULL_DEFAULT ON 
GO
ALTER DATABASE [Test] SET ANSI_NULLS ON 
GO
ALTER DATABASE [Test] SET ANSI_PADDING ON 
GO
ALTER DATABASE [Test] SET ANSI_WARNINGS ON 
GO
ALTER DATABASE [Test] SET ARITHABORT ON 
GO
ALTER DATABASE [Test] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Test] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Test] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Test] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Test] SET CURSOR_DEFAULT  LOCAL 
GO
ALTER DATABASE [Test] SET CONCAT_NULL_YIELDS_NULL ON 
GO
ALTER DATABASE [Test] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Test] SET QUOTED_IDENTIFIER ON 
GO
ALTER DATABASE [Test] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Test] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Test] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Test] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Test] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Test] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Test] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Test] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Test] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Test] SET RECOVERY FULL 
GO
ALTER DATABASE [Test] SET  MULTI_USER 
GO
ALTER DATABASE [Test] SET PAGE_VERIFY NONE  
GO
ALTER DATABASE [Test] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Test] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Test] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [Test] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'Test', N'ON'
GO
ALTER DATABASE [Test] SET QUERY_STORE = OFF
GO 
CREATE LOGIN [User] WITH PASSWORD=N'changeit', DEFAULT_DATABASE=[Test], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO
ALTER SERVER ROLE [sysadmin] ADD MEMBER [User]
GO
USE [Test]
GO
GRANT VIEW ANY COLUMN ENCRYPTION KEY DEFINITION TO [public] AS [dbo]
GO
GRANT VIEW ANY COLUMN MASTER KEY DEFINITION TO [public] AS [dbo]
GO

USE [Test]
GO
/****** Object:  Table [dbo].[audioTrack]    Script Date: 15.08.2020 3:08:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[audioTrack](
	[Id] [uniqueidentifier] NOT NULL,
	[FileName] [nvarchar](1024) NOT NULL,
	[Path] [nvarchar](max) NOT NULL,
	[DirectoryName] [nvarchar](1024) NOT NULL,
	[DirectoryPath] [nvarchar](max) NOT NULL,
	[Genre] [nvarchar](1024) NULL,
	[Album] [nvarchar](1024) NULL,
	[Artist] [nvarchar](1024) NULL,
	[Title] [nvarchar](1024) NULL,
	[Track] [int] NOT NULL,
	[LengthMillis] [bigint] NOT NULL,
	[Md5] [binary](16) NOT NULL,
	[PathMd5] [binary](16) NOT NULL,
	[FirstSeen] [datetime] NOT NULL,
	[LastSeen] [datetime] NOT NULL,
	[Rating] [int] NOT NULL,
	[PlayCount] [int] NOT NULL,
	[SkipCount] [int] NOT NULL,
	[LastPlayed] [datetime] NULL,
 CONSTRAINT [PK_audioTrack] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[audioTrack] ADD  CONSTRAINT [DF_audioTrack_Id]  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[audioTrack] ADD  CONSTRAINT [DF_audioTrack_Track]  DEFAULT ((0)) FOR [Track]
GO
ALTER TABLE [dbo].[audioTrack] ADD  CONSTRAINT [DF_audioTrack_LengthMillis]  DEFAULT ((0)) FOR [LengthMillis]
GO
ALTER TABLE [dbo].[audioTrack] ADD  CONSTRAINT [DF_audioTrack_FirstSeen]  DEFAULT (getutcdate()) FOR [FirstSeen]
GO
ALTER TABLE [dbo].[audioTrack] ADD  CONSTRAINT [DF_audioTrack_LastSeen]  DEFAULT (getutcdate()) FOR [LastSeen]
GO
ALTER TABLE [dbo].[audioTrack] ADD  CONSTRAINT [DF_audioTrack_Rating]  DEFAULT ((0)) FOR [Rating]
GO
ALTER TABLE [dbo].[audioTrack] ADD  CONSTRAINT [DF_audioTrack_PlayCount]  DEFAULT ((0)) FOR [PlayCount]
GO
ALTER TABLE [dbo].[audioTrack] ADD  CONSTRAINT [DF_audioTrack_SkipCount]  DEFAULT ((0)) FOR [SkipCount]
GO
/****** Object:  StoredProcedure [dbo].[get_dirinfo]    Script Date: 15.08.2020 3:08:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[get_dirinfo] 
	@directoryPath NVARCHAR(MAX) null	
AS
BEGIN
	BEGIN TRY
		 
		 declare @lastSeen datetime = null
		 declare @len int
		 
		 select @len =( LEN (@directoryPath + '#') - 1)

		 select @lastSeen = t.LastSeen from dbo.audioTrack t		 
		 where (@directoryPath is null ) or ( (SUBSTRING(t.DirectoryPath, 0, @len)) = @directoryPath)

		 select @lastSeen as [LastSeen]		  
	END TRY

	BEGIN CATCH
		SELECT ERROR_NUMBER() AS ErrorNumber
			,ERROR_SEVERITY() AS ErrorSeverity
			,ERROR_STATE() AS ErrorState
			,ERROR_PROCEDURE() AS ErrorProcedure
			,ERROR_LINE() AS ErrorLine
			,ERROR_MESSAGE() AS ErrorMessage;

	--	IF @@TRANCOUNT > 0
			--ROLLBACK TRANSACTION;
	END CATCH;

	--IF @@TRANCOUNT > 0
		--COMMIT TRANSACTION;
END
GO
/****** Object:  StoredProcedure [dbo].[update_track]    Script Date: 15.08.2020 3:08:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[update_track] @id UNIQUEIDENTIFIER = NULL
	,@fileName NVARCHAR(1024)
	,@path NVARCHAR(MAX)
	,@directoryPath NVARCHAR(MAX)
	,@directoryName NVARCHAR(1024)
	,@album NVARCHAR(1024)
	,@genre NVARCHAR(1024)
	,@playCount INT
	,@skipCount INT
	,@artist NVARCHAR(1024)
	,@title NVARCHAR(1024)
	,@track INT = 0
	,@LengthMillis INT = 0
	,@md5 BINARY (16)
	,@pathMd5 BINARY (16)
	,@rating INT
	,@lastPlayed DATETIME NULL
AS
BEGIN
	BEGIN TRY

		DECLARE @fid UNIQUEIDENTIFIER = NULL
		DECLARE @emptyid UNIQUEIDENTIFIER = NULL

		SELECT @emptyid = cast('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER)

		SELECT @fid = Id
		FROM [dbo].[audioTrack] a
		WHERE a.PathMd5 = @pathMd5
			AND a.[Path] = @path
			AND (
				(@id IS NULL)
				OR (@id = @emptyid)
				OR (id = @id)
				)

		IF (@fid IS NOT NULL)
		BEGIN
			UPDATE [dbo].[audioTrack]
			SET [Album] = @album
				,[Genre] = @genre
				,[Artist] = @artist
				,[Title] = @title
				,[Track] = @track
				,[LengthMillis] = @LengthMillis
				,[Md5] = @md5
				,[LastSeen] = (GETUTCDATE())
				,[Rating] = @rating
				,[PlayCount] = @playCount
				,[SkipCount] = @skipCount
				,[LastPlayed] = @lastPlayed
			WHERE Id = @fid
		END
		ELSE
		BEGIN
			SET @fid = NEWID()

			INSERT INTO [dbo].[audioTrack] (
				Id
				,[FileName]
				,[Path]
				,[DirectoryName]
				,[DirectoryPath]				
				,[Genre]
				,[Album]
				,[Artist]
				,[Title]
				,[Track]
				,[LengthMillis]
				,[Md5]
				,[PathMd5]
				,[Rating]
				,[PlayCount]
				,[SkipCount]
				,[LastPlayed]
				)
			VALUES (
				@fid
				,@fileName
				,@path
				,@directoryName
				,@directoryPath
				,@genre
				,@album
				,@artist
				,@title
				,@track
				,@LengthMillis
				,@md5
				,@pathMd5
				,@rating
				,@playCount
				,@skipCount
				,@lastPlayed
				)
		END

		SELECT TOP 1 [Id]
			,[FileName]
			,[Path]
			,[DirectoryName]
			,[DirectoryPath]
			,[Genre]
			,[Album]
			,[Artist]
			,[Title]
			,[Track]
			,[LengthMillis]
			,[Md5]
			,[PathMd5]
			,[FirstSeen]
			,[LastSeen]
			,[Rating]
			,[PlayCount]
			,[SkipCount]
			,lastPlayed
		FROM [dbo].[audioTrack]
		WHERE id = @fid
	END TRY

	BEGIN CATCH
		SELECT ERROR_NUMBER() AS ErrorNumber
			,ERROR_SEVERITY() AS ErrorSeverity
			,ERROR_STATE() AS ErrorState
			,ERROR_PROCEDURE() AS ErrorProcedure
			,ERROR_LINE() AS ErrorLine
			,ERROR_MESSAGE() AS ErrorMessage;
	END CATCH;

END
GO

ALTER DATABASE [Test] SET  READ_WRITE 
GO
