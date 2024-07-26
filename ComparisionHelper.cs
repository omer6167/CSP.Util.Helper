using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSP.Util.Helper
{

    public static class ComparisionHelper
    {


        public static List<MemberComparisonResult> CompareObjectsAndGetDifferences<T>(T firstObj, T secondObj)
        {
            var differenceInfoList = new List<MemberComparisonResult>();

            foreach (var member in typeof(T).GetMembers())
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    var property = (PropertyInfo)member;
                    if (property.CanRead && property.GetGetMethod().GetParameters().Length == 0)
                    {
                        var xValue = property.GetValue(firstObj, null);
                        var yValue = property.GetValue(secondObj, null);
                        if (!object.Equals(xValue, yValue))
                            differenceInfoList.Add(new MemberComparisonResult(property.Name, xValue, yValue));
                    }
                    else
                        continue;
                }
            }
            return differenceInfoList;
        }

        public static List<MemberComparisonResult> CompareJsonDifferencess(string firstJson, string secondJson)
        {
            var differenceInfoList = new List<MemberComparisonResult>();

            JObject sourceJObject = JsonConvert.DeserializeObject<JObject>(firstJson);
            JObject targetJObject = JsonConvert.DeserializeObject<JObject>(secondJson);

            if (!JToken.DeepEquals(sourceJObject, targetJObject))
            {
                foreach (KeyValuePair<string, JToken> sourceProperty in sourceJObject)
                {
                    JProperty targetProp = targetJObject.Property(sourceProperty.Key);

                    if (!JToken.DeepEquals(sourceProperty.Value, targetProp.Value))
                    {
                        differenceInfoList.Add(new MemberComparisonResult(sourceProperty.Key, sourceProperty.Value, targetProp.Value));
                    }
                }
            }

            return differenceInfoList;
        }

    }

    public class MemberComparisonResult
    {
        public string Name { get; }
        public object FirstValue { get; }
        public object SecondValue { get; }

        public MemberComparisonResult(string name, object firstValue, object secondValue)
        {
            Name = name;
            FirstValue = firstValue;
            SecondValue = secondValue;
        }

        public override string ToString()
        {
            return Name; //+ " : " + FirstValue.ToString() + (FirstValue.Equals(SecondValue) ? " == " : " != ") + SecondValue.ToString()
        }
    }
}
