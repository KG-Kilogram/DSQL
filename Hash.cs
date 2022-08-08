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

        • SyntaxAnalyzer (static class)
        • Hash (static class)               <==
    */

    internal static class Hash
    {
        private static int GetDeterministicHash(string @string)
        {
            unchecked
            {
                int hash = 0b1010101010101010101010101010101;

                foreach (var b in System.Text.Encoding.ASCII.GetBytes(@string))
                    hash ^= b << (b % 24);

                return hash;
            }
        }

        public static int GetDeterministicHash(params object[] args)
        {
            int hash = 0;
            
            foreach (var o in args)
                hash ^= GetDeterministicHash(o?.ToString() ?? string.Empty);
            
            return hash;           
        }
    }
}