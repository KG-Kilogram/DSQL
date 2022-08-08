using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example012()
        {
            WriteExampleCaption("Контроль блока WHERE");

            string
                id = "id",
                subId = "subId",
                caption = "caption",
                description = "description",
                intvalue = "intvalue",
                floatvalue = "floatvalue",
                int2 = "int2",
                uuid = "uuid";

            string query =
                $"    UPDATE [dbo].[TBL]                    --                      \r\n" +
                $"    SET                                   --                      \r\n" +
                $"         [Caption] = @{caption}           -- $A_{caption}         \r\n" +
                $"        ,[Description] = @{description}   -- $A_{description}     \r\n" +
                $"        ,[IntValue] = @{intvalue}         -- $A_{intvalue}        \r\n" +
                $"        ,[FloatValue] = @{floatvalue}     -- $A_{floatvalue}      \r\n" +
                $"        ,[Int2] = @{int2}                 -- $A_{int2}            \r\n" +
                $"        ,[UUID] = @{uuid}                 -- $A_{uuid}            \r\n" +
                $"    WHERE                                 --                      \r\n" +
                $"        [ID] IN (                         --              $A_IID  \r\n" +
                $"            SELECT COALESCE(ID, @{id})    --              $A_IID  \r\n" +
                $"            WHERE                         --      $A_SUB          \r\n" +
                $"                  @{subId} > 5            -- $EW  $A_SUB          \r\n" +
                $"        )                                 -- $EW          $A_IID  \r\n" +
                $"    ;                                     --                          ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var actions = clientMVGroup.GetActionsTotal();
            SysAction idAction = actions.FirstOrDefault(a => a.DestName == id);
            SysAction subIdAction = actions.FirstOrDefault(a => a.DestName == subId);
            SysAction captionAction = actions.FirstOrDefault(a => a.DestName == caption);
            SysAction descriptionAction = actions.FirstOrDefault(a => a.DestName == description);
            SysAction intValueAction = actions.FirstOrDefault(a => a.DestName == intvalue);
            SysAction floatValueAction = actions.FirstOrDefault(a => a.DestName == floatvalue);
            SysAction int2Action = actions.FirstOrDefault(a => a.DestName == int2);
            SysAction uuidAction = actions.FirstOrDefault(a => a.DestName == uuid);

            // Variant 1
            captionAction.Value.Data = "Update caption";
            descriptionAction.Value.Data = "Update description";
            idAction.Value.Data = 5;

            WriteQueryVariant(
                caption: "Результат работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunNonQuery(sql, @params);

            captionAction.ClearValue();
            descriptionAction.ClearValue();
            idAction.ClearValue();

            // Variant 2
            captionAction.Value.Data = "Update caption";
            descriptionAction.Value.Data = "Update description";
            subIdAction.Value.Data = 5;
            idAction.Value.Data = 5;

            WriteQueryVariant(
                caption: "Результат работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunNonQuery(sql, @params);

            captionAction.ClearValue();
            descriptionAction.ClearValue();
            subIdAction.ClearValue();
            idAction.ClearValue();

            // Variant 3
            intValueAction.Value.Data = 111;
            floatValueAction.Value.Data = 0f;
            subIdAction.Value.Enabled = false;
            idAction.Value.Enabled = false;

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