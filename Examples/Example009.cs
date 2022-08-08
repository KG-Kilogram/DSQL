using System;
using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example009()
        {
            WriteExampleCaption("INSERT. Формирование запроса исходя из передаваемых переменных 2");

            Ln(2, "Множественная вставка");

            string
                caption = "caption",
                description = "description",
                intvalue = "intvalue",
                floatvalue = "floatvalue",
                int2 = "int2",
                uuid = "uuid", 
                
                cIns = "cIns";

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
                $"    (                     -- $BV{cIns}        \r\n" + 
                $"         @{caption}       --                  \r\n" +
                $"        ,@{description}   -- $A_{description} \r\n" +
                $"        ,@{intvalue}      -- $A_{intvalue}    \r\n" +
                $"        ,@{floatvalue}    -- $A_{floatvalue}  \r\n" +      
                $"        ,@{int2}          --                  \r\n" +
                $"        ,@{uuid}          --                  \r\n" +
                $"    )                     -- $EV                  ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var actions = clientMVGroup.GetActionsTotal();
            SysAction captionAction = actions.FirstOrDefault(a => a.DestName == caption);
            SysAction descriptionAction = actions.FirstOrDefault(a => a.DestName == description);
            SysAction int2Action = actions.FirstOrDefault(a => a.DestName == int2);
            SysAction uuidAction = actions.FirstOrDefault(a => a.DestName == uuid);

            for (int i = 0; i < 3; i++)
            {
                // Not null values
                captionAction.PushSubvalue(string.Format("Multi insert caption {0}", i));
                int2Action.PushSubvalue(i * 1000);
                uuidAction.PushSubvalue(Guid.NewGuid());

                // Nullable values
                descriptionAction.PushSubvalue(string.Format("Multi insert description {0}", i));
            }

            MVGroup insGroup = clientMVGroup.FindGroup(cIns);

            while (MVGroup.MultiplyGroupForSubvalues(insGroup, new SysAction[] { 
                captionAction, int2Action, uuidAction, descriptionAction }))
            {
                WriteQueryVariant(
                    caption: "Результат работы генератора",
                    info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                    clientMVGroup
                );

                var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
                WriteResultingQuery(sql, @params);
                RunNonQuery(sql, @params);
            }
        }
    }
}