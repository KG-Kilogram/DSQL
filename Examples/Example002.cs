using System.Linq;

namespace DSQL
{
    internal partial class Program
    {       
        public static void Example002()
        {
            WriteExampleCaption("Деактивирующие маркеры (отключение строк, выполняющих JOIN)");

            string trainee = "trainee";

            string query =
                $"    SELECT TOP(5)                                         --                  \r\n" +
                $"         [Main].[ID]              AS [MainID]             --                  \r\n" +
                $"        ,[Main].[Caption]         AS [MainCaption]        --                  \r\n" +
                $"        ,[Main].[Description]     AS [MainDescription]    --                  \r\n" +
                $"        ,[Sub].[ID]               AS [SubID for BOSS]     -- $D_{trainee}     \r\n" + 
                $"        ,[Sub].[Name]             AS [SubName for BOSS]   -- $D_{trainee}     \r\n" +
                $"    FROM                                                  --                  \r\n" +
                $"        [dbo].[TBL] AS [Main]                             --                  \r\n" +
                $"                                                          --                  \r\n" +
                $"        LEFT JOIN [dbo].[TBLSub] AS [Sub]                 -- $D_{trainee}     \r\n" +
                $"        ON [Sub].[TBLID] = [Main].[ID];                   -- $D_{trainee}         ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var markers = clientMVGroup.GetMarkersTotal();
            DSQLMarker globalMarkerTrainee = markers.FirstOrDefault(m => m.Name == trainee);

            // Variant 1
            globalMarkerTrainee.Enabled = true;

            WriteQueryVariant(
                caption: "Первый вариант работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql);
            RunQuery(sql, @params);

            // Variant 2
            globalMarkerTrainee.Enabled = false;

            WriteQueryVariant(
                caption: "Второй вариант работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql);
            RunQuery(sql, @params);
        }
    }
}