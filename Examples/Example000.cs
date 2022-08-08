using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example000()
        {
            WriteExampleCaption("Активирующие маркеры (отключение возвращаемых полей)");

            string
                caption = "caption",
                description = "description",
                intvalue = "intvalue",
                floatvalue = "floatvalue",
                boss = "boss";

            string query =
                $"    SELECT TOP(5)         --                              \r\n" +
                $"         [ID]             --                              \r\n" +
                $"        ,[Caption]        -- $A_{caption}                 \r\n" +
                $"        ,[Description]    -- $A_{description}             \r\n" +
                $"        ,[IntValue]       -- $A_{intvalue}    $A_{boss}   \r\n" +
                $"        ,[FloatValue]     -- $A_{floatvalue}  $A_{boss}   \r\n" +
                $"    FROM                  --                              \r\n" +
                $"       [dbo].[TBL]        --                              \r\n" +
                $"    ;                     --                                  ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var markers = clientMVGroup.GetMarkersTotal();
            DSQLMarker globalMarkerCaption = markers.FirstOrDefault(m => m.Name == caption);
            DSQLMarker globalMarkerDescription = markers.FirstOrDefault(m => m.Name == description);
            DSQLMarker globalMarkerIntValue = markers.FirstOrDefault(m => m.Name == intvalue);
            DSQLMarker globalMarkerFloatValue = markers.FirstOrDefault(m => m.Name == floatvalue);
            DSQLMarker globalMarkerBoss = markers.FirstOrDefault(m => m.Name == boss);

            // Variant 1
            globalMarkerCaption.Enabled = true;
            globalMarkerDescription.Enabled = true;
            globalMarkerIntValue.Enabled = true;
            globalMarkerFloatValue.Enabled = true;
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
            globalMarkerCaption.Enabled = true;
            globalMarkerDescription.Enabled = false;
            globalMarkerIntValue.Enabled = true;
            globalMarkerFloatValue.Enabled = false;
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