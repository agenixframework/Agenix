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
        Assert.That(JsonPathUtils.EvaluateAsString(_jsonSource, keySetOfManufacturersExpression),
            Is.EqualTo("Name, Products"));

        const string oneSpecificManufacturerExpression = "$.Manufacturers[?(@.Name == 'Acme Co')]";
        Assert.That(
            JsonPathUtils.EvaluateAsString(_jsonSource, oneSpecificManufacturerExpression),
            Is.EqualTo("{\"Name\":\"Acme Co\",\"Products\":[{\"Name\":\"Anvil\",\"Price\":50}]}"));

        const string manufacturersSize = "$.Manufacturers.Size()";
        Assert.That(JsonPathUtils.EvaluateAsString(_jsonSource, manufacturersSize),
            Is.EqualTo("2"));

        const string listOfProductNamesWherePriceGreaterThanThree = "$..Products[?(@.Price > 3)].Name";
        Assert.That(JsonPathUtils.EvaluateAsString(_jsonSource, listOfProductNamesWherePriceGreaterThanThree),
            Is.EqualTo("Anvil, Elbow Grease, Headlight Fluid"));
    }
}
