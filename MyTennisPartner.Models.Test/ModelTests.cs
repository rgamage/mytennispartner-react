using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyTennisPartner.Models.Utilities;
using MyTennisPartner.Models.Enums;

namespace MyTennisPartner.Models.Test
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        public void CanPadLeftString()
        {
            var input = "8";
            var output = StringHelper.ZeroPadLeft(input);
            Assert.AreEqual("008", output);

            input = "82";
            output = StringHelper.ZeroPadLeft(input);
            Assert.AreEqual("082", output);

            input = "823";
            output = StringHelper.ZeroPadLeft(input);
            Assert.AreEqual("823", output);

            input = "B";
            output = StringHelper.ZeroPadLeft(input);
            Assert.AreEqual("B", output);
        }

        [TestMethod]
        public void CanGetFullCourtSize()
        {
            var size = LeagueHelper.FullCourtSize(PlayFormat.MensSingles);
            Assert.AreEqual(2, size);

            size = LeagueHelper.FullCourtSize(PlayFormat.SinglesPractice);
            Assert.AreEqual(2, size);

            size = LeagueHelper.FullCourtSize(PlayFormat.WomensSingles);
            Assert.AreEqual(2, size);

            size = LeagueHelper.FullCourtSize(PlayFormat.MensDoubles);
            Assert.AreEqual(4, size);

            size = LeagueHelper.FullCourtSize(PlayFormat.WomensDoubles);
            Assert.AreEqual(4, size);

            size = LeagueHelper.FullCourtSize(PlayFormat.MixedDoubles);
            Assert.AreEqual(4, size);

            size = LeagueHelper.FullCourtSize(PlayFormat.GroupLesson);
            Assert.AreEqual(10, size);

            size = LeagueHelper.FullCourtSize(PlayFormat.PrivateLesson);
            Assert.AreEqual(1, size);
        }
    }
}
