using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example014()
        {
            WriteExampleCaption("Контроль блока WHERE");

            string
                subId = "subId",
                caption = "caption",
                description = "description";

            string query =
                $"    UPDATE [dbo].[TBL]                    --                  \r\n" +
                $"    SET                                   --                  \r\n" +
                $"         [Caption] = @{caption}           -- $A_{caption}     \r\n" +
                $"        ,[Description] = @{description}   -- $A_{description} \r\n" +
                $"    WHERE                                 --                  \r\n" +
                $"        [ID] > 5                          -- $UC              \r\n" +
                $"        OR [ID] = @{subId}                -- $A_SUB           \r\n" +
                $"    ;                                     -- $EW                  ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var actions = clientMVGroup.GetActionsTotal();

            SysAction subIdAction = actions.FirstOrDefault(a => a.DestName == subId);
            SysAction captionAction = actions.FirstOrDefault(a => a.DestName == caption);
            SysAction descriptionAction = actions.FirstOrDefault(a => a.DestName == description);

            // Variant 1
            captionAction.Value.Data = "Update caption";
            descriptionAction.Value.Data = "Update description";
            subIdAction.Value.Data = 10;

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
            subIdAction.ClearValue();

            // Variant 2
            captionAction.Value.Data = "Update caption";
            descriptionAction.Value.Data = "Update description";

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
        }
    }
}