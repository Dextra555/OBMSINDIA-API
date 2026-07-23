namespace OBMS.WebAPI.Utility
{
    public class Utility
    {

        public static List<string> GetStateList()
        {
            List<string> StateList = new List<string>();
            StateList.Add("Tamil Nadu");
            StateList.Add("Andhra Pradesh");
            StateList.Add("Karnataka");
            StateList.Add("Kerala");
            StateList.Add("Maharashtra");
            StateList.Add("Telangana");
            StateList.Add("Delhi");
            StateList.Add("Gujarat");
            StateList.Add("Rajasthan");
            StateList.Add("Uttar Pradesh");
            StateList.Add("Madhya Pradesh");
            StateList.Add("Punjab");
            StateList.Add("Haryana");
            StateList.Add("West Bengal");
            StateList.Add("Bihar");
            StateList.Add("Odisha");
            StateList.Add("Assam");
            StateList.Add("Jharkhand");
            StateList.Add("Chhattisgarh");
            StateList.Add("Uttarakhand");
            StateList.Add("Himachal Pradesh");
            StateList.Add("Goa");
            StateList.Add("Jammu and Kashmir");
            return StateList;
        }
        public static List<string> GetNationalityList()
        {
            List<string> NationalityList = new List<string>();
            NationalityList.Add("Nepal");
            return NationalityList;
        }
        public static List<string> GetICColorList()
        {
            List<string> ICColorList = new List<string>();
            ICColorList.Add("Not Applicable");
            ICColorList.Add("Blue");
            return ICColorList;
        }
        public static List<string> GetRaceList()
        {
            List<string> RaceList = new List<string>();
            RaceList.Add("Chinese");
            RaceList.Add("Indian");
            RaceList.Add("Malay");
            RaceList.Add("Others");
            return RaceList;

        }

        public static Dictionary<string, string> GetCompnay()
        {
            var results = new Dictionary<string, string>();

            results.Add("ShortName", "FWG");

            return results;
        }
    }
}
