using Agenix.Validation.Json.Json;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Json;

public class JsonPathUtilsTest : AbstractNUnitSetUp
{
    private readonly string _jsonSource = @"{
                              'Stores': [
                                'Lambton Quay',
                                'Willis Street'
                              ],
                              'Manufacturers': [
                                {
                                  'Name': 'Acme Co',
                                  'Products': [
                                    {
                                      'Name': 'Anvil',
                                      'Price': 50
                                    }
                                  ]
                                },
                                {
                                  'Name': 'Contoso',
                                  'Products': [
                                    {
                                      'Name': 'Elbow Grease',
                                      'Price': 99.95
                                    },
                                    {
                                      'Name': 'Headlight Fluid',
                                      'Price': 4
                                    }
                                  ]
                                }
                              ]
                            }";

    [Test]
    public void TestEvaluateAsString()
    {
        const string keySetOfManufacturersExpression = "$.Manufacturers[?(@.Name == 'Acme Co')].KeySet()";
        ClassicAssert.AreEqual("Name, Products",
            JsonPathUtils.EvaluateAsString(_jsonSource, keySetOfManufacturersExpression));

        const string oneSpecificManufacturerExpression = "$.Manufacturers[?(@.Name == 'Acme Co')]";
        ClassicAssert.AreEqual(
            "{\"Name\":\"Acme Co\",\"Products\":[{\"Name\":\"Anvil\",\"Price\":50}]}",
            JsonPathUtils.EvaluateAsString(_jsonSource, oneSpecificManufacturerExpression));

        const string manufacturersSize = "$.Manufacturers.Size()";
        ClassicAssert.AreEqual("2",
            JsonPathUtils.EvaluateAsString(_jsonSource, manufacturersSize));

        const string listOfProductNamesWherePriceGreaterThanThree = "$..Products[?(@.Price > 3)].Name";
        ClassicAssert.AreEqual("Anvil, Elbow Grease, Headlight Fluid",
            JsonPathUtils.EvaluateAsString(_jsonSource, listOfProductNamesWherePriceGreaterThanThree));
    }
}
