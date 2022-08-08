using System.Collections.Generic;

namespace DSQL
{
    /*
        ќтношение классов

        Х DSQLAnalyzer (Generator)          
            - MVGroup
                - Line
                    - Word
                - SysAction
                    - ActionValue           
                - DSQLMarker
                - OrderingInfo
                    - DSQLFieldInfo         <==

        Х SyntaxAnalyzer (static class)
        Х Hash (static class)     
    */


    /// <summary>
    ///     ќписатель пол€, спользующийс€ при формировании полей предложени€ ORDER BY.
    ///     TableAlias.FieldAlias [ASC/DESC]
    /// </summary>
    public class DSQLFieldInfo
    {
        public string TableAlias { get; set; } = string.Empty;
        public string FieldAlias { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        ///     ƒанный индекс используетс€ дл€ сортировки списка полей перед построением предложени€ ORDER BY
        /// </summary>
        public int OrderIndex { get; set; } = -1;
        public bool OrderDESC { get; set; } = false;

        /// <summary>
        ///     ћаркеры описател€ пол€ необходимы дл€ того, чтобы сн€ть сортировку деактивированных полей
        /// </summary>
        public List<string> Markers = new List<string>();
    }
}