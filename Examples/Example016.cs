using System;
using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example016()
        {
            WriteExampleCaption("Удаление AND или OR в первой строке после WHERE и скобок без содержания");

            string
                moment = "moment",
                momentOfTime = "momentOfTime",
                order = "order",
                boss = "boss",
                id = "id";

            string query =
                $"    DECLARE @sysVar Datetime2 = @{momentOfTime};                  -- $A_{moment}      \r\n" +
                $"                                                                  --                  \r\n" +
                $"    SELECT                                                        --                  \r\n" +
                $"         [Main].[ID]              AS [MainID]                     --                  \r\n" +
                $"        ,[Main].[Caption]         AS [MainCaption]                --                  \r\n" +
                $"        ,[Main].[Description]     AS [MainDescription]            --                  \r\n" +
                $"        ,[Sub].[ID]               AS [SubID for BOSS]             -- $A_{boss}        \r\n" +
                $"        ,[Sub].[Name]             AS [SubName for BOSS]           -- $A_{boss}        \r\n" +
                $"        ,-1                       AS [SubID for BOSS]             -- $D_{boss}        \r\n" +
                $"        ,''                       AS [SubName for BOSS]           -- $D_{boss}        \r\n" +
                $"        ,[Main].[SysStartTime]    AS [MainSysStartTime]           -- $D_{moment}      \r\n" +
                $"    FROM                                                          --                  \r\n" +
                $"        [dbo].[TBL] FOR SYSTEM_TIME ALL AS [Main]                 -- $D_{moment}      \r\n" +
                $"        [dbo].[TBL] FOR SYSTEM_TIME AS OF @sysVar AS [Main]       -- $A_{moment}      \r\n" +
                $"                                                                  --                  \r\n" +
                $"        LEFT JOIN [dbo].[TBLSub] AS [Sub]                         -- $A_{boss}        \r\n" +
                $"        ON [Sub].[TBLID] = [Main].[ID]                            -- $A_{boss}        \r\n" +
                $"    WHERE                                                         --                  \r\n" +
                $"        [Main].[ID] = @{id}                                       --                  \r\n" +
                $"                                                                  -- $O_{order}       \r\n" +
                $"    ;                                                             --                      ";
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            var clientMVGroup = analyzeResult.GetRootGroupCopy();
            WriteAnalyzeResult(analyzeResult);

            var markers = clientMVGroup.GetMarkersTotal();
            DSQLMarker globalMarkerBoss = markers.FirstOrDefault(m => m.Name == boss);

            var actions = clientMVGroup.GetActionsTotal();
            SysAction momentOfTimeAction = actions.FirstOrDefault(a => a.DestName == momentOfTime);
            SysAction idAction = actions.FirstOrDefault(a => a.DestName == id);

            OrderingInfo orderInfo = clientMVGroup.OrderingList?.FirstOrDefault(oi => oi.Name == order);

            // Variant 1
            idAction.Value.Data = 1;

            orderInfo.Enabled = true;
            orderInfo.SkipCount = 0;
            orderInfo.OnPageCount = 0;
            orderInfo.OrderingFields.Add(new DSQLFieldInfo() { TableAlias = "Main", 
                FieldName = "SysStartTime", OrderIndex = 0, OrderDESC = true });

            globalMarkerBoss.Enabled = false;

            WriteQueryVariant(
                caption: "Вариант работы генератора 1 (!BOSS + !HISTORY)",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);

            // Variant 2
            globalMarkerBoss.Enabled = false;
            momentOfTimeAction.Value.Data = new DateTime(2021, 1, 1, 0, 0, 0);

            WriteQueryVariant(
                caption: "Вариант работы генератора 2 (!BOSS + HISTORY)",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);

            // Variant 3
            globalMarkerBoss.Enabled = true;
            momentOfTimeAction.Value.Data = new DateTime(2023, 1, 1, 0, 0, 0);

            WriteQueryVariant(
                caption: "Вариант работы генератора 3 (BOSS + HISTORY на момент времени ПОСЛЕ изменения Caption)",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);

            // Variant 4
            globalMarkerBoss.Enabled = true;
            momentOfTimeAction.Value.Data = new DateTime(2021, 1, 1, 0, 0, 0);

            WriteQueryVariant(
                caption: "Вариант работы генератора 4 (BOSS + HISTORY на момент времени ДО изменения Caption)",
                info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                clientMVGroup
            );

            (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
            WriteResultingQuery(sql, @params);
            RunQuery(sql, @params);
        }
    }
}