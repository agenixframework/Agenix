namespace Agenix.Screenplay;

/// <summary>
///  Implement this Interface when you wish an {@link Ability} to be torn down upon calling OnStage.drawTheCurtain()
/// </summary>
public interface IHasTeardown
{
    void TearDown(); 
}