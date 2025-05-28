using Agenix.Api.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using Spring.Data.Core;
using static Agenix.Sql.Actions.ExecuteSqlQueryAction.Builder;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Sql.Integration;

[NUnitAgenixSupport]
[AgenixConfiguration(Classes = [typeof(Endpoints)])]
public class ExecuteSqlIT
{
    [Test]
    public void ExecuteSqlAction()

    {
        var adoTemplate = _context.ReferenceResolver.Resolve<AdoTemplate>("AdoTemplate");
        var scriptSqlFile = _context.ReferenceResolver.Resolve<string>("scriptSqlFile");
        var queryScriptSqlFile = _context.ReferenceResolver.Resolve<string>("queryScriptSqlFile");

        _runner.Given(CreateVariables().Variable("customerId", "1").Variable("rowsCount", "0"));

        _runner.Given(global::Agenix.Sql.Actions.ExecuteSqlAction.Builder.Sql()
            .AdoTemplate(adoTemplate)
            .SqlResource(scriptSqlFile));

        _runner.When(
            Query()
                .AdoTemplate(adoTemplate)
                .Statement("select NAME from CUSTOMERS where CUSTOMER_ID='${customerId}'")
                .Statement("select COUNT(1) as overall_cnt from ERRORS")
                .Statement("select ORDER_ID from ORDERS where DESCRIPTION LIKE 'Migrate%'")
                .Statement("select DESCRIPTION from ORDERS where ORDER_ID = 2")
                .Validate("ORDER_ID", "1")
                .Validate("NAME", "Christoph")
                .Validate("OVERALL_CNT", "${rowsCount}")
                .Validate("DESCRIPTION", "")
        );
        _runner.When(
            Query()
                .AdoTemplate(adoTemplate)
                .SqlResource(queryScriptSqlFile)
                .Validate("ORDER_ID", "1")
                .Validate("NAME", "Christoph")
                .Validate("OVERALL_CNT", "${rowsCount}")
                .Validate("DESCRIPTION", "")
        );

        _runner.When(
            Query()
                .AdoTemplate(adoTemplate)
                .Statement("select REQUEST_TAG as RTAG, DESCRIPTION as DESC from ORDERS")
                .Validate("RTAG", "requestTag", "@Ignore@")
                .Validate("DESC", "Migrate")
                .Validate("DESC", "Migrate", "")
                .Extract("RTAG", "tags")
                .Extract("DESC", "description")
        );

        _runner.When(global::Agenix.Sql.Actions.ExecuteSqlAction.Builder.Sql()
            .AdoTemplate(adoTemplate)
            .Statement("DELETE FROM CUSTOMERS"));

        _runner.Then(Query()
            .AdoTemplate(adoTemplate)
            .Statement("select DESCRIPTION as desc from ORDERS where ORDER_ID = 2")
            .Validate("DESC", "")
        );
    }
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _runner;

    [AgenixResource] private TestContext _context;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}
