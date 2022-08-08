using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example007()
        {
            WriteExampleCaption("Маркер выражения ORDER BY 2");

            Ln(2, "В данном примере демонстрируется работа 2х маркеров выражения сортировки");

            string 
                order = "order", 
                suborder = "suborder";

            string query =
                $"    SELECT                    --               \r\n" +
                $"         [ID]                 --               \r\n" +
                $"        ,[Caption]            --               \r\n" +
                $"        ,[Description]        --               \r\n" +
                $"        ,(                    --               \r\n" +
                $"            SELECT            --               \r\n" +
                $"                [ID]          --               \r\n" +
                $"            FROM [dbo].[TBL]  --               \r\n" +
                $"                              -- $O_{suborder} \r\n" +
                $"        ) AS [MaxID]          --               \r\n" +
                $"    FROM [dbo].[TBL]          --               \r\n" +
                $"                              -- $O_{order}    \r\n" +
                $"    ;                         --                   ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            OrderingInfo orderInfo = clientMVGroup.OrderingList?.FirstOrDefault(oi => oi.Name == order);
            OrderingInfo suborderInfo = clientMVGroup.OrderingList?.FirstOrDefault(oi => oi.Name == suborder);

            suborderInfo.OrderingFields.Add(new DSQLFieldInfo() { 
                FieldName = "ID", OrderIndex = 0, OrderDESC = true });
            suborderInfo.OnPageCount = 1;
            suborderInfo.Enabled = true;

            orderInfo.OnPageCount = 10;
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