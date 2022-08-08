using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example001()
        {
            WriteExampleCaption("Активирующие маркеры (отключение строк, выполняющих JOIN)");

            string boss = "boss";

            string query =
                $"    SELECT TOP(5)                                         --              \r\n" +
                $"         [Main].[ID]              AS [MainID]             --              \r\n" +
                $"        ,[Main].[Caption]         AS [MainCaption]        --              \r\n" +
                $"        ,[Main].[Description]     AS [MainDescription]    --              \r\n" +
                $"        ,[Sub].[ID]               AS [SubID (for BOSS)]   -- $A_{boss}    \r\n" + 
                $"        ,[Sub].[Name]             AS [SubName (for BOSS)] -- $A_{boss}    \r\n" +
                $"    FROM                                                  --              \r\n" +
                $"        [dbo].[TBL] AS [Main]                             --              \r\n" +
                $"                                                          --              \r\n" +
                $"        LEFT JOIN [dbo].[TBLSub] AS [Sub]                 -- $A_{boss}    \r\n" +
                $"        ON [Sub].[TBLID] = [Main].[ID];                   -- $A_{boss}        ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var markers = clientMVGroup.GetMarkersTotal();
            DSQLMarker globalMarkerBoss = markers.FirstOrDefault(m => m.Name == boss);

            // Variant 1
            globalMarkerBoss.Enabled = true;

            WriteQueryVariant(
                caption: "Первый вариант работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql);
            RunQuery(sql, @params);

            // Variant 2
            globalMarkerBoss.Enabled = false;

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