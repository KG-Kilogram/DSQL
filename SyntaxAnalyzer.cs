using System;
using System.Collections.Generic;
using System.Linq;

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
                - OrderingInfo
                    - DSQLFieldInfo

        • SyntaxAnalyzer (static class)     <==
        • Hash (static class)     
    */

    /*
            Схема работы SyntaxAnalyzer  
     
            1. Разделение скрипта на строки;
            2. Разделение строк на слова и определение типов слов;
            3. Поиск $маркеров и @переменных и создание списков маркеров
            и переменных для каждой строки;

            4. Установка явных зависимостей для $A_ и $D_ маркеров от переменных.
            Если строка содержит $A_ и/или $D_ маркеры и @переменные запроса,
            то активность маркеров строки ставится в зависимость от наличия
            значения этих переменных;

            5. @sys переменные убираются из рассмотрения и интерпретируются
            как обычная комнда SQL, а не как переменная;
    */

    internal static class SyntaxAnalyzer
    {
        public static IEnumerable<Line> Analyze(string query)
        {
            string[] queryLines = query.Split('\n');

            for (int i = 0; i < queryLines.Length; i++)
                queryLines[i] = queryLines[i].TrimEnd();

            foreach (string queryLine in queryLines)
            {
                if (string.IsNullOrEmpty(queryLine))
                    continue;

                var line = new Line();

                string currentWord = queryLine[0].ToString();
                Word.DSQLWordType currentWordType = Word.GetTypeByChar(queryLine[0]);

                for (int i = 1; i < queryLine.Length; i++)
                    switch (queryLine[i])
                    {
                        case ' ':
                            switch (currentWordType)
                            {
                                case Word.DSQLWordType.Space:
                                    currentWord += queryLine[i];
                                    break;

                                default:
                                    line.Words.Add(new Word(currentWord, currentWordType));
                                    currentWord = queryLine[i].ToString();
                                    currentWordType = Word.GetTypeByChar(queryLine[i]);
                                    break;
                            }
                            break;

                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            switch (currentWordType)
                            {
                                case Word.DSQLWordType.Integer:
                                case Word.DSQLWordType.Float:
                                case Word.DSQLWordType.SqlVar:
                                    currentWord += queryLine[i];
                                    break;

                                case Word.DSQLWordType.Dot:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.Float;
                                    break;

                                case Word.DSQLWordType.Word:
                                    currentWord += queryLine[i];
                                    break;

                                case Word.DSQLWordType.ExponentialFloatIncomplite1:
                                case Word.DSQLWordType.ExponentialFloatIncomplite2:
                                case Word.DSQLWordType.ExponentialFloat:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.ExponentialFloat;
                                    break;

                                default:
                                    line.Words.Add(new Word(currentWord, currentWordType));
                                    currentWord = queryLine[i].ToString();
                                    currentWordType = Word.GetTypeByChar(queryLine[i]);
                                    break;
                            }
                            break;

                        case '.':
                            switch (currentWordType)
                            {
                                case Word.DSQLWordType.Integer:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.Float;
                                    break;

                                default:
                                    line.Words.Add(new Word(currentWord, currentWordType));
                                    currentWord = queryLine[i].ToString();
                                    currentWordType = Word.GetTypeByChar(queryLine[i]);
                                    break;
                            }
                            break;

                        case '=':
                            switch (currentWordType)
                            {
                                case Word.DSQLWordType.GreatreThen:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.GreaterOrEqual;
                                    break;

                                case Word.DSQLWordType.LessThen:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.LessOrEqual;
                                    break;

                                default:
                                    line.Words.Add(new Word(currentWord, currentWordType));
                                    currentWord = queryLine[i].ToString();
                                    currentWordType = Word.GetTypeByChar(queryLine[i]);
                                    break;
                            }
                            break;

                        case '>':
                            switch (currentWordType)
                            {
                                case Word.DSQLWordType.LessThen:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.NotEqual;
                                    break;

                                default:
                                    line.Words.Add(new Word(currentWord, currentWordType));
                                    currentWord = queryLine[i].ToString();
                                    currentWordType = Word.GetTypeByChar(queryLine[i]);
                                    break;
                            }
                            break;

                        case '(':
                        case ')':
                        case '[':
                        case ']':
                        case '{':
                        case '}':
                        case (char)039:
                        case '"':
                        case ':':
                        case ';':
                        case ',':
                        case '#':
                        case '`':
                            line.Words.Add(new Word(currentWord, currentWordType));
                            currentWord = queryLine[i].ToString();
                            currentWordType = Word.GetTypeByChar(queryLine[i]);
                            break;

                        case 'E': // latin
                        case 'e': // latin
                            switch (currentWordType)
                            {
                                case Word.DSQLWordType.Integer:
                                case Word.DSQLWordType.Float:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.ExponentialFloatIncomplite1;
                                    break;

                                case Word.DSQLWordType.Word:
                                case Word.DSQLWordType.SqlVar:
                                case Word.DSQLWordType.CommercialAt:
                                    currentWord += queryLine[i];
                                    break;

                                default:
                                    line.Words.Add(new Word(currentWord, currentWordType));
                                    currentWord = queryLine[i].ToString();
                                    currentWordType = Word.GetTypeByChar(queryLine[i]);
                                    break;
                            }
                            break;

                        case '+':
                            switch (currentWordType)
                            {
                                case Word.DSQLWordType.ExponentialFloatIncomplite1:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.ExponentialFloatIncomplite2;
                                    break;

                                default:
                                    line.Words.Add(new Word(currentWord, currentWordType));
                                    currentWord = queryLine[i].ToString();
                                    currentWordType = Word.GetTypeByChar(queryLine[i]);
                                    break;
                            }
                            break;

                        case '-':
                            switch (currentWordType)
                            {
                                case Word.DSQLWordType.ExponentialFloatIncomplite1:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.ExponentialFloatIncomplite2;
                                    break;

                                case Word.DSQLWordType.Minus:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.DoubleMinus;
                                    break;

                                default:
                                    line.Words.Add(new Word(currentWord, currentWordType));
                                    currentWord = queryLine[i].ToString();
                                    currentWordType = Word.GetTypeByChar(queryLine[i]);
                                    break;
                            }
                            break;

                        case '*':
                            switch (currentWordType)
                            {
                                case Word.DSQLWordType.Division:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.StartInlineComment;
                                    break;

                                default:
                                    line.Words.Add(new Word(currentWord, currentWordType));
                                    currentWord = queryLine[i].ToString();
                                    currentWordType = Word.GetTypeByChar(queryLine[i]);
                                    break;
                            }
                            break;

                        case '/':
                            switch (currentWordType)
                            {
                                case Word.DSQLWordType.Multiply:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.StopInlineComment;
                                    break;

                                default:
                                    line.Words.Add(new Word(currentWord, currentWordType));
                                    currentWord = queryLine[i].ToString();
                                    currentWordType = Word.GetTypeByChar(queryLine[i]);
                                    break;
                            }
                            break;

                        default:
                            switch (currentWordType)
                            {
                                case Word.DSQLWordType.Word:
                                    currentWord += queryLine[i];
                                    break;

                                case Word.DSQLWordType.CommercialAt:
                                case Word.DSQLWordType.SqlVar:
                                    currentWord += queryLine[i];
                                    currentWordType = Word.DSQLWordType.SqlVar;
                                    break;

                                default:
                                    line.Words.Add(new Word(currentWord, currentWordType));
                                    currentWord = queryLine[i].ToString();
                                    currentWordType = Word.GetTypeByChar(queryLine[i]);
                                    break;
                            }
                            break;
                    }

                line.Words.Add(new Word(currentWord, currentWordType));

                bool commentDetected = false;
                foreach (var word in line.Words)
                {
                    if (word.Type == Word.DSQLWordType.DoubleMinus)
                        commentDetected = true;

                    line.LineData += word.WordData;

                    if (!commentDetected)
                    {
                        line.LineStart += word.WordData;

                        if (word.WordData.ToLower().Trim() == "where")
                            line.IsWhereBeginner = true;
                    }
                    else if (word.WordData.ToLower().Trim() == "$a_syslimit")
                        line.HasSysLimitMarker = true;
                }

                ProcessSysParams(line);
                ProcessLineMarkers(line);
                ProcessLineActions(line);

                line.HasUpConditionWord = line.Words.Exists(w => w.WordData.Trim().ToLower() != "where" &&
                    Array.Exists(Word.UpConditionWordTypes, wt => wt == w.Type));

                yield return line;
            }
        }

        /// <summary>
        ///     Метод формирования списков Actions строки и/или маркеров строки    
        /// </summary>
        private static void ProcessLineActions(Line line)
        {
            foreach (var word in line.Words)
            {
                if (word.Type != Word.DSQLWordType.SqlVar)
                    continue;

                string userVarClearedName = word.WordData;
                if (userVarClearedName[0] == '@')
                    userVarClearedName = userVarClearedName.Substring(1);

                if (line.HasActivateMarker || line.HasDeactivateMarker)
                {
                    if (!line.HasActivateMarker)
                        line.Actions.Add(userVarClearedName);

                    foreach (var mrk in line.Markers.Where(m => m.Cmd == DSQLMarker.DSQLMarkerCmd.Activate 
                        || m.Cmd == DSQLMarker.DSQLMarkerCmd.Deactivate))
                        mrk.AddActionName(userVarClearedName);
                }
                else
                    line.Actions.Add(userVarClearedName);
            }
        }

        /// <summary>
        ///     Расчет параметров маркеров     
        /// </summary>
        private static void ProcessLineMarkers(Line line)
        {
            bool inlineComment = false;

            foreach (var word in line.Words)
                if (inlineComment)
                {
                    if (word.WordData != string.Empty && word.WordData[0] == '$')
                        foreach (var (name, cmd) in DSQLMarker.GetMarkerInfo(word.WordData).ToList())
                        {
                            if (line.Markers == null)
                                line.Markers = new List<DSQLMarker>();

                            line.Markers.Add(new DSQLMarker(name, cmd));
                        }
                }
                else if (word.Type == Word.DSQLWordType.DoubleMinus)
                    inlineComment = true;

            if (line.Markers != null)
            {
                line.HasActivateMarker = line.Markers.Any(e => e.Cmd == DSQLMarker.DSQLMarkerCmd.Activate);
                line.HasDeactivateMarker = line.Markers.Any(e => e.Cmd == DSQLMarker.DSQLMarkerCmd.Deactivate);
                line.IsEndOfWhere = line.Markers.Any(e => e.Cmd == DSQLMarker.DSQLMarkerCmd.EndOfWhere);

                var marker = line.Markers.Find(e => e.Cmd == DSQLMarker.DSQLMarkerCmd.OrderingBlock);
                if (marker != null)
                    line.OrderingMarker = marker.Name;
            }
        }

        /// <summary>
        ///     Метод изменения типа слова с wt_user_var на wt_word для параметров запроса, начинающихся на @sys
        /// </summary>
        private static void ProcessSysParams(Line line)
        {
            for (int i = 0; i < line.Words.Count; i++)
            {
                var word = line.Words[i];
                if (word.WordData.StartsWith("@sys"))
                {
                    word.Type = Word.DSQLWordType.Word;
                    line.Words[i] = word;
                }
            }
        }
    }
}
