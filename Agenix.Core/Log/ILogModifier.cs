namespace Agenix.Core.Log
{
    /**
     * Modifier masks output that gets printed to an output stream. Usually used
     * to mask sensitive data like passwords and secrets when printed to the logger output.
     */
    public interface ILogModifier
    {
        /**
         * Mask given logger statement and apply custom modifications before
         * the logger is printed to the output stream.
         * @param statement
         * @return
         */
        string Mask(string statement);
    }
}