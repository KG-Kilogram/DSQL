using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example005()
        {
            WriteExampleCaption("Зависимые активирующие маркеры (продолжение)");

            string 
                var = "var", 
                flt = "float";

            string query =
                $"    SELECT TOP(5)                         --              \r\n" +
                $"         [ID]                             --              \r\n" +
                $"        ,[Caption]                        --              \r\n" + 
                $"        ,[Description]                    --              \r\n" + 
                $"    FROM                                  --              \r\n" +
                $"       [dbo].[TBL]                        --              \r\n" +
                $"    WHERE                                 --              \r\n" + 
                $"       [Caption] LIKE '%' + @{var} + '%'  -- $A_MRK       \r\n" + 
                $"       AND [FloatValue] > @{flt}          -- $A_MRK $EW   \r\n" +
                $"    ;                                     --                  ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var actions = clientMVGroup.GetActionsTotal();
            SysAction varAction = actions.FirstOrDefault(a => a.DestName == var);
            SysAction floatAction = actions.FirstOrDefault(a => a.DestName == flt);

            // Variant 1
            varAction.Value.Enabled = false;
            floatAction.Value.Enabled = false;

            WriteQueryVariant(
                caption: "Первый вариант работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);

            // Variant 2
            varAction.Value.Data = "test";
            floatAction.Value.Data = 0.2f;

            WriteQueryVariant(
                caption: "Второй вариант работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);

            // Variant 3
            floatAction.Value.Enabled = false;

            WriteQueryVariant(
                caption: "Третий вариант работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);
        }
    }
}