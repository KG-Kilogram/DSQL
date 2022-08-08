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
                    - ActionValue           
                - DSQLMarker
                - OrderingInfo              <==
                    - DSQLFieldInfo

        • SyntaxAnalyzer (static class)
        • Hash (static class)     
    */

    /// <summary>
    ///         Класс, экземпляры которого содержат информацию для формирования предложений
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
        ///     Сортировка по умолчанию (по первому возвращаемому полю): ORDERB BY 1 OFFSET {SkipCount} ROWS FETCH NEXT {OnPageCount} ROWS ONLY 
        /// </summary>
        public bool UseInitialSort => !OrderingFields.Exists(f => f.OrderIndex != -1);
                                    
        /// <summary>
        ///     Список полей для сортировки с направлением сортировки        
        /// </summary>
        public List<DSQLFieldInfo> OrderingFields { get; set; } = new List<DSQLFieldInfo>();

        /*
                Подробный расчет результирующего количества пропускаемых строк (смещения):
        
                1. Если Reload == true, то смещение равно установленному значению ResultingSkipCount = SkipCount;
                2. Если Reload == false, то идет загрузка новых строк и ResultingSkipCount = ord.SkipCount + ord.Loaded;
        */

        /// <summary>
        ///     Предложение OFFSET для ORDER BY. Смещение (количество строк)
        /// </summary>
        public Int64 SkipCount { get; set; }

        /*
        
                Подробный расчет результирующего количества строк для загрузки:

                1. Если Reload == true, тогда ResultingLoadCount = pages * ord.OnPageCount; Т.е. количество отображаемых страниц
                умноженное на количество строк на странице. Такая ситуация возможно при добавочной загрузке (+5, +10 ...)

                2. Если Reload == false и NumberOfAdded != 0, то ResultingLoadCount = NumberOfAdded; Добавочная загрузка. Т.е.
                запрос количества добавляемых строк;            

                3. Если Reload == false и NumberOfAdded == 0, то ResultingLoadCount = OnPageCount; При простой загрузке страницы.


            Пример настройки параметров при загрузке какой-либо страницы:

                ClientOrderBy.SkipCount = (PageNumber начиная с 0) * ClientOrderBy.OnPageCount;
                ClientOrderBy.Loaded = 0;
                ClientOrderBy.NumberOfAdded = 0;
                ClientOrderBy.Reload = false;


            Пример настройки параметров для дополнительной загрузки N строк:

                ClientOrderBy.NumberOfAdded = N;
                ClientOrderBy.Reload = false;


            Пример настроки параметров для перезагрузки текущей страница (с доп. строками, если они есть)

                ClientOrderBy.NumberOfAdded = 0;
                ClientOrderBy.Reload = true;
        */

        /// <summary>
        ///     Предложение FETCH NEXT 5 ROW ONLY для ORDER BY. Количество запрашиваемых строк
        /// </summary>
        public int OnPageCount { get; set; } = 15;
        
        /// <summary>
        ///     Хранит значение о количестве загруженных строк. Используется при выполнении операции Reload    
        /// </summary>
        public int Loaded { get; set; } 
        
        /// <summary>
        ///     Количество дополнительно загружаемых строк (+5, +10 ...)    
        /// </summary>
        public int NumberOfAdded { get; set; }

        /// <summary>
        ///     Признак выполнении операции Reload    
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