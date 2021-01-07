using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyTennisPartner.Models.Users;

namespace MyTennisPartner.Web.Test
{
    [TestClass]
    public class ApplicationUserTests
    {
        [TestMethod]
        public void CanCreateApplicationUser()
        {
            var user = new ApplicationUser
            {
                // test that our added properties are there on the extended IdentityUser class
                FirstName = "John",
                LastName = "Smith",
            };
            Assert.AreEqual("John", user.FirstName);
        }
    }
}
