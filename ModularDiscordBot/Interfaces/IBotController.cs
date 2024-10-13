namespace ModularDiscordBot.Interfaces;


/// <summary>
/// A labeling interface for classes that will be automatically added to the collection through the reflection mechanism.
/// </summary>
/// <remarks>
/// Any class that implements this interface will be found through reflection and added to the collection.
/// For example, this interface can be used for service registration.
/// </remarks>
public interface IBotController;