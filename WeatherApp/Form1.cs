using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Policy;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json.Nodes;

namespace WeatherApp
{
    public partial class Form1 : Form
    {
        // Ключ API для доступа к сервису OpenWeatherMap
        private readonly string apiKey = "90f3c903d03c09a887221c3c9993a558";

        // Базовый URL для API погоды
        private readonly string apiUrl = "https://api.openweathermap.org/data/2.5/weather";

        // Путь к файлу, содержащему данные о городах
        private readonly string cityPath = "D:\\C#_Labs\\CSharp_lab09\\CSharp_lab09\\Files\\city.txt";

        // Список для хранения объектов городов, загруженных из файла
        private readonly List<City> _cities = new List<City>();

        // Конструктор формы
        public Form1()
        {
            InitializeComponent();
            // Загрузка городов из файла при инициализации формы
            LoadCitiesAsync(cityPath);
        }

        // Асинхронный метод для загрузки данных о городах из указанного файла
        private async Task LoadCitiesAsync(string filePath)
        {
            try
            {
                // Чтение всех строк из файла
                var cities = File.ReadAllLines(filePath);

                // Обработка каждой строки для создания объектов городов
                foreach (var city in cities)
                {
                    var parts = city.Split('\t');
                    if (parts.Length == 2)
                    {
                        // Разделение координат города
                        var name = parts[0];
                        var coord = parts[1].Replace(" ", "").Split(',');
                        var latitude = Convert.ToDouble(coord[0].Replace(".", ","));
                        var longitude = Convert.ToDouble(coord[1].Replace(".", ","));

                        // Создание объекта города
                        var info = new City(name, latitude, longitude);
                        _cities.Add(info);
                    }
                }

                // Привязка списка городов к выпадающему списку на форме
                CityComboBox.DataSource = _cities;
            }
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке при загрузке городов
                MessageBox.Show($"Ошибка загрузки городов: {ex.Message}");
            }
        }

        // Обработчик нажатия на кнопку "Получить погоду"
        private async void GetWeatherButton_Click(object sender, EventArgs e)
        {
            if (CityComboBox.SelectedItem is City selectedCity)
            {
                try
                {
                    // Получение данных о погоде для выбранного города
                    var weather = await FetchWeatherAsync(apiUrl, selectedCity);

                    if (weather != null)
                    {
                        // Отображение данных о погоде в текстовом поле
                        ResulttextBox.Text = weather.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось получить данные о погоде. Попробуйте позже.");
                    }
                }
                catch (Exception ex)
                {
                    // Вывод сообщения об ошибке при получении данных
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
            else
            {
                // Сообщение, если город не выбран
                MessageBox.Show("Пожалуйста, выберите город.");
            }
        }

        // Асинхронный метод для получения данных о погоде через API
        private async Task<Weather> FetchWeatherAsync(string URL, City city)
        {
            try
            {
                // Создание HTTP-клиента для выполнения запросов к API
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(URL)
                };

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Формирование параметров URL-запроса
                var urlParameters = $"?lat={city.Latitude}&lon={city.Longitude}&appid={apiKey}&units=metric";
                var fullUrl = URL + urlParameters;

                // Выполнение GET-запроса к API
                var response = await client.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Парсинг ответа API в формате JSON
                    var responseString = await response.Content.ReadAsStringAsync();
                    var json = JsonObject.Parse(responseString);

                    // Создание объекта погоды из данных API
                    Weather res = new Weather
                    {
                        Country = (string)json["sys"]["country"],
                        Name = (string)json["name"],
                        Temp = (double)json["main"]["temp"],
                        Description = (string)json["weather"][0]["main"]
                    };

                    return res;
                }
                else
                {
                    MessageBox.Show($"Ошибка API: {response.StatusCode}");
                }
                return null;
            }
            catch (Exception ex)
            {
                // Сообщение об ошибке при получении данных от API
                MessageBox.Show($"Не удалось получить данные о погоде: {ex.Message}");
                return null;
            }
        }
    }

    // Класс для хранения данных о городе
    public class City
    {
        public string Name { get; }
        public double Latitude { get; }
        public double Longitude { get; }

        public City(string name, double latitude, double longitude)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
        }

        // Переопределение метода ToString для отображения имени города
        public override string ToString()
        {
            return Name;
        }
    }

    // Класс для хранения данных о погоде
    public class Weather
    {
        public string Country { get; set; }
        public string Name { get; set; }
        public double Temp { get; set; }
        public string Description { get; set; }

        // Конструктор с параметрами
        public Weather(string country, string name, double temp, string description)
        {
            Country = country;
            Name = name;
            Temp = temp;
            Description = description;
        }

        // Пустой конструктор
        public Weather() { }

        // Переопределение метода ToString для форматированного вывода информации о погоде
        public override string ToString()
        {
            return $"Страна: {Country}, Город: {Name}, Температура: {Temp} °C, Описание: {Description}";
        }
    }
}
