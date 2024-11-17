using System.Net;
using System.Text;

if (args.Length is not 3)
{
    Console.WriteLine("Неверное количество аргументов.");
    Console.WriteLine($"Использование: {AppDomain.CurrentDomain.FriendlyName} входной-файл выходной-файл тип-преобразования (route/firewall)");
    Console.WriteLine($"Пример: {AppDomain.CurrentDomain.FriendlyName} ip.txt ip.rsc firewall");
    return;
}

if (args[2] is not ("route" or "firewall"))
{
    Console.WriteLine("Неизвестный тип преобразования");
    return;
}

var inputFile = args[0];
var outputFile = args[1];

if (!File.Exists(inputFile))
{
    Console.WriteLine("Входной файл не найден");
    return;
}

if (File.Exists(outputFile))
{
    Console.Write("Выходной файл уже существует. Перезаписать? (Y/N): ");
    switch (Console.ReadKey().Key)
    {
        case ConsoleKey.Y:
            break;
        default:
            return;
    }
}

var content = File.ReadAllLines(inputFile);
var ips = new List<string>();
foreach (var line in content)
{
    if (IPAddress.TryParse(line.Trim(), out IPAddress? ip))
    {
        ips.Add(line.Trim());
        continue;
    }

    if (IPNetwork.TryParse(line.Trim(), out IPNetwork net))
    {
        ips.Add(line.Trim());
        continue;
    }
}

Console.WriteLine($"Найдено {ips.Count} адресов");

var sb = new StringBuilder();
switch (args[2])
{
    case "route":
        Console.Write("Шлюз: ");
        var gateway = Console.ReadLine();
        sb.Append($"/ip route{Environment.NewLine}");
        foreach (var ip in ips)
        {
            sb.Append($"add dst-address={ip}/32 gateway={gateway}{Environment.NewLine}");
        }
        break;
    case "firewall":
        Console.Write("Имя списка: ");
        var list = Console.ReadLine();
        sb.Append($"/ip firewall address-list{Environment.NewLine}");
        foreach (var ip in ips)
        {
            sb.Append($"add address={ip} list={list}{Environment.NewLine}");
        }
        break;
}
File.WriteAllText(outputFile, sb.ToString());