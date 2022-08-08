using System;
using System.Collections.Generic;
using System.Linq;

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

#region Схема работы DSQL
/*
        Схема работы DSQL 
    
        1. DSQLAnalyzer получает текст запроса с скриптом DSQL и формирует дерево групп MVGroup;
    
        2. Для работы (при помощи метода MVGroup.GetCopy()) формируется копия дерева групп 
        (Работа с копией ведется для того, чтобы не выполнять процедуру парсинга запроса каждый
        раз перед выполнением, а выполнить ее единожды при старте приложения);
    
        3. В копии дерева находятся Actions, Markers и Subgroups, экземпляры которых могут быть 
        извлечены при помощи методов поиска:
            3.1 Для вложенных групп: MVGroup.FindGroup(string name);
            3.2 Для маркеров: MVGroup.GetMarkersTotal();
            3.3 Для действий: MVGroup.GetActionsTotal();
    
        4. Для действий заполняются значения, безусловные маркеры включаются или отключаются,
        вложенные группы (если необходимо) мультиплицируются;
    
        5. Измененная/настроенная копия дерева групп поступает на вход генератору:
            DSQLAnalyzer.GetSQLCommandParams(MVGroup root).
    
        6. Результатом работы генератора является результирующий SQL-запрос и список кортежей
        (имя, значение) для формирования списка SqlParameter. Т.е. все, что нужно для формирования
        SqlCommand;
    
        Т.е. целью работы комплекса классов является получение множества запросов из одной кодовой 
        базы (SQL-запроса с DSQL скриптом);
*/
#endregion

namespace DSQL
{
    #region Схема работы DSQLAnalyzer
    /*
        Схема работы DSQLAnalyzer 
        
        1. SQL-запрос с DSQL маркерами поступает в конструктор анализатора;
        
        2. Синтаксический анализатор разбивает запрос на строки TemporaryLineDesriptor и для каждой строки выполняет:

            2.1. Поиск слов и определение их типов;

            2.2. Поиск переменных запроса (@Var), находящихся в строке;

            2.3. Поиск маркеров, находящихся в строке (определение типа маркера);

            2.4. $A_ и $D_ маркеры привязываются к переменным запроса (@Var) и становятся условными.
                 Т.е. активность маркера управляется наличием переменной запроса;

            2.5. Исключение переменных запроса, начинающихся на "@sys" из списка переменных запроса;

        3. DSQLAnalyzer строит дерево групп TemporaryGroupDescriptor    

            3.1. Установка неявных зависимостей маркеров от переменных запроса внутри 
            одной группы (SetADMarkerActions);
            
            3.2. Установка неявных зависимостей маркеров от переменных запроса, когда 
            переменная находится внутри вложенной группы (CalcMarkerGroupManaged);

            3.3. Для связок $A_ и $D_ маркеров удаляются $D_. Т.к. в связках управление
            происходит только через $A_ маркеры;
        
        4. Создание дерева групп MVGroup из дерева TemporaryGroupDescriptor. Корень дерева 
        помещается в DSQLAnalyzer.InitialMVGroup. Этот объект будет использоваться для
        извлечения экземпляров Actions, Markers и Subgroups;

    */
    #endregion

    public partial class DSQLAnalyzer
    {                                                                                                                       
        public DSQLAnalyzer(string query)
        {
            InitialMVGroup = MakeTemporaryRootGroup(SyntaxAnalyzer.Analyze(query));
            ProcessMarkers();
            ActionsHash = InitialMVGroup.GetHash();
        }

        private MVGroup InitialMVGroup { get; set; }
        public MVGroup GetRootGroupCopy() => (MVGroup)InitialMVGroup.GetCopy();

        public Int32 ActionsHash { get; }

        private void ProcessMarkers()
        {
            var groups = InitialMVGroup.GetSubgroupsTotal();
            groups.Add(InitialMVGroup);

            foreach (var group in groups)
            {
                foreach (var obj in group.Subobjects)
                {
                    if (obj is Line line)
                    {
                        if (line.Markers != null)
                        {
                            // Ordering initialization
                            foreach (var marker in line.Markers.Where(mrk => mrk.Cmd == DSQLMarker.DSQLMarkerCmd.OrderingBlock))
                            {
                                if (InitialMVGroup.OrderingList == null)
                                    InitialMVGroup.OrderingList = new List<OrderingInfo>();

                                InitialMVGroup.OrderingList.Add(new OrderingInfo(marker.Name));
                            }

                            // Заполнение списка маркеров и действий маркеров для группы исходя из маркеров строки 
                            foreach (var marker in line.Markers.Where(m => m.IsADMarker))
                            {
                                if (group.DSQLGroupMarkers == null)
                                    group.DSQLGroupMarkers = new List<DSQLMarker>();

                                DSQLMarker existsMarker = group.DSQLGroupMarkers.FirstOrDefault(m => m.Name == marker.Name);

                                if (existsMarker == null)
                                    group.DSQLGroupMarkers.Add(marker);
                                else
                                    foreach (var action in marker.GetActions())
                                        if (!existsMarker.HasActionName(action))
                                            existsMarker.AddActionName(action);
                            }
                        }

                        // Заполнение действий группы
                        foreach (string action in line.Actions)
                        {
                            if (group.Actions == null)
                                group.Actions = new List<SysAction>();

                            group.Actions.Add(SysAction.Make(Guid.NewGuid(), action, 0));
                        }
                    }
                }

                if (group.DSQLGroupMarkers != null)
                    foreach (var marker in group.DSQLGroupMarkers)
                    {
                        foreach (string action in marker.GetActions())
                        {
                            if (group.Actions == null)
                                group.Actions = new List<SysAction>();

                            if (!group.Actions.Exists(ma => ma.DestName == action))
                                group.Actions.Add(SysAction.Make(Guid.NewGuid(), action, 0));
                        }
                    }
            }

            SetADMarkerActions(groups);
            CalcMarkerGroupManaged(groups);

            // Удаление деактивирующих маркеров (из связок активирующих и деактивирующих маркеров)
            ProcessADBundle(groups);

            var markers = InitialMVGroup.GetMarkersTotal();
            var actions = InitialMVGroup.GetActionsTotal();

            foreach (var action in actions)
                foreach (var marker in markers)
                    if (marker.HasActionName(action.DestName) && !action.HasMarkerName(marker.Name))
                        action.AddMarkerName(marker.Name);

            foreach (var group in groups)
                group.Clear();
        }

        /// <summary>
        ///     Метод поиска связок активирующих и деактивирующих маркеров и удаление деактивирующего
        ///     маркера из таких связок (для того, чтобы управление производилось через активирующий маркер)
        /// </summary>
        private static void ProcessADBundle(List<MVGroup> groups)
        {
            foreach (var group in groups.Where(g => g.DSQLGroupMarkers != null))
                for (int i = group.DSQLGroupMarkers.Count - 1; i >= 0; i--)
                {
                    DSQLMarker m = group.DSQLGroupMarkers[i];
                    if (m.Cmd == DSQLMarker.DSQLMarkerCmd.Deactivate && 
                        group.DSQLGroupMarkers.Exists(am => am.Cmd == DSQLMarker.DSQLMarkerCmd.Activate && am.Name == m.Name))
                        group.DSQLGroupMarkers.RemoveAt(i);
                }
        }

        private static void CalcMarkerGroupManaged(List<MVGroup> groups)
        {
            foreach (var group in groups.Where(g => g.DSQLGroupMarkers != null))
                foreach (var marker in group.DSQLGroupMarkers)
                {
                    var otherGroup = groups.FirstOrDefault(grp => grp != group && grp.DSQLGroupMarkers != null && 
                        grp.DSQLGroupMarkers.Exists(mrk => mrk.Name == marker.Name && mrk.HasActions));

                    marker.IsGroupManaged = otherGroup != null;
                }
        }

        /// <summary>
        ///     Поиск неявных зависимостей $A_ и $D_ маркеров от переменных (@Var) запроса
        /// </summary>
        /// <param name="groups"></param>
        private static void SetADMarkerActions(List<MVGroup> groups)
        {
            /*
             
                    1.  query line 1                -- $A_MRK (неяная зависимость)
                    2.  query line 2 with @Var      -- $A_MRK (явная зависимость)
                    3.  query line 3                -- $A_MRK (неяная зависимость)
             
                    При построчном парсинге синтаксический анализатор установил явную зависимость
                маркера $A_MRK в строке 2. Т.е. в Makrer.Actions была добывлена @Var.

                    Данный метод устанавливает зависимость от @Var для маркеров $A_MRK
                строк 1 и 3. Т.к. отсутствие @Var должно деактивировать все 3 строки.
             
            */

            foreach (var group in groups)
                foreach (var obj in group.Subobjects)
                    if (obj is Line line)
                        if (line.Markers != null)
                            foreach (var mrk in line.Markers.Where(m => m.IsADMarker))
                            {
                                if (group.DSQLGroupMarkers == null)
                                    group.DSQLGroupMarkers = new List<DSQLMarker>();

                                var existsMarker = group.DSQLGroupMarkers.FirstOrDefault(m => m.Cmd == mrk.Cmd && m.Name == mrk.Name);

                                if (existsMarker == null)
                                    group.DSQLGroupMarkers.Add(mrk);
                                else
                                {
                                    foreach (var action in mrk.GetActions())
                                        if (!existsMarker.HasActionName(action))
                                            existsMarker.AddActionName(action);
                                }
                            }
        }

        /// <summary>
        ///     Метод формирования древа временных мультиплицируемых групп.
        ///     Обработка маркеров начала и окончания мультиплицируемых групп
        /// </summary>
        private static MVGroup MakeTemporaryRootGroup(IEnumerable<Line> lines)
        {
            MVGroup rootTempGroup = MVGroup.Make();
            MVGroup focusedGroup = rootTempGroup;

            foreach (var line in lines)
            {
                bool lineAlreadyProcessed = false;

                if (line.Markers != null)
                    foreach (var marker in line.Markers)
                        switch (marker.Cmd)
                        {
                            case DSQLMarker.DSQLMarkerCmd.BeginMV:
                                var group = MVGroup.Make();
                                group.DevName = marker.Name;
                                group.ParentGroup = focusedGroup;
                                focusedGroup.AddSubgroup(group);

                                focusedGroup.Subobjects.Add(group);

                                focusedGroup = group;
                                focusedGroup.Subobjects.Add(line);

                                lineAlreadyProcessed = true;
                                break;

                            case DSQLMarker.DSQLMarkerCmd.EndMV:
                                if (!focusedGroup.Subobjects.Contains(line))
                                    focusedGroup.Subobjects.Add(line);

                                lineAlreadyProcessed = true;
                                focusedGroup = focusedGroup.ParentGroup;
                                break;
                        }

                if (!lineAlreadyProcessed)
                    focusedGroup.Subobjects.Add(line);
            }

            return rootTempGroup;
        }
    }
}