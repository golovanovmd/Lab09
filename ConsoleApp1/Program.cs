string cityPath = "D:\\C#_Labs\\CSharp_lab09\\CSharp_lab09\\Files\\city.txt";
async Task LoadCitiesAsync(string filePath)
{
    //try
    //{
        var cities = File.ReadAllLines(filePath);
        foreach (var city in cities)
        {
            var parts = city.Split('\t');
            //MessageBox.Show($"city: {parts[0]}, coords: {parts[1]}");
            var name = parts[0];
        
        var coord = parts[1].Split(", ");
        Console.WriteLine($"'{coord[1]}'");
        Console.WriteLine($"Hey ----{Convert.ToDouble(coord[0].Replace(".", ","))}");
        Console.WriteLine($"Hey ----{Convert.ToDouble(coord[1].Replace(".", ","))}");
        Console.WriteLine(city);
        Console.WriteLine(name,coord);
        
        var latitude = Convert.ToDouble(coord[0].Trim());
            var longitude = Convert.ToDouble(coord[1].Trim());
            Console.WriteLine(name, latitude, longitude);
        
        }
    //}
    //catch (Exception ex)
    //{
    //    Console.WriteLine($"Error loading cities: {ex.Message}");
    //}
}
LoadCitiesAsync(cityPath);