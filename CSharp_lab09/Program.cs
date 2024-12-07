using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text.Json;

// Класс для хранения данных о биржевых свечах
public class StockData
{
    public string s { get; set; } // Символ финансового инструмента
    public List<double> c { get; set; } // Цены закрытия
    public List<double> h { get; set; } // Максимальные цены
    public List<double> l { get; set; } // Минимальные цены
    public List<double> o { get; set; } // Цены открытия
    public List<int> t { get; set; } // Таймстемпы
    public List<int> v { get; set; } // Объемы торгов
};

class Market
{
    // Мьютекс для синхронизации записи в файл
    static readonly Mutex mutex = new Mutex();

    // Асинхронное чтение файла с тикерами
    static async Task ReadFileAsync(List<string> massive, string filePath)
    {
        using (StreamReader sr = new StreamReader(filePath))
        {
            string line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                massive.Add(line); // Добавляем строку (тикер) в массив
            }
        }
    }

    // Запись данных в файл
    static void WriteToFile(string filePath, string text)
    {
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose(); // Создаем файл, если он не существует
        }

        mutex.WaitOne(); // Захватываем мьютекс
        try
        {
            File.AppendAllText(filePath, text + Environment.NewLine); // Записываем данные в файл
        }
        finally
        {
            mutex.ReleaseMutex(); // Освобождаем мьютекс
        }
    }

    // Вычисление средней цены на основе минимальных и максимальных цен
    static double calculateAvgPrice(List<double> highPrice, List<double> lowPrice)
    {
        double totalAvgPrice = 0;
        for (int i = 0; i < highPrice.Count; i++)
        {
            totalAvgPrice += (highPrice[i] + lowPrice[i]) / 2;
        }
        return totalAvgPrice / highPrice.Count;
    }

    // Получение данных о биржевых свечах через API
    static async Task GetData(HttpClient client, string quote, string startDate, string endDate, string output)
    {
        string apiKey = "bWROMnE5ZmVGVFRjclI0c01qNzNFaUVzYWNGbmc4enIwcmJ6Z0ZXZkVHbz0";
        string URL = $"https://api.marketdata.app/v1/stocks/candles/D/{quote}/?from={startDate}&to={endDate}&token={apiKey}";

        HttpResponseMessage response = await client.GetAsync(URL);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Ошибка! Код состояния: {response.StatusCode}");
        }
        else
        {
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<StockData>(json);

            if (data != null && data.h != null && data.l != null)
            {
                double avgPrice = calculateAvgPrice(data.h, data.l); // Рассчитываем среднюю цену
                string result = $"{quote}: {avgPrice}";
                WriteToFile(output, result); // Записываем результат в файл
                Console.WriteLine($"Средняя цена для {quote}: {avgPrice}");
            }
            else
            {
                Console.WriteLine($"Недостаточно данных для {quote}");
            }
        }
    }

  
    static async Task Main(string[] args)
    {
        // API-ключ для доступа к сервису
        string apiKey = "bWROMnE5ZmVGVFRjclI0c01qNzNFaUVzYWNGbmc4enIwcmJ6Z0ZXZkVHbz0";

        // Пути к файлам с тикерами и результатами
        string tickerPath = "D:\\C#_Labs\\CSharp_lab09\\CSharp_lab09\\Files\\ticker.txt";
        string outputPath = "D:\\C#_Labs\\CSharp_lab09\\CSharp_lab09\\Files\\output.txt";

        List<string> ticker = new List<string>();
        await ReadFileAsync(ticker, tickerPath); // Читаем тикеры из файла

        // Устанавливаем временные интервалы
        DateTime endDateTime = DateTime.Now;
        string endDate = endDateTime.AddMonths(-1).ToString("yyyy-MM-dd");
        string startDate = endDateTime.AddYears(-1).AddMonths(1).ToString("yyyy-MM-dd");

        Console.WriteLine($"Дата начала: {startDate}");
        Console.WriteLine($"Дата окончания: {endDate}");

        // Настраиваем HTTP-клиент
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Запускаем задачи получения данных для каждого тикера
        List<Task> tasks = new List<Task>();
        foreach (var quote in ticker)
        {
            tasks.Add(GetData(client, quote, startDate, endDate, outputPath));
        }

        await Task.WhenAll(tasks); // Дожидаемся завершения всех задач
    }
}
