using System.Collections.Generic;
using System.Linq;

namespace Api.Extensions
{
    public static class BookedRoomExtensions
    {
        public static List<KeyValuePair<string, string>> ToNormilizedRemarks(this List<KeyValuePair<string, string>> remarks)
        {
            remarks.RemoveAll(r => r.Key.Equals("Amenities"));
            remarks.RemoveAll(r => r.Key.Equals("Rate Plan Description"));
            remarks.RemoveAll(r => r.Key.Equals("Check in and check out"));

            var roomRateDescriptions = remarks.Where(r => r.Key.Equals("Room Rate Description"));
            if (roomRateDescriptions != default)
            {
                var listMealPlan = new List<KeyValuePair<string, string>>();

                foreach (var item in roomRateDescriptions)
                {
                    listMealPlan.Add(new KeyValuePair<string, string>("Meal Plan", item.Value));
                }

                if (listMealPlan.Count > 0)
                    remarks.AddRange(listMealPlan);
            }

            remarks.RemoveAll(r => r.Key.Equals("Room Rate Description"));

            return remarks;
        }
    }
}