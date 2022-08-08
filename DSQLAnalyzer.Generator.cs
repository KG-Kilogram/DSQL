using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSQL
{
    /*
        Отношение классов

        • DSQLAnalyzer (Generator)          <==
            - MVGroup
                - Line
                    - Word
                - SysAction
                    - ActionValue           
                - DSQLMarker
                - OrderingInfo
                    - DSQLFieldInfo

        • SyntaxAnalyzer (static class)
        • Hash (static class)     
    */

    public partial class DSQLAnalyzer                                                                                                           
    {
        /// <summary>
        ///     Метод получения запроса и списка кортежей (имя sql-параметра, значение sql-параметра) для 
        ///     передачи этих значений в экземпляр SqlCommand
        /// </summary>
        /// <param name="sourceMVGroup">Корневая группа дерева, изначально полученного при копировании дерева-шаблона
        /// (результата работы анализатора) с внесенными изменениями (значения Actions, состояния маркеров) и
        /// мультиплицированием вложенных групп</param>
        /// <returns></returns>
        public (string query, List<(string name, SysAction value)>) GetSQLCommandParams(MVGroup sourceMVGroup)
        {
            /*
                Расчет Enabled для маркеров на основании дерева групп с учетом мультипликации 
                групп и явных/неявных зависимостей маркеров от переменных запроса, а также с учетом
                безусловных маркеров                
            */
            MVGroup.CalcMarkersEnabled(sourceMVGroup);


            /*
                Подготовка списка групп-шаблонов, полученных при работе анализатора запроса 
            */
            var groups = InitialMVGroup.GetSubgroupsTotal();
            groups.Add(InitialMVGroup);


            /*
                     
            */
            var generatedList = sourceMVGroup.GetQuery(groups);


            /*
                Комментирование (если нужно) блоков WHERE, которые не имеют активных строк
            */
            ProcessWhereBlock(generatedList);


            /*
                Обработка следующих ситуаций:
                    1. Удаление запятой, OR или AND после SELECT, WHERE, SET;
                    2. Удаление открывающией и закрывающей скобок, идущих подряд.
            */
            ProcessGroupOperator(generatedList);


            /*
                Развертывание блоков сортировки ORDER BY (обработка маркеров $O_)
            */
            ProcessOrderByMarkers(sourceMVGroup, generatedList);


            StringBuilder code = new StringBuilder();
            foreach (var (lineSql, _) in generatedList.Where(ln => ln.SQLLine.Trim() != string.Empty))
                code.Append($"{lineSql}\r\n");


            /*
                Подготовка данных для параметров запроса и имен с учетом ультиплицированных значений
                MV - Multiple Value
            */
            List<SysAction> actions = sourceMVGroup.GetActionsTotal();
            List<(string name, SysAction value)> outParams = new List<(string name, SysAction value)>();

            foreach (SysAction a in actions.Where(action => action.Value.Enabled))
                if (a.MVIndex == 0)
                    outParams.Add((a.DestName, a));
                else
                    outParams.Add((string.Format("{0}_MV{1}", a.DestName, a.MVIndex), a));

            return (code.ToString().TrimEnd(), outParams);
        }

        internal static int StartSpaceCount(string str)
        {
            int cnt = 0;
            foreach (var c in str)
                if (c == ' ')
                    cnt++;
                else
                    break;

            return cnt;
        }

        private static void ProcessOrderByMarkers(MVGroup clientMVGroup, List<(string sqlLine, Line lineLink)> generatedList)
        {
            List<string> processedOrders = new List<string>();
            List<string> orderList = new List<string>();
            List<int> spaceCntList = new List<int>();

            for (int i = 0; i < generatedList.Count - 1; i++)
            {
                Line line = generatedList[i].lineLink;

                if (line.LineData.Trim().ToLower().StartsWith("from"))
                    spaceCntList.Add(StartSpaceCount(line.LineData));

                if (line.OrderingMarker != null && line.OrderingMarker.Trim() != string.Empty)
                {
                    if (!processedOrders.Contains(line.OrderingMarker))
                    {
                        string orderSql = string.Empty;

                        foreach (var ord in clientMVGroup.OrderingList?.Where(o => o.Enabled))
                            if (ord != null && ord.Name == line.OrderingMarker)
                            {
                                long resultingSkipCount;

                                if (ord.Reload)
                                    resultingSkipCount = ord.SkipCount;
                                else
                                    resultingSkipCount = ord.SkipCount + ord.Loaded;

                                int resultingLoadCount = 0;

                                int pages = 0;
                                if (ord.OnPageCount > 0)
                                {
                                    pages = ord.Loaded / ord.OnPageCount;
                                    resultingLoadCount = ord.Loaded % ord.OnPageCount;
                                }

                                if (ord.Reload)
                                    resultingLoadCount += (pages == 0 ? 1 : pages) * ord.OnPageCount;
                                else
                                    resultingLoadCount = ord.NumberOfAdded != 0 ? ord.NumberOfAdded : ord.OnPageCount;

                                string prefixSpaces = string.Empty;

                                if (spaceCntList.Count > 0)
                                {
                                    prefixSpaces = new string(' ', spaceCntList[spaceCntList.Count - 1]);
                                    spaceCntList.RemoveAt(spaceCntList.Count - 1);
                                }

                                if (ord.UseInitialSort)
                                {
                                    orderSql = prefixSpaces + "ORDER BY 1";

                                    if (resultingSkipCount > 0 || resultingLoadCount > 0)
                                    {
                                        orderSql += string.Format("\r\n{0}OFFSET {1} ROWS", prefixSpaces, resultingSkipCount);

                                        if (resultingLoadCount > 0)
                                            orderSql += string.Format("\r\n{0}FETCH NEXT {1} ROWS ONLY", prefixSpaces, resultingLoadCount);
                                    }
                                }
                                else
                                {
                                    orderSql = string.Empty;

                                    ord.OrderingFields.Sort((DSQLFieldInfo f1, DSQLFieldInfo f2) => Math.Sign(f1.OrderIndex - f2.OrderIndex));

                                    foreach (var f in ord.OrderingFields)
                                        if (f.OrderIndex != -1)
                                        {
                                            if (f.TableAlias != string.Empty)
                                                orderSql += string.Format("{0}[{1}].[{2}] {3}",
                                                    orderSql == string.Empty ? string.Empty : ", ", f.TableAlias, f.FieldName, f.OrderDESC ? "DESC" : "");
                                            else
                                                orderSql += string.Format("{0}[{1}] {2}",
                                                    orderSql == string.Empty ? string.Empty : ", ", f.FieldName, f.OrderDESC ? "DESC" : "");
                                        }

                                    orderSql = string.Format("{0}ORDER BY {1}", prefixSpaces, orderSql == string.Empty ? "1" : orderSql);

                                    if (resultingSkipCount > 0 || resultingLoadCount > 0)
                                    {
                                        orderSql += string.Format("\r\n{0}OFFSET {1} ROWS", prefixSpaces, resultingSkipCount);

                                        if (resultingLoadCount > 0)
                                            orderSql += string.Format("\r\n{0}FETCH NEXT {1} ROWS ONLY", prefixSpaces, resultingLoadCount);
                                    }
                                }
                            }

                        processedOrders.Add(line.OrderingMarker);

                        generatedList[i] = (orderSql, generatedList[i].lineLink);
                    }
                }
            }
        }

        private static void ProcessGroupOperator(List<(string sqlLine, Line lineLink)> generatedList)
        {
            string lastStr = string.Empty;
            int lastLineIdx = -1;
            for (int i = 0; i < generatedList.Count; i++)
            {
                var (sqlLine, lineLink) = generatedList[i];

                string workStr = sqlLine.Trim().ToLower();

                if (lastStr.EndsWith("(") || lastStr.EndsWith("select") || 
                    lastStr.EndsWith("where") || lastStr.EndsWith("set") ||
                    lastStr.EndsWith(" on") || lastStr.EndsWith("values"))
                {
                    if (workStr.StartsWith(",") || workStr.StartsWith("or ") || workStr.StartsWith("and "))
                    {
                        int spaceCount = StartSpaceCount(sqlLine);

                        if (workStr.StartsWith(","))
                            workStr = new string(' ', spaceCount) + sqlLine.Trim().Substring(1).Trim();
                        else if (workStr.StartsWith("or "))
                            workStr = new string(' ', spaceCount) + sqlLine.Trim().Substring(3).Trim();
                        else if (workStr.StartsWith("and "))
                            workStr = new string(' ', spaceCount) + sqlLine.Trim().Substring(4).Trim();

                        generatedList[i] = (workStr, lineLink);
                    }
                    else if (lastStr.EndsWith("(") && workStr.StartsWith(")"))
                    {
                        var prevLineData = generatedList[lastLineIdx];

                        if (prevLineData.sqlLine.TrimEnd().Length > 0)
                            generatedList[lastLineIdx] = (prevLineData.sqlLine.TrimEnd().Substring(0, 
                                prevLineData.sqlLine.TrimEnd().Length - 1), prevLineData.lineLink);
                        generatedList[i] = (workStr.Substring(1, workStr.Length - 1), lineLink);
                        continue;
                    }
                }

                if (sqlLine.Trim() != string.Empty && !lineLink.HasSysLimitMarker)
                {
                    lastStr = sqlLine.Trim().ToLower();
                    lastLineIdx = i;
                }
            }
        }

        private static void ProcessWhereBlock(List<(string sqlLine, Line lineLink)> generatedList)
        {
            List<int> whereStartAt = new List<int>();

            for (int i = 0; i < generatedList.Count; i++)
            {
                int spaceCount = 0;
                var (lineSql, lineLink) = generatedList[i];

                // Формирование списка индексов строк, где начинаются блоки WHERE
                if (lineLink.IsWhereBeginner)
                {
                    whereStartAt.Add(i);
                    spaceCount = StartSpaceCount(lineSql);
                }

                if (whereStartAt.Count == 0)
                    continue;

                // Отмена комментирования по HasUpConditionWord
                if (lineLink.HasUpConditionWord && lineSql != string.Empty)
                    whereStartAt[whereStartAt.Count - 1] = -1;


                if (whereStartAt[whereStartAt.Count - 1] != -1 && lineLink.IsEndOfWhere)
                {
                    // Комментирование всего блоки WHERE
                    for (int z = i; z >= whereStartAt[whereStartAt.Count - 1]; z--)
                        generatedList[z] = (new string(' ', spaceCount) + "-- " + generatedList[z].sqlLine.TrimEnd(), 
                            generatedList[z].lineLink);

                    whereStartAt.RemoveAt(whereStartAt.Count - 1);
                    continue;
                }
            }
        }
    }
}