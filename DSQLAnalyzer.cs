using System;
using System.Collections.Generic;
using System.Linq;

/*
        ��������� �������

        � DSQLAnalyzer (Generator)          <==
            - MVGroup
                - Line
                    - Word
                - SysAction
                    - ActionValue
                - DSQLMarker
                - OrderingInfo
                    - DSQLFieldInfo

        � SyntaxAnalyzer (static class)
        � Hash (static class) 
*/

#region ����� ������ DSQL
/*
        ����� ������ DSQL 
    
        1. DSQLAnalyzer �������� ����� ������� � �������� DSQL � ��������� ������ ����� MVGroup;
    
        2. ��� ������ (��� ������ ������ MVGroup.GetCopy()) ����������� ����� ������ ����� 
        (������ � ������ ������� ��� ����, ����� �� ��������� ��������� �������� ������� ������
        ��� ����� �����������, � ��������� �� �������� ��� ������ ����������);
    
        3. � ����� ������ ��������� Actions, Markers � Subgroups, ���������� ������� ����� ���� 
        ��������� ��� ������ ������� ������:
            3.1 ��� ��������� �����: MVGroup.FindGroup(string name);
            3.2 ��� ��������: MVGroup.GetMarkersTotal();
            3.3 ��� ��������: MVGroup.GetActionsTotal();
    
        4. ��� �������� ����������� ��������, ����������� ������� ���������� ��� �����������,
        ��������� ������ (���� ����������) �����������������;
    
        5. ����������/����������� ����� ������ ����� ��������� �� ���� ����������:
            DSQLAnalyzer.GetSQLCommandParams(MVGroup root).
    
        6. ����������� ������ ���������� �������� �������������� SQL-������ � ������ ��������
        (���, ��������) ��� ������������ ������ SqlParameter. �.�. ���, ��� ����� ��� ������������
        SqlCommand;
    
        �.�. ����� ������ ��������� ������� �������� ��������� ��������� �������� �� ����� ������� 
        ���� (SQL-������� � DSQL ��������);
*/
#endregion

namespace DSQL
{
    #region ����� ������ DSQLAnalyzer
    /*
        ����� ������ DSQLAnalyzer 
        
        1. SQL-������ � DSQL ��������� ��������� � ����������� �����������;
        
        2. �������������� ���������� ��������� ������ �� ������ TemporaryLineDesriptor � ��� ������ ������ ���������:

            2.1. ����� ���� � ����������� �� �����;

            2.2. ����� ���������� ������� (@Var), ����������� � ������;

            2.3. ����� ��������, ����������� � ������ (����������� ���� �������);

            2.4. $A_ � $D_ ������� ������������� � ���������� ������� (@Var) � ���������� ���������.
                 �.�. ���������� ������� ����������� �������� ���������� �������;

            2.5. ���������� ���������� �������, ������������ �� "@sys" �� ������ ���������� �������;

        3. DSQLAnalyzer ������ ������ ����� TemporaryGroupDescriptor    

            3.1. ��������� ������� ������������ �������� �� ���������� ������� ������ 
            ����� ������ (SetADMarkerActions);
            
            3.2. ��������� ������� ������������ �������� �� ���������� �������, ����� 
            ���������� ��������� ������ ��������� ������ (CalcMarkerGroupManaged);

            3.3. ��� ������ $A_ � $D_ �������� ��������� $D_. �.�. � ������� ����������
            ���������� ������ ����� $A_ �������;
        
        4. �������� ������ ����� MVGroup �� ������ TemporaryGroupDescriptor. ������ ������ 
        ���������� � DSQLAnalyzer.InitialMVGroup. ���� ������ ����� �������������� ���
        ���������� ����������� Actions, Markers � Subgroups;

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

                            // ���������� ������ �������� � �������� �������� ��� ������ ������ �� �������� ������ 
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

                        // ���������� �������� ������
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

            // �������� �������������� �������� (�� ������ ������������ � �������������� ��������)
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
        ///     ����� ������ ������ ������������ � �������������� �������� � �������� ���������������
        ///     ������� �� ����� ������ (��� ����, ����� ���������� ������������� ����� ������������ ������)
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
        ///     ����� ������� ������������ $A_ � $D_ �������� �� ���������� (@Var) �������
        /// </summary>
        /// <param name="groups"></param>
        private static void SetADMarkerActions(List<MVGroup> groups)
        {
            /*
             
                    1.  query line 1                -- $A_MRK (������ �����������)
                    2.  query line 2 with @Var      -- $A_MRK (����� �����������)
                    3.  query line 3                -- $A_MRK (������ �����������)
             
                    ��� ���������� �������� �������������� ���������� ��������� ����� �����������
                ������� $A_MRK � ������ 2. �.�. � Makrer.Actions ���� ��������� @Var.

                    ������ ����� ������������� ����������� �� @Var ��� �������� $A_MRK
                ����� 1 � 3. �.�. ���������� @Var ������ �������������� ��� 3 ������.
             
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
        ///     ����� ������������ ����� ��������� ����������������� �����.
        ///     ��������� �������� ������ � ��������� ����������������� �����
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