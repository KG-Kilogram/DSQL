using System.Linq;

namespace DSQL
{
    internal partial class Program
    {
        public static void Example011()
        {
            WriteExampleCaption("MV Groups");

            Ln(2, "Вложенные мультиплицирующие группы");

            string
                mainId = "mainId",
                subId = "subId",
                oGroup = "oGroup",
                cGroup = "cGroup";

            string query =
                $"    SELECT                                        --                              \r\n" +
                $"         [Main].[ID]              AS [MainID]     --                              \r\n" +
                $"        ,[Sub].[ID]               AS [SubID]      --                              \r\n" +
                $"    FROM                                          --                              \r\n" +
                $"        [dbo].[TBL] AS [Main]                     --                              \r\n" +
                $"            LEFT JOIN [dbo].[TBLSub] AS [Sub]     --                              \r\n" +
                $"            ON [Main].[ID] = [Sub].[TBLID]        --                              \r\n" +
                $"    WHERE                                         --                              \r\n" +
                $"        ([Main].[ID] = @{mainId}                  -- $BV{oGroup}                  \r\n" + // Begin group1 ---------------|
                $"            AND [Sub].[ID] IN (                   -- $A_UserStreetID              \r\n" + //   |                         |
                $"                @{subId}                          -- $A_UserStreetID $2V{cGroup}  \r\n" + //   -- Begin and end group2   |
                $"            )                                     -- $A_UserStreetID              \r\n" + //                             |
                $"        )                                         -- $EV                          \r\n" + //                             |
                $"    ;                                                                                 ";  // End group1 -----------------|
            WriteQuery(query);

            DSQLAnalyzer analyzeResult = new(query);
            WriteAnalyzeResult(analyzeResult);

            // Variant 1
            {
                var clientMVGroup = analyzeResult.GetRootGroupCopy();

                MVGroup oMVGroup = clientMVGroup.FindGroup(oGroup);
                var actions = clientMVGroup.GetActionsTotal();

                SysAction mainIDAction = actions.FirstOrDefault(a => a.DestName == mainId);
                mainIDAction.Value.Data = 1;

                MVGroup oMVGroupCopy = MVGroup.MultiplyGroup(oMVGroup);
                actions = oMVGroupCopy.GetActionsTotal();

                mainIDAction = actions.FirstOrDefault(a => a.DestName == mainId);
                mainIDAction.Value.Data = 2;

                WriteQueryVariant(
                    caption: "Результат работы генератора",
                    info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                    clientMVGroup
                );

                var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
                WriteResultingQuery(sql, @params);
                RunQuery(sql, @params);
            }

            // Variant 2 (мультиплицирование вложенной группы cMVGroup)
            {
                var clientMVGroup = analyzeResult.GetRootGroupCopy();

                MVGroup oMVGroup = clientMVGroup.FindGroup(oGroup);
                MVGroup cMVGroup = oMVGroup.FindGroup(cGroup);
                var actions = clientMVGroup.GetActionsTotal();

                SysAction mainIDAction = actions.FirstOrDefault(a => a.DestName == mainId);
                mainIDAction.Value.Data = 1;

                SysAction subIDAction = actions.FirstOrDefault(a => a.DestName == subId);
                subIDAction.PushSubvalue(2, 26, 28, 30);

                MVGroup.MultiplyGroupForSubvalues(cMVGroup, new SysAction[] { subIDAction });

                WriteQueryVariant(
                    caption: "Результат работы генератора",
                    info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                    clientMVGroup
                );

                var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
                WriteResultingQuery(sql, @params);
                RunQuery(sql, @params);
            }

            // Variant 3 (мультиплицирование обеих групп)
            {
                var clientMVGroup = analyzeResult.GetRootGroupCopy();

                #region текущее состояние групп и параметров
                /*
                        lClientInitialMVGroup
                            oMVGroup
                                MainIDAction (без значения)
                                Group2
                                    SubIDAction (без значения)    
                */
                #endregion

                MVGroup oMVGroup = clientMVGroup.FindGroup(oGroup);
                MVGroup cMVGroup = oMVGroup.FindGroup(cGroup);
                var oMVGroupActions = oMVGroup.GetActionsTotal();

                SysAction mainIDAction = oMVGroupActions.FirstOrDefault(a => a.DestName == mainId);
                mainIDAction.Value.Data = 1;

                SysAction subIDAction = oMVGroupActions.FirstOrDefault(a => a.DestName == subId);
                subIDAction.PushSubvalue(2, 26, 28, 30);

                MVGroup.MultiplyGroupForSubvalues(cMVGroup, new SysAction[] { subIDAction });

                #region текущее состояние групп и параметров
                /*
                        InitialMVGroup
                            oMVGroup                      
                                MainIDAction = 1
                                cMVGroup                    |
                                    SubIDAction = 2         |
                                cMVGroup                    |
                                    SubIDAction = 26        | <- Это результат работы MultiplyGroupForSubvalues для oMVGroup \ cMVGroup
                                cMVGroup                    |
                                    SubIDAction = 28        |
                                cMVGroup                    |
                                    SubIDAction = 30        |
                */
                #endregion








                // При мультиплицировании group1 мультиплицируется и внутренняя группа group2
                MVGroup oMVGroupCopy = MVGroup.MultiplyGroup(oMVGroup);

                #region текущее состояние групп и параметров
                /*
                        lClientInitialMVGroup
                            oMVGroup                     
                                MainIDAction = 1
                                cMVGroup                    |
                                    SubIDAction = 2         |
                                cMVGroup                    |
                                    SubIDAction = 26        | <- Это результат работы MultiplyGroupForSubvalues для oMVGroup \ cMVGroup
                                cMVGroup                    |
                                    SubIDAction = 28        |
                                cMVGroup                    |
                                    SubIDAction = 30        |
                            oMVGroupCopy                      
                                MainIDAction (без значения)
                                cMVGroup
                                    SubIDAction (без значения)    
                */
                #endregion


                cMVGroup = oMVGroupCopy.FindGroup(cGroup);
                oMVGroupActions = oMVGroupCopy.GetActionsTotal();

                mainIDAction = oMVGroupActions.FirstOrDefault(a => a.DestName == mainId);
                mainIDAction.Value.Data = 2;
                
                subIDAction = oMVGroupActions.FirstOrDefault(a => a.DestName == subId);
                subIDAction.PushSubvalue(32, 38);

                MVGroup.MultiplyGroupForSubvalues(cMVGroup, new SysAction[] { subIDAction });

                #region текущее состояние групп и параметров
                /*
                        lClientInitialMVGroup
                            oMVGroup                      
                                MainIDAction = 1
                                cMVGroup                    |
                                    SubIDAction = 2         |
                                cMVGroup                    |
                                    SubIDAction = 26        | <- Это результат работы MultiplyGroupForSubvalues для oMVGroup / cMVGroup
                                cMVGroup                    |
                                    SubIDAction = 28        |
                                cMVGroup                    |
                                    SubIDAction = 30        |
                            oMVGroupCopy                    
                                MainIDAction = 2
                                cMVGroup                    |
                                    SubIDAction = 2         | <- Это результат работы MultiplyGroupForSubvalues для oMVGroupCopy / cMVGroup
                                cMVGroup                    |
                                    SubIDAction = 26        | 
                */
                #endregion









                MVGroup oMVGroupCopy2 = MVGroup.MultiplyGroup(oMVGroup);

                #region текущее состояние групп и параметров
                /*
                        lClientInitialMVGroup
                            oMVGroup                             
                                MainIDAction = 1
                                cMVGroup                     |
                                    SubIDAction = 2          |
                                cMVGroup                     |
                                    SubIDAction = 26         | <- Это результат работы MultiplyGroupForSubvalues для oMVGroup / cMVGroup
                                cMVGroup                     |
                                    SubIDAction = 28         |
                                cMVGroup                     |
                                    SubIDAction = 30         |
                            oMVGroupCopy                     
                                MainIDAction = 2             
                                cMVGroup                     |
                                    SubIDAction = 2          | <- Это результат работы MultiplyGroupForSubvalues для oMVGroupCopy / cMVGroup
                                cMVGroup                     |
                                    SubIDAction = 26         | 
                            oMVGroupCopy2                         
                                MainIDAction (без значения)       
                                cMVGroup                 
                                    SubIDAction (без значения)    
                */
                #endregion

                cMVGroup = oMVGroupCopy2.FindGroup(cGroup);
                oMVGroupActions = oMVGroupCopy2.GetActionsTotal();

                mainIDAction = oMVGroupActions.FirstOrDefault(a => a.DestName == mainId);
                mainIDAction.Value.Data = 5;

                subIDAction = oMVGroupActions.FirstOrDefault(a => a.DestName == subId);
                subIDAction.Value.Data = 10;

                #region текущее состояние групп и параметров
                /*
                        lClientInitialMVGroup
                            oMVGroup                         
                                MainIDAction = 1
                                cMVGroup                     |
                                    SubIDAction = 2          |
                                cMVGroup                     |
                                    SubIDAction = 26         | <- Это результат работы MultiplyGroupForSubvalues для oMVGroup / cMVGroup
                                cMVGroup                     |
                                    SubIDAction = 28         |
                                cMVGroup                     |
                                    SubIDAction = 30         |
                            oMVGroupCopy                     
                                MainIDAction = 2
                                cMVGroup                     |
                                    SubIDAction = 2          | <- Это результат работы MultiplyGroupForSubvalues для oMVGroupCopy / cMVGroup
                                cMVGroup                     |
                                    SubIDAction = 26         | 
                            oMVGroupCopy2                    
                                MainIDAction = 5       
                                cMVGroup                 
                                    SubIDAction = 10         <- Для одного значения oMVGroupCopy2 / cMVGroup мультиплицирование не требуется
                */
                #endregion

                WriteQueryVariant(
                    caption: "Результат работы генератора",
                    info: "Обратите внимание на состояние маркеров (Enabled / Disabled)",
                    clientMVGroup
                );

                var (sql, @params) = analyzeResult.GetSQLCommandParams(clientMVGroup);
                WriteResultingQuery(sql, @params);
                RunQuery(sql, @params);
            }
        }
    }
}