using System;

namespace MyTennisPartner.Data.Test.Seeding
{
    public class RandomUserResults
    {
        public RandomUser[] Results { get; set; }
        public RandomUserInfo  info { get; set; }
    }

    public class RandomUserInfo
    {
        public string seed { get; set; }
        public int results { get; set; }
        public int page { get; set; }
        public string version { get; set; }
    }

    public class RandomUserName
    {
        public string title { get; set; }
        public string first { get; set; }
        public string last { get; set; }
    }

    public class RandomUserAddress
    {
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public int postcode { get; set; }
    }

    public class RandomUserLogin
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class RandomUserPicture
    {
        public string large { get; set; }
        public string medium { get; set; }
        public string thumbnail { get; set; }
    }

    public class RandomUser
    {
        public string gender { get; set; }
        public RandomUserName name { get; set; }
        public RandomUserAddress location { get; set; }
        public string email { get; set; }
        public RandomUserLogin login { get; set; }
        public DateTime dob { get; set; }
        public string phone { get; set; }
        public RandomUserPicture picture { get; set; }
        public bool IsAdmin { get; set; }
    }
}
