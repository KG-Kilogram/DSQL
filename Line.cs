using System.Collections.Generic;

namespace DSQL
{
    /*
        Отношение классов

        • DSQLAnalyzer (Generator)          
            - MVGroup
                - Line                      <==
                    - Word
                - SysAction
                    - ActionValue           
                - DSQLMarker
                - OrderingInfo
                    - DSQLFieldInfo

        • SyntaxAnalyzer (static class)
        • Hash (static class)     
    */

    /// <summary>
    ///     Класс объекта, описывающий строку запроса (набор слов, список маркеров, список перемнных)
    /// </summary>
    internal class Line
    {
        public List<string> Actions { get; set; } = new List<string>();

        /// <summary>
        ///     Признак наличия активирующего маркера $A_ (см. описание маркеров DSQL) во временноми описателе строки
        /// </summary>
        public bool HasActivateMarker { get; set; }

        /// <summary>
        ///     Признак наличия деактивирующего маркера $D_ (см. описание маркеров DSQL) во временноми описателе строки
        /// </summary>
        public bool HasDeactivateMarker { get; set; }

        /// <summary>
        ///     Строка-источник
        /// </summary>
        public string LineData { get; set; } = string.Empty;

        /// <summary>
        ///     Часть строки, соответствующая началу строки-источника (до начала однострочного комментария)
        ///     Line: "[T].[F] = @UserVar       -- $A_Mrk" => LineStart: "[T].[F] = @UserVar       "
        ///     
        ///     В текущем исполнении началом однострочного комментария является слово типа 
        ///     DSQLWord.DSQLWordType.wt_double_minus (исполнение для MS SQL Server)
        /// </summary>
        public string LineStart { get; set; } = string.Empty;

        /// <summary>
        ///     Признак наличия в строке признака условия
        /// </summary>
        public bool HasUpConditionWord { get; set; }

        /// <summary>
        ///     Имя маркера $O_ (см. описание маркеров DSQL)
        /// </summary>
        public string OrderingMarker { get; set; }

        /// <summary>
        ///     Признак наличия в строке маркера $EW (см. описание маркеров DSQL)
        /// </summary>
        public bool IsEndOfWhere { get; set; }

        /// <summary>
        ///     Признак наличия в строке маркера $A_SYSLIMIT (см. описание маркеров DSQL)
        /// </summary>
        public bool HasSysLimitMarker { get; set; }

        /// <summary>
        ///     Признак наличия в строке слова WHERE (признак начала условия). 
        ///     Данный признак автоматический и не требует маркера
        /// </summary>
        public bool IsWhereBeginner { get; set; }

        /// <summary>
        ///     Данный список (список маркеров строки) используется при проверке наличия всех 
        ///     активирующих маркеров для активации строки или наличие хотя бы одного
        ///     деактивирующего маркера (см. работу активирующих и деактивирующих маркеров DSQL)
        /// </summary>
        public List<DSQLMarker> Markers { get; set; }

        public List<Word> Words { get; set; } = new List<Word>();
    }
}