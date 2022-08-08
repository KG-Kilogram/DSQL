using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace DSQL
{
    internal partial class Program
    {
        static void Main()
        {
            WriteInitialLines();
            WriteHomeScreen();

            while (true)
            {
                string line = Console.ReadLine().ToLower().Trim();

                Console.Clear();

                WriteInitialLines();
                Ln(0, string.Format("Command: \"{0}\"", line));

                switch (line)
                {
                    case "q":
                    case "quit":
                    case "exit":
                        return;

                    case "i":
                        ExampleInit();
                        break;

                    case "h":
                        WriteHomeScreen();
                        break;

                    default:
                        if (Int32.TryParse(line, out int result))
                        {
                            string method = string.Format("Example{0}", result.ToString("000"));
                            MethodInfo methodInfo = typeof(Program).GetMethod(method, BindingFlags.Public | BindingFlags.Static);

                            if (methodInfo != null)
                            {
                                methodInfo.Invoke(null, Array.Empty<object>());

                                Ln();
                                Ln();
                                Ln(1, "// -----------------------------------------------------------------------");
                                Ln(1, $"// Попробуйте и другие варианты комбинаций Enabled / Disabled маркеров и/или " +
                                    $"параметров для данного примера (см. ..\\Examples\\{method}.cs)");
                                Ln();
                                Ln(1, "// Для корректного отображения результатов запроса необходимо " +
                                    "заполнить строку подключения (см. ..\\Program.cs => private static readonly string _connectionString)");
                                Ln();
                                Ln(1, "// Посмотреть запросы для создания тестовых объектов можно при помощи команды \"i\"");
                            }
                            else
                                Ln(0, string.Format("(!) Такого примера не существует: \"{0}\"", line));
                        }
                        else
                            Ln(0, string.Format("(!) Неизвестная команда: \"{0}\"", line));

                        break;
                }
            }
        }

        private static void WriteHomeScreen()
        {
            string str =
                "    Повышение читаемости кода и упрощение сопровождения ПО при работе с SQL.               \r\n" +
                "                                                                                           \r\n" +
                "    Подробнее: https://sites.google.com/view/dsql-overview                                 \r\n" +
                "                                                                                           \r\n";

            Ln(2, str);
        }

        private static void WriteInitialLines()
        {
            Ln(0, "Приложение-демонстратор работы пакета DSQL\r\n");
            Ln(1, "> Введите q, quit или exit для выхода;");
            Ln(1, "> Введите номер примера для его запуска (начиная с 0);");
            Ln(1, "> Введите i для отображения скриптов создания тестовых таблиц;");
            Ln();
        }

        private static readonly string
            server = "192.168.1.55",
            database = "dsql_test";

        private static readonly string _connectionString = $"server={server};database={database};Trusted_Connection=True;MultipleActiveResultSets=true";

        private static void RunQuery(string query, List<(string name, SysAction action)> @params)
        {
            using SqlConnection connection = new(_connectionString);
            try
            {
                connection.Open();
                try
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = query;

                    foreach (var (name, action) in @params)
                        cmd.Parameters.Add(new SqlParameter(name, action.Value.Data));

                    var reader = cmd.ExecuteReader();

                    int[] fieldLength = new int[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                        fieldLength[i] = 0;

                    List<string[]> fieldsData = new();

                    string[] coldata = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        fieldLength[i] = reader.GetName(i).Length;
                        coldata[i] = reader.GetName(i);
                    }
                    fieldsData.Add(coldata);

                    while (reader.Read())
                    {
                        string[] data = new string[reader.FieldCount];
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string field_data = reader.GetValue(i).ToString();

                            if (field_data.Length > fieldLength[i])
                                fieldLength[i] = field_data.Length;

                            data[i] = field_data;
                        }
                        fieldsData.Add(data);
                    }

                    WriteTableResults(fieldsData, fieldLength);
                    reader.Close();
                }
                finally
                {
                    connection.Close();
                }
            }
            catch (Exception ex) 
            { 
                Ln(4, $"Ошибка: {ex.Message}"); 
            }
        }

        private static void RunNonQuery(string query, List<(string name, SysAction action)> @params)
        {
            using SqlConnection connection = new(_connectionString);
            try
            {
                connection.Open();
                try
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = query;

                    foreach (var (name, action) in @params)
                        cmd.Parameters.Add(new SqlParameter(name, action.Value.Data));

                    var tran = connection.BeginTransaction();
                    try
                    {
                        cmd.Transaction = tran;
                        cmd.ExecuteNonQuery();
                        Ln();
                        Ln(3, "Запрос выполнен удачно!");
                        Ln(3, "Данный запрос выполнялся внутри транзакции, которая была завершена откатом (Rollback).");
                    }
                    finally
                    {
                        tran.Rollback();
                        tran.Dispose();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Ln(4, $"Ошибка: {ex.Message}");
            }
        }

        private static void WriteTableResults(List<string[]> fieldsData, int[] fieldsLength)
        {
            if (fieldsData.Count == 0)
                return;

            Ln();
            Ln(3, string.Format("Результат запроса (полей: {0}, строк: {1}):", fieldsLength.Length, fieldsData.Count - 1));

            string outString = string.Empty;
            Ln(4, outString);

            // Троки заголовков таблицы
            outString = "| ";
            for (int i = 0; i < fieldsLength.Length; i++)
                outString += fieldsData[0][i] + new string(' ', fieldsLength[i] - fieldsData[0][i].Length) + " | ";
            outString = outString[0..^2] + "|";
            Ln(4, outString);

            // Строка разделитель заголовка и данных
            outString = " -";
            for (int i = 0; i < fieldsLength.Length; i++)
                outString += new string('-', fieldsLength[i]) + "- -";
            outString = outString[0..^2] + " ";
            Ln(4, outString);

            // Строки с данными
            for (int j = 1; j < fieldsData.Count; j++)
            {
                string[] data = fieldsData[j];

                outString = "| ";

                for (int i = 0; i < data.Length; i++)
                    outString += string.Format("{0}{1} | ", data[i], new string(' ', fieldsLength[i] - data[i].Length));

                Ln(4, outString);
            }
        }

        private static void Ln(int tabs = 0, string value = "")
        {
            var strs = value.Split("\n");

            foreach (var s in strs)
                Console.WriteLine(new string(' ', tabs * 4) + s);
        }

        private static void WriteExampleCaption(string caption) => 
            Ln(1, $"Пример \"{caption}\"");

        private static void WriteQuery(string query)
        {
            var lines = query.Split('\n');

            int CommentStart = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].TrimEnd();

                int pos = lines[i].IndexOf("--");
                if (pos > CommentStart)
                    CommentStart = pos;
            }

            string outCode = string.Empty;
            for (int i = 0; i < lines.Length; i++)
            {
                int pos = lines[i].IndexOf("--");

                if (pos != -1)
                {
                    string ns = lines[i][..pos] + new string(' ', CommentStart - pos) + lines[i][pos..];
                    outCode += ns + "\r\n";
                }
            }

            Ln();
            Ln();
            Ln(2, "// Рассматриваемый запрос:");
            Ln(2, outCode);
        }

        private static void WriteAnalyzeResult(DSQLAnalyzer inAnalyzeResult)
        {
            Ln();
            Ln();
            Ln(2, "// Результат работы анализатора запроса (древо основной группы):");
            LnMVGRoup(3, inAnalyzeResult.GetRootGroupCopy(), 0);
        }

        private static void WriteQueryVariant(string caption, string info, MVGroup mvgroup)
        {
            Ln();
            Ln();
            Ln(2, $"{caption}:");
            Ln(2, $"// {info}");
            LnMVGRoup(3, mvgroup, 0);
        }

        private static void WriteResultingQuery(string query, List<(string name, SysAction action)> @params = null)
        {
            Ln();
            Ln(3, "Окончательный запрос:");
            Ln(3, query);

            if (@params is not null && @params.Count > 0)
            {
                Ln();
                Ln(3, "Параметры запроса:");
                foreach (var (name, action) in @params)
                    Ln(4, string.Format("param: {0} = \"{1}\"", name, action.Value.Data));  
            }
        }

        private static void LnMVGRoup(int tabs, MVGroup group, int level)
        {
            Ln(tabs, $"> Group \"{group.DevName}\" level: {level} GroupUIDX: {group.UIndex}");

            if (group.DSQLGroupMarkers != null)
                foreach (var m in group.DSQLGroupMarkers)
                    LnDSQLMarker(tabs + 1, group, m);

            if (group.Actions != null)
                foreach (var a in group.Actions)
                    LnAction(tabs + 1, a);

            if (group.OrderingList != null)
                foreach (var ord in group.OrderingList)
                    LnOrderInfo(tabs + 1, ord);

            foreach (var subgroup in group.Subgroups)
                LnMVGRoup(tabs + 1, subgroup, level + 1);
        }

        private static void LnDSQLMarker(int tabs, MVGroup group, DSQLMarker marker)
        {
            var markerActions = group.GetMarkerActions(marker);

            if (markerActions.Any())
                Ln(tabs, $"Marker {marker.Name} is controlled by variables:");
            else
                Ln(tabs, $"Marker {marker.Name} is {(marker.Enabled ? "enabled" : "disabled")}");

            foreach (var a in markerActions)
                LnAction(tabs + 1, a);
        }

        private static void LnAction(int tabs, SysAction action)
        {
            Ln(tabs, $"Action {action.DestName} is {(action.Value.Enabled ? "enabled" : "disabled")}");
        }

        private static void LnOrderInfo(int tabs, OrderingInfo orderingInfo)
        {
            if (orderingInfo.UseInitialSort)
                Ln(tabs, $"ORDER BY 1 SKIP: {orderingInfo.SkipCount} ONPAGE: {orderingInfo.OnPageCount}");
            else
            {
                string flds = string.Empty;

                foreach (var fld in orderingInfo.OrderingFields)
                    flds += string.Format(", [{0}].[{1}] {2}", fld.TableAlias, fld.FieldName, fld.OrderDESC ? "DESC" : string.Empty);

                if (flds.Length > 0)
                    flds = flds[1..].Trim();
                else
                    flds = "1";

                Ln(tabs, $"ORDER BY {flds} SKIP: {orderingInfo.SkipCount} ONPAGE: {orderingInfo.OnPageCount}");
            }
        }
    }


    internal static class Extensions
    {
        public static void ClearValue(this SysAction @this)
        {
            @this.Value.Data = null;
            @this.Value.Enabled = false;
            @this.SubValues?.Clear();
        }
    }
}
