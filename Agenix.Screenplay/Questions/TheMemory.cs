namespace Agenix.Screenplay.Questions;

/// <summary>
/// Represents a question in the Screenplay pattern to verify the presence of a specific memory
/// associated with an <see cref="Actor"/>.
/// </summary>
public class TheMemory : IQuestion<bool>
{
    private readonly string _memoryKey;

    private TheMemory(string memoryKey)
    {
        _memoryKey = memoryKey;
    }

    public bool AnsweredBy(Actor actor)
    {
        return actor.Recall<dynamic>(_memoryKey) != null;
    }

    /// <summary>
    /// Provides a builder for creating instances of <see cref="TheMemory"/> questions.
    /// </summary>
    public class TheMemoryQuestionBuilder(string memoryKey)
    {
        public TheMemory IsPresent()
        {
            return new TheMemory(memoryKey);
        }
    }

    /// <summary>
    /// Initializes a builder to create a memory question with the specified memory key.
    /// </summary>
    /// <param name="memoryKey">The key to identify the memory to be questioned.</param>
    /// <returns>A <see cref="TheMemoryQuestionBuilder"/> instance initialized with the specified memory key.</returns>
    public static TheMemoryQuestionBuilder WithKey(string memoryKey)
    {
        return new TheMemoryQuestionBuilder(memoryKey);
    }
}
