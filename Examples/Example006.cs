using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example006()
        {
            WriteExampleCaption("Маркер выражения ORDER BY");

            Ln(2, "В данном примере демонстрируется работа маркера выражения сортировки");

            string order = "order";

            string query =
                $"    SELECT            --              \r\n" +
                $"         [ID]         --              \r\n" +
                $"        ,[Caption]    --              \r\n" +
                $"    FROM              --              \r\n" +
                $"       [dbo].[TBL]    --              \r\n" +
                $"                      -- $O_{order}   \r\n" +
                $"    ;                 --                  ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            OrderingInfo orderInfo = clientMVGroup.OrderingList?.FirstOrDefault(oi => oi.Name == order);

            orderInfo.OnPageCount = 7;
            orderInfo.Enabled = true;

            WriteQueryVariant(
                caption: "Результат работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);
        }
    }
}