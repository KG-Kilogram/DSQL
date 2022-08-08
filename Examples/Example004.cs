using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example004()
        {
            WriteExampleCaption("Зависимые активирующие маркеры");

            Ln(2, "В данном примере демонстрируется работа зависимого активирующего маркера");
            Ln();
            Ln(2, "    > Анализатор идентифицирует маркер зависимым, если в строке с маркером встречается SQL перемнная (@Var);");
            Ln(2, "    > Управлять активностью зависимого маркера можно только путем передачи перемнной.");
            Ln();
            Ln(2, "    Обратите внимание на маркер $EW, который определяет окончание блока WHERE используется");
            Ln(2, "при деактивации блока (WHERE) целиком. Маркер $EW будет подробно рассмотрен в одном следующих примеров.");

            string var = "var";

            string query =
                $"    SELECT TOP(5)                             --              \r\n" +
                $"         [ID]                                 --              \r\n" +
                $"        ,[Caption]                            --              \r\n" + 
                $"        ,[Description]                        --              \r\n" + 
                $"    FROM                                      --              \r\n" +
                $"       [dbo].[TBL]                            --              \r\n" +
                $"    WHERE                                     --              \r\n" + 
                $"       [Caption] LIKE '%' + @{var} + '%'  -- $A_NAME $EW  \r\n" + 
                $"    ;                                         --                  ";

            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var actions = clientMVGroup.GetActionsTotal();
            SysAction varAction = actions.FirstOrDefault(a => a.DestName == var);
            
            // Variant 1
            varAction.Value.Enabled = false;

            WriteQueryVariant(
                caption: "Первый вариант работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);

            // Variant 2
            varAction.Value.Data = "05";

            WriteQueryVariant(
                caption: "Второй вариант работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);
        }
    }
}