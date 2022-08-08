using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;

namespace DSQL                                                                      
{
    /*
        Отношение классов

        • DSQLAnalyzer (Generator)          
            - MVGroup
                - Line
                    - Word
                - SysAction                 <==
                    - ActionValue           
                - DSQLMarker
                - OrderingInfo
                    - DSQLFieldInfo

        • SyntaxAnalyzer (static class)
        • Hash (static class)     
    */

    /// <summary>
    ///     Класс, экземпляры которого описывают действие пользователя (ввод данных, запуск операции выбора
    ///     или автоматическое заполнение на сервере)
    /// </summary>
    public class SysAction
    {
        /*
        
                Конструкторы 
         
        */

        [JsonConstructor]
        public SysAction() { }

        public static Type SysActionType { get; set; } = typeof(SysAction);

        public static SysAction Make(Guid uuid, string destname, int mvindex)
        {
            var ctor = SysActionType.GetConstructor(Type.EmptyTypes);
            var action = (SysAction)ctor.Invoke(Array.Empty<object>());

            action.UUID = uuid;
            action.FDestName = destname;
            action.MVIndex = mvindex;

            return action;
        }

        public virtual SysAction GetDescriptor()
        {
            SysAction action = Make(UUID, DestName, MVIndex);
            action.Markers = Markers;

            return action;
        }


        [ReadOnly(true)]
        public Guid UUID { get; set; }



        [ReadOnly(true)]
        [JsonProperty("DestName")]
        protected string FDestName { get; set; } = string.Empty;

        /// <summary>
        ///     Имя SQL параметра, используемого в запросе (Например @Var) без символа @
        /// </summary>
        [ReadOnly(true)]
        [JsonIgnore]
        public string DestName => FDestName;



        public string Markers { get; set; } = string.Empty;
        public bool HasMarkerName(string name) => Markers.Contains($"#{name}#");
        public void AddMarkerName(string name)
        {
            if (!HasMarkerName(name))
                Markers += $"#{name}#";
        } 




        /// <summary>
        ///     Значение, которое должно быть передано в виде SqlParameter в SqlCommand. Используется генератором
        ///     DSQL для формирования результирующего запроса (активация маркеров)
        /// </summary>
        [JsonProperty("Value")]
        [Browsable(false)]
        public ActionValue Value { get; } = new ActionValue() { };

        /// <summary>
        ///     Набор вложенных значений, использующийся при мультиплицировании 
        /// </summary>
        public List<ActionValue> SubValues { get; set; } = null;

        [JsonIgnore]
        public int TotalValueCount => (Value.Enabled ? 1 : 0) + (SubValues?.Count ?? 0);

        public void PushSubvalue(params object[] values)
        {
            if (SubValues is null)
                SubValues = new List<ActionValue>();

            foreach (var value in values)
            {
                if (value is Array arr)
                {
                    foreach (var v in arr)
                    {
                        ActionValue newValue = new ActionValue();
                        newValue.Data = v;
                        SubValues.Add(newValue);
                        Value.Enabled = false;
                    }
                }
                else
                {
                    ActionValue newValue = new ActionValue();
                    newValue.Data = value;
                    SubValues.Add(newValue);
                    Value.Enabled = false;
                }
            }
        }





        /// <summary>
        ///     Индекс мультиплицирования действия. При мультиплицировании группы этот индек увеличивается на 1
        ///     для каждой новой копии. Используется для идентификации и поиска экземпляра действия среди копий
        ///     (т.к. у копий все остальное одинаковое)
        /// </summary>
        [Browsable(false)]
        public int MVIndex { get; set; }

        public static void RecalcMVIndexes<T>(List<T> actions) where T : SysAction
        {
            Dictionary<string, int> indexes = new Dictionary<string, int>();

            foreach (var a in actions)
                if (indexes.ContainsKey(a.DestName))
                {
                    indexes[a.DestName]++;
                    a.MVIndex = indexes[a.DestName];
                }
                else
                {
                    a.MVIndex = 0;
                    indexes.Add(a.DestName, 0);
                }
        }


        public int GetHash() => Hash.GetDeterministicHash(DestName);
    }
}