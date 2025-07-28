namespace Agenix.Screenplay.Tests.Shopping.Questions;

public abstract class ThePrice
{
    public static IQuestion<int> Total()
    {
        return IQuestion<int>.About("the total price").AnsweredBy(_ => 100);
    }

    public static IQuestion<int> Vat()
    {
        return IQuestion<int>.Create(_ => 20);
    }

    public static IQuestion<int> Vat(bool throwError)
    {
        return IQuestion<int>.Create(_ =>
            {
                if (throwError)
                {
                    throw new SystemException("Oh crap!");
                }

                return 20;
            }
        );
    }

    public static IQuestion<int> TotalWithVat()
    {
        return IQuestion<int>.Create(_ => 120);
    }
}
