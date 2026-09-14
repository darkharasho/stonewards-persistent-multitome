using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace PersistentMultiTome
{
    /// <summary>
    /// Every upgrade shared during the current run, in order. Lets players who join the run for the first time get
    /// everything shared before they arrived. No game types, so it can be unit tested.
    /// </summary>
    internal sealed class ShareLedger
    {
        internal readonly struct Entry
        {
            public readonly ulong PickerID;
            public readonly string UpgradeID;
            public readonly int Rarity;

            public Entry(ulong pickerID, string upgradeID, int rarity)
            {
                PickerID = pickerID;
                UpgradeID = upgradeID;
                Rarity = rarity;
            }
        }

        private const string Header = "# PersistentMultiTome shared upgrades";

        private readonly List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        public void Add(Entry entry) => entries.Add(entry);

        public void Clear() => entries.Clear();

        /// <summary>Entries a player joining the run for the first time should be given.</summary>
        public IEnumerable<Entry> BacklogFor(ulong playerID)
        {
            foreach (var entry in entries)
            {
                if (entry.PickerID != playerID) yield return entry;
            }
        }

        /// <summary>Text form, tagged with the save it belongs to so a stale file is never applied to another save.</summary>
        public string Serialize(long saveStamp)
        {
            var text = new StringBuilder();
            text.Append(Header).Append('\n');
            text.Append("save ").Append(saveStamp.ToString(CultureInfo.InvariantCulture)).Append('\n');
            foreach (var entry in entries)
            {
                text.Append(entry.PickerID.ToString(CultureInfo.InvariantCulture)).Append('\t')
                    .Append(entry.Rarity.ToString(CultureInfo.InvariantCulture)).Append('\t')
                    .Append(entry.UpgradeID).Append('\n');
            }
            return text.ToString();
        }

        /// <summary>
        /// Replaces the entries with those in <paramref name="text"/>. Returns false, leaving the ledger empty, if the
        /// text is malformed or was written for a different save.
        /// </summary>
        public bool TryLoad(string text, long saveStamp)
        {
            entries.Clear();
            if (text == null) return false;

            var loaded = new List<Entry>();
            using (var reader = new StringReader(text))
            {
                if (reader.ReadLine() != Header) return false;
                var save = reader.ReadLine();
                if (save == null || !save.StartsWith("save ", StringComparison.Ordinal)
                    || !long.TryParse(save.Substring(5), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stamp)
                    || stamp != saveStamp)
                {
                    return false;
                }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0) continue;
                    var parts = line.Split(new[] { '\t' }, 3);
                    if (parts.Length != 3 || parts[2].Length == 0
                        || !ulong.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var picker)
                        || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var rarity))
                    {
                        return false;
                    }
                    loaded.Add(new Entry(picker, parts[2], rarity));
                }
            }

            entries.AddRange(loaded);
            return true;
        }
    }
}
