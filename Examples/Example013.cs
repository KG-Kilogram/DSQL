using System;
using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example013()
        {
            WriteExampleCaption("Включение/отключение использования истории");

            string
                moment = "moment",
                momentOfTime = "momentOfTime",
                id = "id";

            string query =
                $"    DECLARE @sysVar Datetime  = @{momentOfTime};          -- $A_{moment}  \r\n" +
                $"                                                          --              \r\n" +
                $"    SELECT                                                --              \r\n" +
                $"         [ID]                                             --              \r\n" +
                $"        ,[Caption]                                        --              \r\n" +
                $"        ,[Description]                                    --              \r\n" +
                $"        ,[IntValue]                                       --              \r\n" +
                $"        ,[SysStartTime]                                   -- $D_{moment}  \r\n" +
                $"    FROM                                                  --              \r\n" +
                $"        [dbo].[TBL] FOR SYSTEM_TIME ALL                   -- $D_{moment}  \r\n" +
                $"        [dbo].[TBL] FOR SYSTEM_TIME AS OF @sysVar         -- $A_{moment}  \r\n" +
                $"    WHERE                                                 --              \r\n" +
                $"        [ID] = @{id}                                      --              \r\n" +
                $"    ;                                                     --                  ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var actions = clientMVGroup.GetActionsTotal();
            SysAction momentOfTimeAction = actions.FirstOrDefault(a => a.DestName == momentOfTime);
            SysAction idAction = actions.FirstOrDefault(a => a.DestName == id);

            idAction.Value.Data = 5;

            // Variant 1
            WriteQueryVariant(
                caption: "Результат работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);

            // Variant 2
            momentOfTimeAction.Value.Data = new DateTime(2021, 1, 1, 0, 0, 0);

            WriteQueryVariant(
                caption: "Результат работы генератора",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);
        }
    }
}