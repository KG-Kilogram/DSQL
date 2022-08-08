using System.Collections.Generic;

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
                - OrderingInfo
                    - DSQLFieldInfo         <==

        � SyntaxAnalyzer (static class)
        � Hash (static class)     
    */


    /// <summary>
    ///     ��������� ����, ������������� ��� ������������ ����� ����������� ORDER BY.
    ///     TableAlias.FieldAlias [ASC/DESC]
    /// </summary>
    public class DSQLFieldInfo
    {
        public string TableAlias { get; set; } = string.Empty;
        public string FieldAlias { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        ///     ������ ������ ������������ ��� ���������� ������ ����� ����� ����������� ����������� ORDER BY
        /// </summary>
        public int OrderIndex { get; set; } = -1;
        public bool OrderDESC { get; set; } = false;

        /// <summary>
        ///     ������� ��������� ���� ���������� ��� ����, ����� ����� ���������� ���������������� �����
        /// </summary>
        public List<string> Markers = new List<string>();
    }
}