using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example015()
        {
            WriteExampleCaption("Удаление AND или OR в первой строке после WHERE и скобок без содержания");

            string var = "var";

            string query =
                $"    WHERE                                 --                 \r\n" +
                $"       AND [FloatValue] > @{var}          -- $A_MRK          \r\n" +
                $"       (                                  --                 \r\n" +
                $"          [Var] = @ASD                    -- $A_MRK2         \r\n" +
                $"       )                                  --                 \r\n" +
                $"    ;                                     -- $EW             \r\n";

            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var actions = clientMVGroup.GetActionsTotal();
            SysAction varAction = actions.FirstOrDefault(a => a.DestName == var);

            varAction.Value.Data = 1;

            WriteQueryVariant(
                caption: "Вариант работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
        }
    }
}