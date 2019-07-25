/*
 * A very simple interface that forces you to write an execute method
 * for every subclass that implements this interface.
 * 
 * An interface, in it's simplest form, says "hey, don't forget to do this"
 * 
 */
namespace WPFTrek.Controllers
{
    interface ControllerInterface
    {
        bool Execute();
    }
}
