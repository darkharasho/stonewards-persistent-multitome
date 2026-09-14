using System.Linq;
using Xunit;

namespace PersistentMultiTome.Tests
{
    public class ShareLedgerTests
    {
        private const ulong Alice = 76561198000000001;
        private const ulong Bob = 76561198000000002;
        private const ulong Carol = 76561198000000003;

        [Fact]
        public void BacklogSkipsUpgradesThePlayerPickedThemselves()
        {
            var ledger = new ShareLedger();
            ledger.Add(new ShareLedger.Entry(Alice, "TOME_DAMAGE", 2));
            ledger.Add(new ShareLedger.Entry(Bob, "TOME_SPEED", 0));
            ledger.Add(new ShareLedger.Entry(Alice, "TOME_DAMAGE", 4));

            Assert.Equal(new[] { "TOME_SPEED" }, ledger.BacklogFor(Alice).Select(e => e.UpgradeID));
            Assert.Equal(3, ledger.BacklogFor(Carol).Count());
        }

        [Fact]
        public void RoundTripsForTheSameSave()
        {
            var ledger = new ShareLedger();
            ledger.Add(new ShareLedger.Entry(Alice, "TOME_DAMAGE", 2));
            ledger.Add(new ShareLedger.Entry(Bob, "ID WITH SPACES", 4));

            var loaded = new ShareLedger();
            Assert.True(loaded.TryLoad(ledger.Serialize(1234), 1234));

            Assert.Equal(2, loaded.Entries.Count);
            Assert.Equal(Bob, loaded.Entries[1].PickerID);
            Assert.Equal("ID WITH SPACES", loaded.Entries[1].UpgradeID);
            Assert.Equal(4, loaded.Entries[1].Rarity);
        }

        [Fact]
        public void IgnoresFileFromAnotherSave()
        {
            var ledger = new ShareLedger();
            ledger.Add(new ShareLedger.Entry(Alice, "TOME_DAMAGE", 2));

            var loaded = new ShareLedger();
            loaded.Add(new ShareLedger.Entry(Carol, "OLD", 0));
            Assert.False(loaded.TryLoad(ledger.Serialize(1234), 999));
            Assert.Empty(loaded.Entries);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("garbage")]
        [InlineData("# PersistentMultiTome shared upgrades\nsave 1234\nnot-a-number\t2\tTOME\n")]
        [InlineData("# PersistentMultiTome shared upgrades\nsave 1234\n1\t2\n")]
        public void RejectsMalformedText(string text)
        {
            var loaded = new ShareLedger();
            Assert.False(loaded.TryLoad(text, 1234));
            Assert.Empty(loaded.Entries);
        }
    }

    public class ShareRulesTests
    {
        [Theory]
        //          pickup  serverWide  pickups rings  serverWide  expected
        [InlineData(true,   false,      true,   false, false,      true)]
        [InlineData(true,   false,      false,  true,  false,      false)]
        [InlineData(false,  false,      true,   false, false,      false)]
        [InlineData(false,  false,      false,  true,  false,      true)]
        [InlineData(true,   true,       true,   true,  false,      false)]
        [InlineData(true,   true,       true,   false, true,       true)]
        [InlineData(false,  true,       true,   false, true,       false)]
        public void FollowsSettings(bool isPickup, bool isServerWide, bool sharePickups, bool shareRings, bool shareServerWide, bool expected)
        {
            Assert.Equal(expected, ShareRules.ShouldShare(isPickup, isServerWide, sharePickups, shareRings, shareServerWide));
        }
    }
}
