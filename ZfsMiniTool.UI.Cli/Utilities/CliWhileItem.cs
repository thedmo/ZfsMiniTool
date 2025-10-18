namespace ZfsMiniTool.UI.Cli.Utilities;
public class CliWhileItem
{
    public void WhileYesTry(Action action)
    {
        bool _running = true;

        while (_running)
        {
            try
            {
                action();

                _running = false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);

                Console.WriteLine("Try again? (y = yes / any other Key = abort) ");

                var key = Console.ReadKey();
                if (!key.KeyChar.ToString().ToLower().Equals("y"))
                    _running = false;

                else
                    _running = true;
            }
        }
    }
}
