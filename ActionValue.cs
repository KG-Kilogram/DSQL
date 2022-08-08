using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSQL
{
    /*
        Отношение классов

        • DSQLAnalyzer (Generator)          
            - MVGroup
                - Line
                    - Word
                - SysAction
                    - ActionValue           <==
                - DSQLMarker
                - OrderingInfo
                    - DSQLFieldInfo

        • SyntaxAnalyzer (static class)
        • Hash (static class)     
    */

    public class ActionValue
    {
        private object _data;

        public object Data
        {
            get => _data;
            set
            {
                _data = value;
                Enabled = true;
            }
        }

        private Type _dataType = typeof(string);

        [JsonProperty("DataType")]
        public Type DataType
        {
            get => _dataType;
            set
            {
                _dataType = value;

                if (Initializing)
                    if (_dataType.IsEnum && DevelopEnumTypes.IndexOf(DataType) == -1)
                        DevelopEnumTypes.Add(_dataType);

                EnumIndex = DevelopEnumTypes.IndexOf(DataType);
            }
        }

        [JsonProperty("dataTypeName")]
        public string DataTypeName => _dataType.ToString();


        /// <summary>
        ///         Для типов данных enum значение отлично от 0 и равно позиции типа данных (enum`a) 
        ///     в списке DevelopEnumTypes.
        /// </summary>
        [JsonProperty("enumIndex")]
        public int EnumIndex { get; private set; } = -1;


        /// <summary>
        ///         Флаг процесса инициализации во время которого происходит накопление enum`ов, используемых
        ///     как тип данных в действиях древа-шаблона.
        /// </summary>
        public static bool Initializing { get; set; }


        /// <summary>
        ///     Список enum`ов, используемых как тип данных действий
        /// </summary>
        [JsonIgnore]
        public static List<Type> DevelopEnumTypes { get; } = new List<Type>();

        public bool Enabled { get; set; }


        public static IEnumerable<ActionValue> MakeRange(IEnumerable<object> objects)
        {
            if (objects != null)
                foreach (var obj in objects)
                    yield return new ActionValue() { Data = obj };
        }
    }
}