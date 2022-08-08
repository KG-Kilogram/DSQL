using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

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
                - DSQLMarker                <==
                - OrderingInfo
                    - DSQLFieldInfo

        Х SyntaxAnalyzer (static class)
        Х Hash (static class)     
    */


    /// <summary>
    ///     ћаркер - единственна€ управл€юща€ команда DSQL скрипта.
    ///     “.е. DSQL скрипт - это набор маркеров.
    /// </summary>
    public class DSQLMarker
    {
        /*
         
        */
        
        [JsonConstructor]
        internal DSQLMarker(string name, DSQLMarkerCmd cmd, string markerActionNames, bool isGroupManaged)
        {
            Name = name;
            Cmd = cmd;
            MarkerActionNames = markerActionNames;
            IsGroupManaged = isGroupManaged; 
        }

        internal DSQLMarker(string name, DSQLMarkerCmd cmd)
            : this(name, cmd, string.Empty, false)
        { }


        /*
         
        */

        [Browsable(true)]
        [ReadOnly(true)]
        [JsonProperty("Name")]
        public string Name { get; internal set; }

        public enum DSQLMarkerCmd
        {
            Empty = 0,
            Activate = 1, // $A_
            Deactivate = 9, // $D_
            OrderingBlock = 13, // $O_
            BeginMV = 14, // $BV
            EndMV = 15, // $EV
            EndOfWhere = 16, // $EW
        }

        [JsonProperty("Cmd")]
        public DSQLMarkerCmd Cmd { get; internal set; }

        public bool IsADMarker => Cmd == DSQLMarkerCmd.Activate ||
            Cmd == DSQLMarkerCmd.Deactivate;

        public bool HasActions => !string.IsNullOrEmpty(MarkerActionNames);

        [Browsable(true)]
        public bool Enabled { get; set; }

        [Browsable(true)]
        [ReadOnly(true)]
        public bool IsGroupManaged { get; internal set; } 





        [Browsable(false)]
        [JsonProperty("MarkerActionNames")]
        private string MarkerActionNames { get; set; } 

        public string[] GetActions() => MarkerActionNames.Split(new string[] { "#" }, 
            System.StringSplitOptions.RemoveEmptyEntries);

        public bool HasActionName(string actionDestName)
            => MarkerActionNames?.Contains(string.Format("#{0}#", actionDestName)) ?? false;

        public void AddActionName(string actionDestName)
        {
            if (actionDestName.Trim() == string.Empty)
                return;

            if (MarkerActionNames == null)
                MarkerActionNames = string.Empty;

            if (!HasActionName(actionDestName))
                MarkerActionNames += string.Format("#{0}#", actionDestName);
        }

        public static bool ActionsIsEqual(DSQLMarker markerA, DSQLMarker markerB) =>
            markerA.MarkerActionNames == markerB.MarkerActionNames;







        internal DSQLMarker GetCopy()
        {
            return new DSQLMarker(Name, Cmd, MarkerActionNames, IsGroupManaged)
            {
                Enabled = Enabled
            };
        }

        internal static IEnumerable<(string name, DSQLMarkerCmd cmd)> GetMarkerInfo(string marker)
        {
            string commandName = marker.Substring(0, 3);
            string markerName = string.Empty;

            if (marker.Length > 3)
                markerName = marker.Substring(3).Trim();

            switch (commandName)
            {
                case "$2V":
                    yield return (markerName, DSQLMarkerCmd.BeginMV);
                    yield return ("", DSQLMarkerCmd.EndMV);
                    yield break;

                case "$EW":
                    yield return (string.Format("{0}", markerName), DSQLMarkerCmd.EndOfWhere);
                    yield break;

                case "$A_":
                    yield return (string.Format("{0}", markerName), DSQLMarkerCmd.Activate);
                    yield break;

                case "$D_":
                    yield return (string.Format("{0}", markerName), DSQLMarkerCmd.Deactivate);
                    yield break;

                case "$BV":
                    yield return (markerName, DSQLMarkerCmd.BeginMV);
                    yield break;

                case "$EV":
                    yield return (string.Empty, DSQLMarkerCmd.EndMV);
                    yield break;

                case "$O_":
                    yield return (string.Format("{0}", markerName), DSQLMarkerCmd.OrderingBlock);
                    yield break;

                default:
                    yield return (string.Format("{0}", markerName), DSQLMarkerCmd.Empty);
                    yield break;
            }
        }

        internal int GetHash()
        {
            int hash = Hash.GetDeterministicHash(Name, (int)Cmd);
            hash ^= Hash.GetDeterministicHash(MarkerActionNames);

            return hash;
        }
    }
}