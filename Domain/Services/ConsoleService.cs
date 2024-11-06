using DiscretionaryAccessControl.Domain.Interfaces;

using Spectre.Console;

namespace DiscretionaryAccessControl.Domain.Services
{
    public class ConsoleService : IConsoleService
    {
        private static readonly IDataService _dataService;

        private static bool _showHelp = true;
        private static bool _unknownCommandException = false;

        static ConsoleService()
        {
            _dataService = new DataService();
        }

        public static void Init()
        {
            while (true)
            {
                if (_unknownCommandException)
                {
                    TitleUnknownCommand();
                }
                else
                {
                    Title(13);
                }

                if (ReadCommand() == 0)
                {
                    return;
                }

                AnsiConsole.Clear();
            }
        }

        private static int ReadCommand()
        {
            string name = Program.User?.Login ?? "unauthorized";
            string command = AnsiConsole.Ask<string>($"[green]{name}:[/]");

            string[] lines = command
                .ToLower()
                .Trim()
                .Split(' ');

            if (lines.Length == 0)
            {
                throw new ApplicationException("Critical error!");
            }

            switch (lines[0])
            {
                case "help":
                    Help();
                    return 1;
                case "login":
                    LogIn();
                    return 1;
                case "logout":
                    LogOut();
                    return 1;
                case "list":
                    ObjectsList();
                    return 1;
                case "add":
                    AddObject();
                    return 1;
                case "read":
                    Read(lines);
                    return 1;
                case "edit":
                    Edit(lines);
                    return 1;
                case "adduser":
                    AddUser();
                    return 1;
                case "users":
                    UsersList();
                    return 1;
                case "exit":
                    return Exit();
                default:
                    _unknownCommandException = true;
                    return 1;
            }
        }

        private static void Title(int textHeight)
        {
            AnsiConsole.Write(new FigletText("Discretionary")
                .Centered()
                .Color(Color.Yellow));

            AnsiConsole.Write(new FigletText("Access Control")
                .Centered()
                .Color(Color.Yellow));

            int height;
            if (_showHelp)
            {
                height = Console.WindowHeight - textHeight - 2;
            }
            else
            {
                height = Console.WindowHeight - textHeight;
            }

            if (height < 0)
            {
                height = 0;
            }

            for (int i = 0; i < height; i++)
            {
                AnsiConsole.WriteLine();
            }

            if (_showHelp)
            {
                AnsiConsole.MarkupLine("[gray]Type 'help' to see available commands.[/]");
                AnsiConsole.WriteLine();
                _showHelp = !_showHelp;
            }
        }

        private static void TitleUnknownCommand()
        {
            AnsiConsole.Write(new FigletText("Discretionary")
                .Centered()
                .Color(Color.Yellow));

            AnsiConsole.Write(new FigletText("Access Control")
                .Centered()
                .Color(Color.Yellow));

            int height = Console.WindowHeight - 17;

            if (height < 0)
            {
                height = 0;
            }

            for (int i = 0; i < height; i++)
            {
                AnsiConsole.WriteLine();
            }

            AnsiConsole.MarkupLine("[gray]Type 'help' to see available commands.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[red]Unknown command! Type 'help' to see available commands.[/]");
            AnsiConsole.WriteLine();

            _unknownCommandException = !_unknownCommandException;
        }

        private static void Help()
        {
            AnsiConsole.Clear();

            List<string> commands = new()
            {
                "[green]help[/]\t\t-   Show available commands",
                "[green]login[/]\t\t-   Enter login interface",
                "[green]logout[/]\t\t-   Logout from account",
                "[green]list[/]\t\t-   View objects list",
                "[green]users[/]\t\t-   View users list",
                "[green]add[/]\t\t-   Add new object",
                "[green]read {name}[/]\t-   Read object data",
                "[green]edit {name}[/]\t-   Edit object data",
                "[green]adduser[/]\t\t-   Add new user",
                "[green]exit[/]\t\t-   Exit the application"
            };

            Title(14 + commands.Count);

            AnsiConsole.MarkupLine("[cyan]Available commands:[/]");

            foreach (string command in commands)
            {
                AnsiConsole.MarkupLine(command);
            }

            Console.ReadLine();
        }

        private static int Exit()
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[yellow]Exiting...[/]");
            return 0;
        }

        private static void Exception(Exception ex)
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(new FigletText("Discretionary")
                .Centered()
                .Color(Color.Yellow));

            AnsiConsole.Write(new FigletText("Access Control")
                .Centered()
                .Color(Color.Yellow));

            int height = Console.WindowHeight - 14;

            if (height < 0)
            {
                height = 0;
            }

            for (int i = 0; i < height; i++)
            {
                AnsiConsole.WriteLine();
            }

            AnsiConsole.MarkupLine("[red]An error occurred: {0}[/]", [ex.Message]);
            Console.ReadLine();
        }

        private static void LogIn()
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(new FigletText("Discretionary")
                .Centered()
                .Color(Color.Yellow));

            AnsiConsole.Write(new FigletText("Access Control")
                .Centered()
                .Color(Color.Yellow));

            int height = Console.WindowHeight - 14;

            if (height < 0)
            {
                height = 0;
            }

            for (int i = 0; i < height; i++)
            {
                AnsiConsole.WriteLine();
            }

            string login = AnsiConsole.Ask<string>("Enter your [green]login[/]:");

            string password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter your [green]password[/]:")
                    .PromptStyle("red")
                    .Secret()
            );

            try
            {
                _dataService.LogIn(login, password);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private static void LogOut()
        {
            try
            {
                _dataService.LogOut();
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private static void ObjectsList()
        {
            try
            {
                var objects = _dataService.ObjectsList();

                Table table = new Table()
                    .Centered()
                    .Title("Objects")
                    .AddColumn(new TableColumn(new Markup($"[white]№[/]").Centered()))
                    .AddColumn(new TableColumn(new Markup($"[white]Name[/]").Centered()))
                    .AddColumn(new TableColumn(new Markup($"[white]Id[/]").Centered()))
                    .AddColumn(new TableColumn(new Markup($"[white]Length[/]").Centered()));

                table.Border = TableBorder.Rounded;
                table.BorderColor(Color.Yellow);

                int index = 0;
                int textHeight = 18;

                foreach (string obj in objects)
                {
                    string[] lines = obj.Split('\t');

                    if (lines.Length != 3)
                    {
                        throw new ApplicationException("Critical error!");
                    }

                    table.AddRow(
                        new Markup($"[silver]{++index}[/]").Centered(),
                        new Markup($"[silver]{lines[0]}[/]").Centered(),
                        new Markup($"[silver]{lines[1]}[/]").Centered(),
                        new Markup($"[silver]{lines[2]}[/]").Centered());

                    textHeight += 2;
                }

                AnsiConsole.Clear();
                Title(textHeight);
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
                ReadCommand();
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private static void AddObject()
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(new FigletText("Discretionary")
                .Centered()
                .Color(Color.Yellow));

            AnsiConsole.Write(new FigletText("Access Control")
                .Centered()
                .Color(Color.Yellow));

            int height = Console.WindowHeight - 18;

            if (height < 0)
            {
                height = 0;
            }

            for (int i = 0; i < height; i++)
            {
                AnsiConsole.WriteLine();
            }

            string name = AnsiConsole.Ask<string>("Enter [green]name[/]:");

            string permission = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose [green]permission[/]:")
                    .PageSize(3)
                    .MoreChoicesText("[grey](Move up and down for chose permission)[/]")
                    .AddChoices(new[] {
                        "Read",
                        "Write",
                        "RootOnly",
                    }));

            string data = AnsiConsole.Ask<string>("Enter [green]data[/]:");

            try
            {
                _dataService.AddObject(name, permission, data);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private static void Read(string[] lines)
        {
            try
            {
                if (lines.Length != 2)
                {
                    throw new ArgumentException("Object name is empty");
                }

                string data = _dataService.ReadObject(lines[1]);

                AnsiConsole.Clear();
                Title(13);
                AnsiConsole.Write(new Markup($"[silver]{data}[/]"));
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private static void Edit(string[] lines)
        {
            try
            {
                if (lines.Length != 2)
                {
                    throw new ArgumentException("Object name is empty");
                }

                AnsiConsole.Clear();
                Title(13);
                string data = AnsiConsole.Ask<string>("Enter new [green]data[/]:");
                _dataService.EditObject(lines[1], data);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private static void AddUser()
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(new FigletText("Discretionary")
                .Centered()
                .Color(Color.Yellow));

            AnsiConsole.Write(new FigletText("Access Control")
                .Centered()
                .Color(Color.Yellow));

            int height = Console.WindowHeight - 18;

            if (height < 0)
            {
                height = 0;
            }

            for (int i = 0; i < height; i++)
            {
                AnsiConsole.WriteLine();
            }

            string login = AnsiConsole.Ask<string>("Enter [green]login[/]:");

            string password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter your [green]password[/]:")
                    .PromptStyle("red")
                    .Secret()
            );

            string permission = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose [green]permission[/]:")
                    .PageSize(3)
                    .MoreChoicesText("[grey](Move up and down for chose permission)[/]")
                    .AddChoices(new[] {
                        "User",
                        "Observer",
                    }));

            try
            {
                _dataService.AddUser(login, password, permission);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }

        private static void UsersList()
        {
            try
            {
                var users = _dataService.UsersList();

                Table table = new Table()
                    .Centered()
                    .Title("Users")
                    .AddColumn(new TableColumn(new Markup($"[white]№[/]").Centered()))
                    .AddColumn(new TableColumn(new Markup($"[white]Login[/]").Centered()))
                    .AddColumn(new TableColumn(new Markup($"[white]Id[/]").Centered()))
                    .AddColumn(new TableColumn(new Markup($"[white]Permission[/]").Centered()));

                table.Border = TableBorder.Rounded;
                table.BorderColor(Color.Yellow);

                int index = 0;
                int textHeight = 18;

                foreach (string user in users)
                {
                    string[] lines = user.Split('\t');

                    if (lines.Length != 3)
                    {
                        throw new ApplicationException("Critical error!");
                    }

                    table.AddRow(
                        new Markup($"[silver]{++index}[/]").Centered(),
                        new Markup($"[silver]{lines[0]}[/]").Centered(),
                        new Markup($"[silver]{lines[1]}[/]").Centered(),
                        new Markup($"[silver]{lines[2]}[/]").Centered());

                    textHeight += 2;
                }

                AnsiConsole.Clear();
                Title(textHeight);
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
                ReadCommand();
            }
            catch (Exception ex)
            {
                Exception(ex);
            }
        }
    }
}