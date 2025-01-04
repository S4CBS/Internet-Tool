using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

class Program
{
    // Метод для включения или выключения интерфейса через netsh
    public static void SetInterfaceStatus(string interfaceName, bool enable)
    {
        var action = enable ? "enable" : "disable";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"interface set interface \"{interfaceName}\" {action}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        process.WaitForExit();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        if (process.ExitCode == 0)
        {
            Console.WriteLine($"Интерфейс '{interfaceName}' успешно {action}.");
        }
        else
        {
            Console.WriteLine($"Ошибка: {error}");
        }
    }

    // Метод для получения активного подключения или отключенного интерфейса
    public static string GetInterfaceName()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces();

        // Отображаем все интерфейсы для отладки
        Console.WriteLine("Сетевые интерфейсы:");
        foreach (var ni in interfaces)
        {
            Console.WriteLine($"Имя: {ni.Name}, Статус: {ni.OperationalStatus}, Тип: {ni.NetworkInterfaceType}");
        }

        // Находим первый интерфейс, который либо активен, либо отключён
        var targetInterface = interfaces
            .FirstOrDefault(ni =>
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback && // Исключаем Loopback
                ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&  // Исключаем Tunnel
                ni.OperationalStatus == OperationalStatus.Up ||           // Либо активен
                ni.OperationalStatus == OperationalStatus.Down);          // Либо отключён

        if (targetInterface != null)
        {
            Console.WriteLine($"Обнаружен интерфейс: {targetInterface.Name}, Статус: {targetInterface.OperationalStatus}");
            return targetInterface.Name;
        }

        Console.WriteLine("Нет подходящих сетевых интерфейсов.");
        return null;
    }

    static void Main()
    {
        Console.WriteLine("Программа для управления интернет-соединением:");
        Console.WriteLine("Нажмите '1' для выключения текущего подключения.");
        Console.WriteLine("Нажмите '2' для включения текущего подключения.");
        Console.WriteLine("Нажмите 'Esc' для выхода.");

        // Получение интерфейса динамически
        string targetInterfaceName = GetInterfaceName();

        if (string.IsNullOrEmpty(targetInterfaceName))
        {
            Console.WriteLine("Не удалось найти подходящий интерфейс.");
            return;
        }

        // Обработка ввода с клавиатуры
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.D1: // Клавиша '1' выключает интерфейс
                    SetInterfaceStatus(targetInterfaceName, false);
                    break;

                case ConsoleKey.D2: // Клавиша '2' включает интерфейс
                    SetInterfaceStatus(targetInterfaceName, true);
                    break;

                case ConsoleKey.Escape: // Клавиша 'Esc' завершает программу
                    Console.WriteLine("Выход из программы.");
                    return;

                default:
                    Console.WriteLine("Нажмите '1', '2' или 'Esc'.");
                    break;
            }
        }
    }
}