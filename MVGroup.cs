using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace DSQL
{
    /*
        Отношение классов

        • DSQLAnalyzer (Generator)          
            - MVGroup                       <==
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

    /*
            Схема работы MVGroup
    
            SELECT                  --                                        | <== начало группы root
                *                   --                                        |
            FROM                    --                                        |
                [T]                 --                                        |
            WHERE                   --                                        |
                [T].[ID] IN (       --                                        |
                    @VarID          -- $2VcID   | <== однострочная группа cID |
                )                   --                                        |
            ;                       --                                        | <== конец группы root

            Соответсвенно после выполнения анализа запроса будем иметь:

            DSQLAnalyzer analyzeResult = new(query);

            analyzeResult.InitialMVGroup                <== root
            analyzeResult.InitialMVGroup.Subgroups      <== список из одной подгруппы cID

            * Корневая (root) группа создается всегда и содержит в себе строки кода, не содержащиеся
            во вложенных группах.

            ** Корневая группа не может быть мультиплицирована
          
            Группы содержат списки Actions и Markers управляя активностью/значениями которых
            можно влиять на получаемый запрос.

            DSQLAnalyzer в методе получения результирующего запроса и результирующего набора 
            параметров (GetSQLCommandParams) проводит 2 логические операции:
                1. Получение кода SQL из корневой группы MVGroup.GetQueryFor(...);
                2. Постобработка полученного запроса
    
    */

    public class MVGroup
    {
        /*
            Конструкторы / фабрики 
        */

        public static Type MVGroupType { get; set; } = typeof(MVGroup);

        private static MVGroup GetInstance()
        {
            var ctor = MVGroupType.GetConstructor(Type.EmptyTypes);
            return (MVGroup)ctor.Invoke(Array.Empty<object>());
        }

        internal static MVGroup Make(UInt64? devIndex = null)
        {
            var group = GetInstance();
            group.Subobjects = new List<object>();

            if (devIndex == null)
                group.DevIndex = _index++;
            else
                group.DevIndex = devIndex.Value;

            group.UIndex = _index++;

            return group;
        }




        /*
            json
        */

        [OnDeserialized]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            if (ActionGroups != null)
                foreach (var g in ActionGroups)
                    g.ParentGroup = this;
        }




        /*
                   
        */

        private string _devName = string.Empty;
        
        /// <summary>
        ///     Имя группы, указанное разработчиком
        /// </summary>
        [JsonProperty("DevName")]
        public string DevName 
        {
            get
            {
                if (_devName == string.Empty)
                    _devName = DevIndex.ToString();

                return _devName;
            }
            set => _devName = value; 
        }

        /// <summary>
        ///     Индекс группы, полученный исходя из логики разработки.
        ///     При мультиплицировании сохраняется
        /// </summary>
        [Browsable(false)]
        public UInt64 DevIndex { get;  set; }

        /// <summary>
        ///     Уникальный индекс группы (при мультиплицировании новая группа 
        ///     получает новый уникальный индекс)    
        /// </summary>
        [Browsable(false)]
        public UInt64 UIndex { get; set; }

        [JsonProperty("IsFirst")]
        public bool IsFirst
        {
            get
            {
                if (ParentGroup is null)
                    return false;
                else
                {
                    foreach (var group in ParentGroup.Subgroups)
                    {
                        if (group.DevName == DevName)
                            return group == this;
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public List<SysAction> Actions { get; set; } = new List<SysAction>();

        [Browsable(false)]
        public IEnumerable<SysAction> MVActions => 
            Actions?.Where(action => action.Markers == string.Empty) ?? Enumerable.Empty<SysAction>();

        [Browsable(false)]
        public IEnumerable<SysAction> MarkerActions => 
            Actions?.Where(action => action.Markers != string.Empty) ?? Enumerable.Empty<SysAction>();

        [JsonIgnore]
        public List<OrderingInfo> OrderingList { get; set; } = null;

        [JsonIgnore]
        public MVGroup ParentGroup { get; internal set; } = null;




        /*
            Subgroups
        */

        [Browsable(false)]
        [JsonProperty("ActionGroups")]
        protected List<MVGroup> ActionGroups { get; set; }

        [JsonIgnore]
        public MVGroup[] Subgroups => ActionGroups == null ? Array.Empty<MVGroup>() : ActionGroups.ToArray();

        #region Методы, обеспечивающие работу с private ActionGroups
        public void AddSubgroup(MVGroup group)
        {
            if (ActionGroups == null)
                ActionGroups = new List<MVGroup>();

            group.ParentGroup = this;
            ActionGroups.Add(group);
        }

        public void InsertSubgroup(int inIdx, MVGroup group)
        {
            group.ParentGroup = this;
            ActionGroups.Insert(inIdx, group);
        }

        public void DelSubgroup(MVGroup group)
        {
            if (ActionGroups != null)
                ActionGroups.Remove(group);
        }

        public void DelSubgroup(int index)
        {
            if (ActionGroups != null)
                ActionGroups.RemoveAt(index);
        }

        public void ClearSubgroups()
        {
            if (ActionGroups != null)
                ActionGroups.Clear();
        }

        #endregion




        /*
            internal & private properties (fields)
        */


        [JsonIgnore]
        internal List<object> Subobjects;

        [JsonProperty("DSQLGroupMarkers")]
        internal List<DSQLMarker> DSQLGroupMarkers { get; set; }

        /// <summary>
        ///     Поле уникального индекса
        /// </summary>
        [JsonIgnore]
        private static UInt64 _index = 0;

        /// <summary>
        ///     Оператор мультиплицирования группы
        /// </summary>
        [JsonIgnore]
        private string GroupOperator
        {
            get
            {
                if (DevName.StartsWith("c"))
                    return ",";
                else
                    return DevName.StartsWith("o") ? "OR " : "AND ";
            }
        }




        /*
            Public методы поиска и рекурсивного получения списков объектов         
        */

        public MVGroup FindGroup(string name) => 
            GetSubgroupsTotal().FirstOrDefault(g => g.DevName == name);

        private void GetMarkersTotalRec(List<DSQLMarker> list)
        {
            if (DSQLGroupMarkers != null)
                list.AddRange(DSQLGroupMarkers);

            foreach (MVGroup g in Subgroups)
                g.GetMarkersTotalRec(list);
        }

        public List<DSQLMarker> GetMarkersTotal()
        {
            List<DSQLMarker> res = new List<DSQLMarker>();
            GetMarkersTotalRec(res);
            return res;
        }

        private void GetActionsTotalRec(List<SysAction> list)
        {
            if (Actions != null)
                list.AddRange(Actions.ToList());

            foreach (MVGroup g in Subgroups)
                g.GetActionsTotalRec(list);
        }

        public List<SysAction> GetActionsTotal()
        {
            List<SysAction> res = new List<SysAction>();
            GetActionsTotalRec(res);
            return res;
        }

        public MVGroup GetRootGroup()
        {
            MVGroup group = this;
            while (group.ParentGroup != null)
                group = group.ParentGroup;

            return group;
        }

        private void GetSubgroupsTotalRec(List<MVGroup> list)
        {
            list.AddRange(Subgroups);

            foreach (MVGroup g in Subgroups)
                g.GetSubgroupsTotalRec(list);
        }

        public List<MVGroup> GetSubgroupsTotal()
        {
            List<MVGroup> res = new List<MVGroup>();
            GetSubgroupsTotalRec(res);
            return res;
        }










        public virtual object GetCopy(bool withActions = true)
        {
            if (DevName == string.Empty)
                DevName = DevIndex.ToString();

            MVGroup group = Make(DevIndex);
            group.DevName = DevName;
            group.ParentGroup = ParentGroup;

            if (Actions != null && withActions)
            {
                group.Actions = new List<SysAction>();

                foreach (var action in Actions)
                    group.Actions.Add(action.GetDescriptor());
            }

            if (DSQLGroupMarkers != null)
            {
                group.DSQLGroupMarkers = new List<DSQLMarker>();
                foreach (var marker in DSQLGroupMarkers)
                    group.DSQLGroupMarkers.Add(marker.GetCopy());
            }

            foreach (var g in Subgroups)
                group.AddSubgroup((MVGroup)g.GetCopy(withActions));

            if (OrderingList != null)
                foreach (var ord in OrderingList)
                {
                    if (group.OrderingList == null)
                        group.OrderingList = new List<OrderingInfo>();

                    group.OrderingList.Add(ord.GetCopy());
                }

            return group;
        }



        public int LastMultiplyCount;

        /// <summary>
        ///     Мультиплицирование группы исходя из списков SubValues передаваемых Actions
        /// </summary>
        /// <param name="group">Мультиплицируемая группа</param>
        /// <param name="actions">Действия, SubValues которых следует использовать при мультиплицировании.
        /// Количество SubValues в кадом действии должно быть одинаковым</param>
        /// <param name="maxActionsCount">Максимальное количество переменных запроса SQL-сервера</param>
        /// <returns>true, если мультиплицирование было произведено (SubValues было больше 0)</returns>
        public static bool MultiplyGroupForSubvalues(MVGroup group, SysAction[] actions, int maxActionsCount = 2001)
        {
            group.LastMultiplyCount = 0;

            if (group.ParentGroup == null)
                throw new ArgumentException("Root group cannot be multiplied.");

            if (actions.Length == 0)
                throw new ArgumentException("Action array is empty.");

            var workActions = actions.Where(a => a.SubValues != null && a.SubValues.Count > 0).ToList();

            if (workActions.Count == 0)
                return false;

            if (workActions[0].SubValues.Count == 0)
                return false;

            if (group.Subgroups.Length > 0)
                throw new ArgumentException("MultiplyGroupForSubvalues method not support nested groups.");




            /*
                Очистка списка, в котором находится мультиплицируемая группа, от всех групп 
                с DevIndex равным DevIndex мультиплицруемой группы. (т.е. сама мультиплицируемая 
                группа тоже удаляется)
            */

            var subs = group.ParentGroup.Subgroups;
            for (int i = subs.Length - 1; i >= 0; i--)
            {
                if (subs[i].DevIndex == group.DevIndex)
                    group.ParentGroup.DelSubgroup(i);
            }


            /*
                Т.к. в мультиплицировании могут участвовать не все действия группы, то 
                формируется список соответвия индексов workActions и Actions  
            */

            int[] actionIndexes = new int[workActions.Count];
            for (int i = 0; i < workActions.Count; i++)
            {
                var a = workActions[i];
                if (a.Value.Enabled && a.SubValues.Count != workActions[0].SubValues.Count)
                    throw new ArgumentException("The number of values ​​must be the same for all actions in the array.");

                actionIndexes[i] = group.Actions.IndexOf(workActions[i]); 
            }


            /*
                Мультиплицирование длится до тех пор пока:
                    1. Следующая итекация не приведет к превышению maxActionsCount;
                    2. Количество итераций меньше SubValues.Count.
            */

            int iterationCount = 0;
            while ((iterationCount + 1) * workActions.Count < maxActionsCount)
            {
                group.LastMultiplyCount++;
                MVGroup groupCopy = (MVGroup)group.GetCopy();

                for (int i = 0; i < workActions.Count; i++)
                    groupCopy.Actions[actionIndexes[i]].Value.Data = workActions[i].SubValues[iterationCount].Data;

                group.ParentGroup.AddSubgroup(groupCopy);
                iterationCount++;

                if (iterationCount == workActions[0].SubValues.Count)
                    break;
            }

            foreach (SysAction a in workActions)
                a.SubValues.RemoveRange(0, iterationCount);

            SysAction.RecalcMVIndexes(group.GetRootGroup().GetActionsTotal());

            return true;
        }

        public string GetSourceCode()
        {
            string code = string.Empty;
            GetSourceCode(this, ref code);
            return code;
        }



        /*
         
        */

        private void GetSourceCode(MVGroup group, ref string code)
        {
            foreach (var o in group.Subobjects)
                if (o is MVGroup subgroup)
                    GetSourceCode(subgroup, ref code);
                else if (o is Line ln)
                    code += string.Format("{0}\r\n", ln.LineData.TrimEnd());
        }

        /// <summary>
        ///     Часть генератора для получения строк кода мультиплицируемой группы (и корневой тоже)   
        /// </summary>
        private void GetQueryFor(List<MVGroup> allTemplateGroups, bool isFirstGroup, ref List<(string, Line)> outLines)
        {
            /*
                Группа-шаблон для this (шаблон содержит строки исходного запроса) 
            */

            MVGroup templateGroup = allTemplateGroups.FirstOrDefault((g) => g.DevName == DevName);

            if (templateGroup == null)
                return;




            /*
                Получение строк с именами $A_ и $D_ маркеров группы (только активных) 
            */

            string activatingMarkers = string.Empty;
            string deactivatingMarkers = string.Empty;

            if (DSQLGroupMarkers != null)
                foreach (DSQLMarker m in DSQLGroupMarkers)
                {
                    if (m.Enabled)
                        if (m.Cmd == DSQLMarker.DSQLMarkerCmd.Activate)
                            activatingMarkers += string.Format("#{0}#", m.Name);
                        else if (m.Cmd == DSQLMarker.DSQLMarkerCmd.Deactivate)
                            deactivatingMarkers += string.Format("#{0}#", m.Name);
                }



            /*
                Генерация кода для строк группы и подгрупп    
            */

            bool firstGroupLine = true;

            foreach (object so in templateGroup.Subobjects)
                if (so is MVGroup subgroup)
                {
                    var clientSubGroups = ActionGroups.FindAll((g) => g.DevName == subgroup.DevName);

                    foreach (var subGroup in clientSubGroups)
                        subGroup.GetQueryFor(allTemplateGroups, subGroup == clientSubGroups[0], ref outLines);
                }
                else
                {
                    Line line = so as Line;

                    /*
                        Вычисление hasAllActivatingMarkers (true, если все активирующие маркеры активны) и
                        hasDeactivatingMarker (true, если есть хоть один активный деактивирующий маркер).

                        Генерация строки производится только в том случае, когда активны все $A_ и не активны
                        все $D_ маркеры строки.
                    */

                    bool hasAllActivatingMarkers = true;
                    bool hasDeactivatingMarker = false;

                    if (line.Markers != null)
                        for (int j = 0; j < line.Markers.Count; j++)
                        {
                            var marker = line.Markers[j];

                            if (marker.Cmd == DSQLMarker.DSQLMarkerCmd.Activate && !activatingMarkers.Contains(marker.Name))
                            {
                                hasAllActivatingMarkers = false;
                                break;
                            }
                            else if (marker.Cmd == DSQLMarker.DSQLMarkerCmd.Deactivate && activatingMarkers.Contains(marker.Name))
                            {
                                hasDeactivatingMarker = true;
                                break;
                            }
                            else if (marker.Cmd == DSQLMarker.DSQLMarkerCmd.Deactivate && deactivatingMarkers.Contains(marker.Name))
                            {
                                hasDeactivatingMarker = true;
                                break;
                            }
                        }


                    /*
                      
                    */

                    string generatedSqlLine = string.Empty;

                    if (hasAllActivatingMarkers && !hasDeactivatingMarker)
                    {
                        generatedSqlLine = line.LineStart.TrimEnd();


                        /*
                            Для первой строки мультиплицированной группы (не первой группы в списке мультиплицированных)
                            следует добавить оператор мультиплицирования группы 
                        */
                        if (firstGroupLine && !isFirstGroup)
                        {
                            string ws = generatedSqlLine.ToLower().Trim();

                            if (!ws.StartsWith(",") && !ws.StartsWith("and") && !ws.StartsWith("or"))
                            {
                                int SpaceCount = DSQLAnalyzer.StartSpaceCount(generatedSqlLine);
                                string Spaces = new string(' ', SpaceCount);
                                generatedSqlLine = string.Format("{0}{1}{2}", Spaces, GroupOperator, generatedSqlLine.TrimStart());
                            }
                        }




                        /* 
                            Замена имен переменных в зависимости от индекса мультипликации 
                        */
//                        if (MVActions != null)
                            foreach (SysAction action in MVActions.Where(a => a.MVIndex > 0))
                                generatedSqlLine = generatedSqlLine.Replace($"@{action.DestName}", 
                                    string.Format("{0}_MV{1}", $"@{action.DestName}", action.MVIndex));

                        if (DSQLGroupMarkers != null)
                            foreach (DSQLMarker marker in DSQLGroupMarkers)
                                foreach (SysAction action in GetMarkerActions(marker).Where(a => a.MVIndex > 0))
                                    generatedSqlLine = generatedSqlLine.Replace($"@{action.DestName}", 
                                        string.Format("{0}_MV{1}", $"@{action.DestName}", action.MVIndex));



                        outLines.Add((generatedSqlLine, line));

                        if (generatedSqlLine.Trim() != string.Empty)
                            firstGroupLine = false;
                    }
                    else
                        outLines.Add((string.Empty, line));
                }
        }






        /// <summary>
        ///    Метод получает копию группы и добавляет в список в котором содержится 
        ///    мультиплицируемая группа. Данный вид мультиплицирования следует применять 
        ///    при реализации сл. логики:
        ///    
        ///         1. Получение копии основной группы DSQLAnalyzer.GetRootGroupCopy();
        ///         2. Поиск группы для мультиплицирования;
        ///         3. Получение мультиплицированной копии;
        ///         4. Получение действий из всех групп и заполнение значений;
        ///         5. Генерация результирующего SQL-запроса и списка параметров;
        /// </summary>
        /// <param name="group">Мультиплицируемая группа</param>
        /// <returns>Копия группы</returns>
        /// <exception cref="ArgumentException">Root group cannot be multiplied</exception>
        internal static MVGroup MultiplyGroup(MVGroup group)
        {
            if (group.ParentGroup == null)
                throw new ArgumentException("Root group cannot be multiplied");

            MVGroup newGroup = (MVGroup)group.GetCopy();

            group.ParentGroup.InsertSubgroup(
                group.ParentGroup.Subgroups.ToList().FindLastIndex((g) => g.DevName == group.DevName) + 1, newGroup);

            SysAction.RecalcMVIndexes(group.GetRootGroup().GetActionsTotal());

            return newGroup;
        }






        /// <summary>
        ///     
        /// </summary>
        /// <param name="marker"></param>
        /// <returns>Список действий для конкретного экземпляра маркера</returns>
        internal IEnumerable<SysAction> GetMarkerActions(DSQLMarker marker)
        {
            MVGroup rootGroup = GetRootGroup();

            var groups = rootGroup.GetSubgroupsTotal();
            groups.Add(rootGroup);

            var markerGroup = groups.FirstOrDefault(g => g.DSQLGroupMarkers != null && 
                g.DSQLGroupMarkers.Contains(marker));

            if (markerGroup?.MarkerActions != null)
                foreach (var ma in markerGroup.MarkerActions)
                    if (marker.HasActionName(ma.DestName))
                        yield return ma;
        }




        internal static void CalcMarkersEnabled(MVGroup initialGroup)
        {
            var markers = initialGroup.GetMarkersTotal()
                .Where(m => m.HasActions || m.IsGroupManaged).ToList();
            var actions = initialGroup.GetActionsTotal();

            // Поиск деактивированных маркеров
            List<(string markerName, int mvindex)> deactivatedMarkerNames = new List<(string markerName, int mvindex)>();
            string processedMarkerNames = string.Empty;

            foreach (var marker in markers)
            {
                if (processedMarkerNames.Contains($"#{marker.Name}#"))
                    continue;

                var action = actions.FirstOrDefault(a => !a.Value.Enabled && a.HasMarkerName(marker.Name));
                if (action != null)
                    deactivatedMarkerNames.Add((marker.Name, action.MVIndex));

                processedMarkerNames += $"#{marker.Name}#";
            }

            // Поиск Actions, деактивированных маркерами
            List<SysAction> deactivatedByMarker = new List<SysAction>();
            foreach (var a in actions)
                foreach (var daectivated in deactivatedMarkerNames)
                    if (a.HasMarkerName(daectivated.markerName) && a.MVIndex == daectivated.mvindex)
                        deactivatedByMarker.Add(a);


            foreach (var marker in markers)
            {
                var thisMarkerInstanceActions = initialGroup.GetMarkerActions(marker).ToList();

                marker.Enabled = !thisMarkerInstanceActions.Any(a => !a.Value.Enabled);

                if (!marker.Enabled)
                    continue;

                foreach (var a in thisMarkerInstanceActions)
                    if (deactivatedByMarker.Contains(a))
                    {
                        marker.Enabled = false;
                        break;
                    }
            }

            /*
                Для маркеров, управляемых из вложенной группы дополнительный расчет 
            */
            foreach (var m in markers.Where(mrk => mrk.IsGroupManaged))
            {
                var thisMarkerInstances = markers.Where(mrk => mrk.Name == m.Name && mrk.HasActions).ToList();

                bool hasDisabled = thisMarkerInstances.Exists(mrk => !mrk.Enabled && 
                    !thisMarkerInstances.Exists(mrk2 => mrk != mrk2 && DSQLMarker.ActionsIsEqual(mrk, mrk2) && mrk2.Enabled));

                m.Enabled = m.Enabled && !hasDisabled;
            }    
        }

        internal List<(string SQLLine, Line LineLink)> GetQuery(List<MVGroup> allTemplateGroups)
        {
            List<(string, Line)> outLines = new List<(string, Line)>();
            GetQueryFor(allTemplateGroups, true, ref outLines);
            return outLines;
        }

        /*
            Очистка списков, неиспользуемых после создания группы-шаблона 
        */
        internal void Clear()
        {
            foreach (var obj in Subobjects)
                if (obj is MVGroup group)
                    group.Clear();
                else if (obj is Line line)
                {
                    line.Words = null;
                    line.Actions = null;
                }
        }

        internal int GetHash()
        {
            int hash = Hash.GetDeterministicHash(DevName, DevIndex);

            if (ActionGroups != null)
                foreach (MVGroup g in ActionGroups)
                    hash ^= g.GetHash();

            if (DSQLGroupMarkers != null)
                foreach (DSQLMarker m in DSQLGroupMarkers)
                    hash ^= m.GetHash();

            if (Actions != null)
                foreach (SysAction a in Actions)
                    hash ^= a.GetHash();

            return hash;
        }
    }
}