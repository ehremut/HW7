using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using Microsoft.EntityFrameworkCore.Storage;

namespace HW7
{
    class Program
    {
        static Context context = new Context();
        static private bool isRunning = true;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Registration");
            do
            {
                Console.WriteLine("Input name");
                var name = Input();
                if (CheckInBase(name))
                {
                    Console.WriteLine("Input country");
                    var country = Input();
                    Console.WriteLine("Input city");
                    var city = Input();
                    
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "C# App");
                        
                        var countryEncode = HttpUtility.UrlEncode(country).ToLower();
                        var cityEncode = HttpUtility.UrlEncode(city).ToLower();
                        var apiString = $"https://nominatim.openstreetmap.org/search?country={countryEncode}&city={cityEncode}&format=jsonv2";
                        HttpResponseMessage pointsResponse =
                            await client.GetAsync(apiString);
                        try
                        {
                            if (pointsResponse.IsSuccessStatusCode)
                            {
                                List<Root> roots = await pointsResponse.Content.ReadFromJsonAsync<List<Root>>();
                                if (roots.Count > 0 && roots.First().type == "city")
                                {
                                    Console.WriteLine($"Find {roots.First().type} {roots.First().display_name}");
                                    await Register(name, country, city);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Name is exist!"); 
                }
                
                Console.WriteLine("Select command: again or exit");
                var command = "";
                do
                {
                    command = Console.ReadLine();
                    if (command == "exit")
                    {
                        isRunning = false;
                    }
                } while (command != "exit" && command != "again");
                 
            } while (isRunning);
        }
        
        static private string Input()
        {
            string input = "";
            do
            {
                input = Console.ReadLine();
            } while (input.Length < 1);
            return input;
        }

        static async Task Register(string name, string country, string city)
        {
            using (Context context = new Context())
            {
                IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    User newUser = new User()
                    {
                        Name = name,
                        City = city,
                        Country = country
                    };
                    context.Users.Add(newUser);
                    await transaction.CommitAsync();
                    await context.SaveChangesAsync();
                    Console.WriteLine("Success register");
                }
                catch
                {
                    Console.WriteLine("Failed register");
                }
            }
        }

        static bool CheckInBase(string name)
        {
            IQueryable<User> users = from user in context.Users
                where (user.Name == name)
                select user;
            if (users.Count() == 0)
            {
                return true;
            }
            return false;
        }
    }
}