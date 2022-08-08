using System;
using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example008()
        {
            WriteExampleCaption("INSERT. Формирование запроса исходя из передаваемых переменных");

            Ln(2, "Все Nullable поля отмечены активирующими маркерами");

            string
                caption = "caption",
                description = "description",
                intvalue = "intvalue",
                floatvalue = "floatvalue",
                int2 = "int2",
                uuid = "uuid";

            string query =
                $"    INSERT                --                  \r\n" +
                $"    INTO [dbo].[TBL](     --                  \r\n" +
                $"         [Caption]        --                  \r\n" +
                $"        ,[Description]    -- $A_{description} \r\n" +
                $"        ,[IntValue]       -- $A_{intvalue}    \r\n" +
                $"        ,[FloatValue]     -- $A_{floatvalue}  \r\n" +
                $"        ,[Int2]           --                  \r\n" +
                $"        ,[UUID]           --                  \r\n" +
                $"    )                     --                  \r\n" +
                $"    VALUES                --                  \r\n" +
                $"    (                     --                  \r\n" +
                $"         @{caption}       --                  \r\n" +
                $"        ,@{description}   -- $A_{description} \r\n" +
                $"        ,@{intvalue}      -- $A_{intvalue}    \r\n" +
                $"        ,@{floatvalue}    -- $A_{floatvalue}  \r\n" +
                $"        ,@{int2}          --                  \r\n" +
                $"        ,@{uuid}          --                  \r\n" +
                $"    )                     --                      ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var actions = clientMVGroup.GetActionsTotal();

            SysAction captionAction = actions.FirstOrDefault(a => a.DestName == caption);
            SysAction descriptionAction = actions.FirstOrDefault(a => a.DestName == description);
            SysAction intValueAction = actions.FirstOrDefault(a => a.DestName == intvalue);
            SysAction floatValueAction = actions.FirstOrDefault(a => a.DestName == floatvalue);
            SysAction int2Action = actions.FirstOrDefault(a => a.DestName == int2);
            SysAction uuidAction = actions.FirstOrDefault(a => a.DestName == uuid);

            // Not null values
            captionAction.Value.Data = "test caption";
            int2Action.Value.Data = "123";
            uuidAction.Value.Data = Guid.NewGuid();

            // Nullable values
            descriptionAction.Value.Data = "test description";

            WriteQueryVariant(
                caption: "Результат работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunNonQuery(sql, @params);

            // Дополнение
            descriptionAction.Value.Enabled = false;
            intValueAction.Value.Data = 333;
            floatValueAction.Value.Data = 0.25f;

            WriteQueryVariant(
                caption: "Результат работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunNonQuery(sql, @params);
        }
    }
}