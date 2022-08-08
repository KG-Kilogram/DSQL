using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example010()
        {
            WriteExampleCaption("UPDATE. Выборочное изменение полей с применением мультиплицирующей группы.");

            Ln(2, "Множественная вставка");

            string
                id = "id",
                caption = "caption",
                description = "description",
                intvalue = "intvalue",
                floatvalue = "floatvalue",
                int2 = "int2",
                uuid = "uuid",

                cIds = "cIds";

            string query =
                $"    UPDATE [dbo].[TBL]                    --                  \r\n" +
                $"    SET                                   --                  \r\n" +
                $"         [Caption] = @{caption}           -- $A_{caption}     \r\n" +
                $"        ,[Description] = @{description}   -- $A_{description} \r\n" +
                $"        ,[IntValue] = @{intvalue}         -- $A_{intvalue}    \r\n" +
                $"        ,[FloatValue] = @{floatvalue}     -- $A_{floatvalue}  \r\n" +
                $"        ,[Int2] = @{int2}                 -- $A_{int2}        \r\n" +
                $"        ,[UUID] = @{uuid}                 -- $A_{uuid}        \r\n" +
                $"    WHERE                                 --                  \r\n" +
                $"        [ID] IN (                         --                  \r\n" +
                $"            @{id}                         -- $BV{cIds} $EV    \r\n" +
                $"        )                                 --                  \r\n" +
                $"    ;                                     --                      ";

            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var actions = clientMVGroup.GetActionsTotal();

            SysAction idAction = actions.FirstOrDefault(a => a.DestName == id);
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
            MVGroup idsGroup = clientMVGroup.FindGroup(cIds);

            idAction.PushSubvalue(new int[] { 3, 4, 5, 6 });
            MVGroup.MultiplyGroupForSubvalues(idsGroup, new SysAction[] { idAction });

            intValueAction.Value.Data = 0;
            floatValueAction.Value.Data = 0;

            WriteQueryVariant(
                caption: "Результат работы генератора 2",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunNonQuery(sql, @params);
        }
    }
}