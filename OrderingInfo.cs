using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSQL
{
    /*
        ��������� �������

        � DSQLAnalyzer (Generator)          
            - MVGroup
                - Line
                    - Word
                - SysAction
                    - ActionValue           
                - DSQLMarker
                - OrderingInfo              <==
                    - DSQLFieldInfo

        � SyntaxAnalyzer (static class)
        � Hash (static class)     
    */

    /// <summary>
    ///         �����, ���������� �������� �������� ���������� ��� ������������ �����������
    ///     ORDERB BY ..., ... OFFSET ... ROWS FETCH NEXT ... ROWS ONLY
    /// </summary>
    public class OrderingInfo
    {
        [JsonConstructor]
        public OrderingInfo(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public bool Enabled { get; set; } 

        /// <summary>
        ///     ���������� �� ��������� (�� ������� ������������� ����): ORDERB BY 1 OFFSET {SkipCount} ROWS FETCH NEXT {OnPageCount} ROWS ONLY 
        /// </summary>
        public bool UseInitialSort => !OrderingFields.Exists(f => f.OrderIndex != -1);
                                    
        /// <summary>
        ///     ������ ����� ��� ���������� � ������������ ����������        
        /// </summary>
        public List<DSQLFieldInfo> OrderingFields { get; set; } = new List<DSQLFieldInfo>();

        /*
                ��������� ������ ��������������� ���������� ������������ ����� (��������):
        
                1. ���� Reload == true, �� �������� ����� �������������� �������� ResultingSkipCount = SkipCount;
                2. ���� Reload == false, �� ���� �������� ����� ����� � ResultingSkipCount = ord.SkipCount + ord.Loaded;
        */

        /// <summary>
        ///     ����������� OFFSET ��� ORDER BY. �������� (���������� �����)
        /// </summary>
        public Int64 SkipCount { get; set; }

        /*
        
                ��������� ������ ��������������� ���������� ����� ��� ��������:

                1. ���� Reload == true, ����� ResultingLoadCount = pages * ord.OnPageCount; �.�. ���������� ������������ �������
                ���������� �� ���������� ����� �� ��������. ����� �������� �������� ��� ���������� �������� (+5, +10 ...)

                2. ���� Reload == false � NumberOfAdded != 0, �� ResultingLoadCount = NumberOfAdded; ���������� ��������. �.�.
                ������ ���������� ����������� �����;            

                3. ���� Reload == false � NumberOfAdded == 0, �� ResultingLoadCount = OnPageCount; ��� ������� �������� ��������.


            ������ ��������� ���������� ��� �������� �����-���� ��������:

                ClientOrderBy.SkipCount = (PageNumber ������� � 0) * ClientOrderBy.OnPageCount;
                ClientOrderBy.Loaded = 0;
                ClientOrderBy.NumberOfAdded = 0;
                ClientOrderBy.Reload = false;


            ������ ��������� ���������� ��� �������������� �������� N �����:

                ClientOrderBy.NumberOfAdded = N;
                ClientOrderBy.Reload = false;


            ������ �������� ���������� ��� ������������ ������� �������� (� ���. ��������, ���� ��� ����)

                ClientOrderBy.NumberOfAdded = 0;
                ClientOrderBy.Reload = true;
        */

        /// <summary>
        ///     ����������� FETCH NEXT 5 ROW ONLY ��� ORDER BY. ���������� ������������� �����
        /// </summary>
        public int OnPageCount { get; set; } = 15;
        
        /// <summary>
        ///     ������ �������� � ���������� ����������� �����. ������������ ��� ���������� �������� Reload    
        /// </summary>
        public int Loaded { get; set; } 
        
        /// <summary>
        ///     ���������� ������������� ����������� ����� (+5, +10 ...)    
        /// </summary>
        public int NumberOfAdded { get; set; }

        /// <summary>
        ///     ������� ���������� �������� Reload    
        /// </summary>
        public bool Reload { get; set; } 

        public OrderingInfo GetCopy()
        {
            var res = new OrderingInfo(Name)
            {
                Enabled = Enabled,
                NumberOfAdded = NumberOfAdded,
                Loaded = Loaded,
                SkipCount = SkipCount,
                OnPageCount = OnPageCount,
                Reload = Reload,
            };

            foreach (var fld in OrderingFields)
            {
                var newField = new DSQLFieldInfo()
                {
                    TableAlias = fld.TableAlias,
                    FieldName = fld.FieldName,
                    OrderIndex = fld.OrderIndex,
                    OrderDESC = fld.OrderDESC,
                };

                foreach (var m in fld.Markers)
                    newField.Markers.Add(m);

                res.OrderingFields.Add(newField);
            }

            return res;
        }
    }
}