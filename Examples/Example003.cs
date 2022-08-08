using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example003()
        {
            WriteExampleCaption("Связка активирующих и деактивирующих маркеров");

            Ln();
            Ln(2, "// В данном примере использована связка активирующей и деактивирующей функции,");
            Ln(2, "// что следует толковать следующим образом:");
            Ln();
            Ln(2, "//    > При использовании связки маркеров разработчик может управлять только активирующим;");
            Ln(2, "//    > Если $A_BOSS активировать, то строки с $A_BOSS будут активны в результирующем запросе,");
            Ln(2, "//          а строки с $D_BOSS - не активны;");
            Ln(2, "//    > Если $A_BOSS деактивировать, то строки с $A_BOSS будут не активны в результирующем запросе,");
            Ln(2, "//          а строки с $D_BOSS - активны;");
            Ln();
            Ln(2, "//    Обратите внимание, что в данном примере количество возвращаемых полей постоянно.");

            string boss = "boss";

            string query =
                $"    SELECT TOP(5)                                         --              \r\n" +
                $"         [Main].[ID]              AS [MainID]             --              \r\n" +
                $"        ,[Main].[Caption]         AS [MainCaption]        --              \r\n" +
                $"        ,[Main].[Description]     AS [MainDescription]    --              \r\n" +
                $"        ,[Sub].[ID]               AS [SubID for BOSS]     -- $A_{boss}    \r\n" + // Global activate marker BOSS
                $"        ,[Sub].[Name]             AS [SubName for BOSS]   -- $A_{boss}    \r\n" +
                $"        ,-1                       AS [SubID for BOSS]     -- $D_{boss}    \r\n" + // Global activate marker BOSS
                $"        ,''                       AS [SubName for BOSS]   -- $D_{boss}    \r\n" +
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