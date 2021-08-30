using System;

using vtCore;
using vtCore.Implementation;
using vtCore.Interfaces;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;

namespace tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var solution = Factory.makeSolution();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters ={
                    new JsonStringEnumConverter()
                }
            };

            string json = JsonSerializer.Serialize(solution, options);
            File.WriteAllText("solution.json", json);


        }
    }
}
