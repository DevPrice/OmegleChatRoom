namespace OmegleChat.Command
{
    public class CommandInvoker
    {
        public void Execute(ICommand command)
        {
            command.Execute();
        }
    }
}
