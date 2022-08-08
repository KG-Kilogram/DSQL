namespace DSQL
{
    /*
        ќтношение классов

        Х DSQLAnalyzer (Generator)          
            - MVGroup
                - Line
                    - Word                  <==
                - SysAction
                    - ActionValue           
                - DSQLMarker
                - OrderingInfo
                    - DSQLFieldInfo

        Х SyntaxAnalyzer (static class)
        Х Hash (static class)     
    */

    internal struct Word
    {
        public Word(string word, DSQLWordType type)
        {
            WordData = word;
            Type = type;
        }

        public DSQLWordType Type { get; set; }
        public string WordData { get; set; }

        public enum DSQLWordType
        {
            Empty,                  // [empty string]
            Space,                  // [space]
            Comma,                  // ,
            Dot,                    // .
            Hash,                   // #
            CommercialAt,           // @
            Quote,                  // '
            DoubleQuote,            // "
            OpenRoundBracket,       // (
            OpenSquareBracket,      // [
            OpenCurlyBracket,       // {
            CloseRoundBracket,      // )
            CloseSquareBracket,     // ]
            CloseCurlyBracket,      // }
            Multiply,               // *
            Division,               // /
            Equal,                  // =
            Minus,                  // -
            Plus,                   // +
            GreatreThen,            // >
            LessThen,               // <
            Colon,                  // :
            Semicolon,              // ;

            StartInlineComment,     // /*
            StopInlineComment,      // */
            SysComment,             // #!
            GreaterOrEqual,         // >=
            LessOrEqual,            // <=
            NotEqual,               // <>
            NullSafeEqual,          // <=>
            DoubleMinus,            // --   (sql server comment)

            ExponentialFloatIncomplite1,    // 0.0e   / 0.0E
            ExponentialFloatIncomplite2,    // 0.0e+  / 0.0e-  / 0.0E+  / 0.0E-
            ExponentialFloat,               // 0.0e+0 / 0.0e-0 / 0.0E+0 / 0.0E-0

            Word,
            Integer,
            Float,
            Hex,                    // 0xf3 0xFA
            IntegerOverflow,         
            SqlVar,                 // (sql server @Var)
        }

        /// <summary>
        ///     “ипы слов - признаки наличи€ услови€ 
        /// </summary>
        public static readonly DSQLWordType[] UpConditionWordTypes = new Word.DSQLWordType[]
        {
            DSQLWordType.Word,
            DSQLWordType.Integer,
            DSQLWordType.Float,
            DSQLWordType.Hex,
            DSQLWordType.SqlVar,
        };

        /// <summary>
        ///     ћетод, определ€ющий тип слова по первому символу (при стерте нового слова)
        /// </summary>
        public static DSQLWordType GetTypeByChar(char @char)
        {
            switch (@char)
            {
                case ' ':
                    return DSQLWordType.Space;
                case ',':
                    return DSQLWordType.Comma;
                case '.':
                    return DSQLWordType.Dot;
                case '#':
                    return DSQLWordType.Hash;
                case (char)039:
                    return DSQLWordType.Quote;
                case '"':
                    return DSQLWordType.DoubleQuote;
                case '(':
                    return DSQLWordType.OpenRoundBracket;
                case '[':
                    return DSQLWordType.OpenSquareBracket;
                case '{':
                    return DSQLWordType.OpenCurlyBracket;
                case ')':
                    return DSQLWordType.CloseRoundBracket;
                case ']':
                    return DSQLWordType.CloseSquareBracket;
                case '}':
                    return DSQLWordType.CloseCurlyBracket;
                case '*':
                    return DSQLWordType.Multiply;
                case '/':
                    return DSQLWordType.Division;
                case '=':
                    return DSQLWordType.Equal;
                case '-':
                    return DSQLWordType.Minus;
                case '+':
                    return DSQLWordType.Plus;
                case '>':
                    return DSQLWordType.GreatreThen;
                case '<':
                    return DSQLWordType.LessThen;
                case ':':
                    return DSQLWordType.Colon;
                case ';':
                    return DSQLWordType.Semicolon;
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
                    return DSQLWordType.Integer;
                case '@':
                    return DSQLWordType.CommercialAt;
                default:
                    return DSQLWordType.Word;
            }
        }
    }
}