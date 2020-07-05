using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Witanra.DeepStack.Models;

namespace Witanra.DeepStack
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var files = Directory.GetFiles("Samples");
            foreach (var file in files)
            {
                detectFace(file).Wait();
            }
        }

        private static HttpClient client = new HttpClient();

        public static async Task detectFace(string filename)
        {
            var request = new MultipartFormDataContent();
            var image_data = File.OpenRead(filename);
            request.Add(new StreamContent(image_data), "image", Path.GetFileName(filename));
            var output = await client.PostAsync("http://localhost:8080/v1/vision/detection", request);
            var jsonString = await output.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<DeepStackResponse>(jsonString);

            var saved_Filename = "Output" + filename;
            Save_Image_with_Result(filename, response, saved_Filename);

            foreach (var user in response.predictions)
            {
                Console.WriteLine(user.label);
            }

            Console.WriteLine(jsonString);
        }

        private static void Save_Image_with_Result(string orginal_Filename, DeepStackResponse response, string saved_Filename)
        {
            Bitmap bitmap = (Bitmap)Image.FromFile(orginal_Filename);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                foreach (var predection in response.predictions)
                {
                    Pen pen = new Pen(Brushes.Red);

                    Rectangle rect = new Rectangle(predection.x_min, predection.y_min, predection.x_max - predection.x_min, predection.y_max - predection.y_min);
                    graphics.DrawRectangle(pen, rect);

                    graphics.DrawString(predection.label, new Font("Arial", 16), Brushes.Red, new Point(predection.x_min, predection.y_min));
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(saved_Filename));
            bitmap.Save(saved_Filename, ImageFormat.Jpeg);
        }
    }
}