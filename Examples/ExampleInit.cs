namespace DSQL
{
    internal partial class Program
    {
        private static void ExampleInit()
        {
            WriteExampleCaption("-- Запросы для создания тестовых таблиц");
            Ln(2, "SET ANSI_NULLS ON                                                                                                                             ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "SET QUOTED_IDENTIFIER ON                                                                                                                      ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "ALTER TABLE [dbo].[TBL] SET (SYSTEM_VERSIONING = OFF)                                                                                         ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "ALTER TABLE [dbo].[TBL] DROP PERIOD FOR SYSTEM_TIME                                                                                           ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "DROP TABLE [dbo].[TBLHistory];                                                                                                                ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "DROP TABLE [dbo].[TBLSub];                                                                                                                    ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "DROP TABLE [dbo].[TBL];                                                                                                                       ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "CREATE TABLE [dbo].[TBL] (                                                                                                                    ");
            Ln(2, "    [ID][int] IDENTITY(1, 1) NOT NULL,                                                                                                        ");
            Ln(2, "    [Caption] [nvarchar] (100) NOT NULL,                                                                                                      ");
            Ln(2, "    [Description] [nvarchar] (max)NULL,                                                                                                       ");
            Ln(2, "    [IntValue] [int] NULL,                                                                                                                    ");
            Ln(2, "    [FloatValue] [float] NULL,                                                                                                                ");
            Ln(2, "    [Int2][int] NOT NULL,                                                                                                                     ");
            Ln(2, "    [UUID] [uniqueidentifier] NOT NULL,                                                                                                       ");
            Ln(2, "CONSTRAINT [PK_TBL] PRIMARY KEY CLUSTERED                                                                                                     ");
            Ln(2, "(                                                                                                                                             ");
            Ln(2, "    [ID] ASC                                                                                                                                  ");
            Ln(2, ") WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON,                                             ");
            Ln(2, "             ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]                                                           ");
            Ln(2, ") ON [PRIMARY] TEXTIMAGE_ON[PRIMARY]                                                                                                          ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "ALTER TABLE [dbo].[TBL] ADD CONSTRAINT [DF_TBL_IntValue] DEFAULT((10)) FOR [IntValue]                                                         ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "ALTER TABLE [dbo].[TBL] ADD CONSTRAINT [DF_TBL_Int2] DEFAULT((20)) FOR [Int2]                                                                 ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "ALTER TABLE [dbo].[TBL] ADD                                                                                                                   ");
            Ln(2, "     SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN CONSTRAINT dboTBLSysStart DEFAULT SYSUTCDATETIME()                           ");
            Ln(2, "    ,SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN CONSTRAINT dboTBLSysEnd DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999')");
            Ln(2, "    ,PERIOD FOR SYSTEM_TIME(SysStartTime, SysEndTime)                                                                                         ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "CREATE TABLE [dbo].[TBLSub](                                                                                                                  ");
            Ln(2, "    [ID] [int] IDENTITY(1,1) NOT NULL,                                                                                                        ");
            Ln(2, "    [TBLID] [int] NOT NULL,                                                                                                                   ");
            Ln(2, "    [Name] [nvarchar](100) NOT NULL                                                                                                           ");
            Ln(2, ") ON [PRIMARY]                                                                                                                                ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "ALTER TABLE [dbo].[TBLSub]  WITH CHECK ADD  CONSTRAINT [FK_TBLSub_TBL] FOREIGN KEY([TBLID])                                                   ");
            Ln(2, "REFERENCES [dbo].[TBL] ([ID])                                                                                                                 ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "ALTER TABLE [dbo].[TBLSub] CHECK CONSTRAINT [FK_TBLSub_TBL]                                                                                   ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "-- Для заполнения тестовых таблиц:                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "INSERT INTO [dbo].[TBL](                                                                                                                      ");
            Ln(2, "      [Caption]                                                                                                                               ");
            Ln(2, "     ,[Description]                                                                                                                           ");
            Ln(2, "     ,[IntValue]                                                                                                                              ");
            Ln(2, "     ,[FloatValue]                                                                                                                            ");
            Ln(2, "     ,[Int2]                                                                                                                                  ");
            Ln(2, "     ,[UUID])                                                                                                                                 ");
            Ln(2, "VALUES                                                                                                                                        ");
            Ln(2, "      ('Caption 01', 'Description 01', 1, 0.1, 100000, NEWID())                                                                               ");
            Ln(2, "     ,('Caption 02', 'Description 02', 2, 0.2, 200000, NEWID())                                                                               ");
            Ln(2, "     ,('Caption 03', 'Description 03', 3, 0.3, 300000, NEWID())                                                                               ");
            Ln(2, "     ,('Caption 04', 'Description 04', 4, 0.4, 400000, NEWID())                                                                               ");
            Ln(2, "     ,('Caption 05', 'Description 05', 5, 0.5, 500000, NEWID())                                                                               ");
            Ln(2, "     ,('Caption 06', 'Description 06', 6, 0.6, 600000, NEWID())                                                                               ");
            Ln(2, "     ,('Caption 07', 'Description 07', 7, 0.7, 700000, NEWID())                                                                               ");
            Ln(2, "     ,('Caption 08', 'Description 08', 8, 0.8, 800000, NEWID())                                                                               ");
            Ln(2, "     ,('Caption 09', 'Description 09', 9, 0.9, 900000, NEWID())                                                                               ");
            Ln(2, "     ,('test', 'Description 10', 100, 0.123, 555, NEWID())                                                                                    ");
            Ln(2, "     ,('test test', 'Description 11', 200, 0.456, 666, NEWID())                                                                               ");
            Ln(2, "     ,('test test test', 'Description 12', 300, 0.78, 777, NEWID());                                                                          ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "INSERT INTO [dbo].[TBLSub]([TBLID], [Name])                                                                                                   ");
            Ln(2, "VALUES                                                                                                                                        ");
            Ln(2, "      (1,  'Sub_1'),  (1,  'Sub_1_2'),                                                                                                        ");
            Ln(2, "      (2,  'Sub_2'),  (2,  'Sub_2_2'),                                                                                                        ");
            Ln(2, "      (3,  'Sub_3'),  (3,  'Sub_3_2'),                                                                                                        ");
            Ln(2, "      (4,  'Sub_4'),  (4,  'Sub_4_2'),                                                                                                        ");
            Ln(2, "      (5,  'Sub_5'),  (5,  'Sub_5_2'),                                                                                                        ");
            Ln(2, "      (6,  'Sub_6'),  (6,  'Sub_6_2'),                                                                                                        ");
            Ln(2, "      (7,  'Sub_7'),  (7,  'Sub_7_2'),                                                                                                        ");
            Ln(2, "      (8,  'Sub_8'),  (8,  'Sub_8_2'),                                                                                                        ");
            Ln(2, "      (9,  'Sub_9'),  (9,  'Sub_9_2'),                                                                                                        ");
            Ln(2, "      (10, 'Sub_10'), (10, 'Sub_10_2'),                                                                                                       ");
            Ln(2, "      (11, 'Sub_11'), (11, 'Sub_11_2'),                                                                                                       ");
            Ln(2, "      (12, 'Sub_12'), (12, 'Sub_12_2'),                                                                                                       ");
            Ln(2, "      (1, 'Sub_1_3'), (1, 'Sub_1_4'),                                                                                                         ");
            Ln(2, "      (1, 'Sub_1_5'), (1, 'Sub_1_6'),                                                                                                         ");
            Ln(2, "      (1, 'Sub_1_7'), (1, 'Sub_1_8'),                                                                                                         ");
            Ln(2, "      (1, 'Sub_1_9'),                                                                                                                         ");
            Ln(2, "      (2, 'Sub_2_3'), (2,  'Sub_2_4'),                                                                                                        ");
            Ln(2, "      (2, 'Sub_2_5'), (2,  'Sub_2_6'),                                                                                                        ");
            Ln(2, "      (2, 'Sub_2_7'), (2,  'Sub_2_8'),                                                                                                        ");
            Ln(2, "      (2, 'Sub_2_9')                                                                                                                          ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "ALTER TABLE [dbo].[TBL] DROP PERIOD FOR SYSTEM_TIME;                                                                                          ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "UPDATE [dbo].[TBL] SET [SysStartTime] = '2020-01-01 00:00:00';                                                                                ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "ALTER TABLE [dbo].[TBL] ADD PERIOD FOR SYSTEM_TIME ([SysStartTime], [SysEndTime]);                                                            ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "ALTER TABLE [dbo].[TBL] SET(SYSTEM_VERSIONING = ON(HISTORY_TABLE = [dbo].[TBLHistory]))                                                       ");
            Ln(2, "GO                                                                                                                                            ");
            Ln(2, "                                                                                                                                              ");
            Ln(2, "UPDATE [dbo].[TBL] SET [Caption] = [Caption] + '_upd';                                                                                        ");
            Ln(2, "GO                                                                                                                                            ");
        }
    }
}