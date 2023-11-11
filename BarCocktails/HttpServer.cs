using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Numerics;
using System.Xml.Linq;
using System.Net.Mail;
using System.Linq;
using System.Text.RegularExpressions;

namespace BarCocktails
{
    public class AppSettings
    {
        public int Port { get; set; }
        public string Address { get; set; }
        public string StaticFilePath { get; set; }
        public string EmailAdress { get; set; }
        public string EmailPassword { get; set; }
    }

    public class FormAnswer
    {
        public string City { get; set; }
        public string WorkAdress { get; set; }
        public string Profession { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Birthday { get; set; }
        public string Phone { get; set; }
        public string NWLink { get; set; }

        public FormAnswer(string City, string WorkAdress, string Profession, string Name, string Surname, string Birthday, string Phone, string NWLink) 
        {
            this.City = City;
            this.WorkAdress = WorkAdress;
            this.Profession = Profession;
            this.Name = Name;
            this.Surname = Surname;
            this.Birthday = Birthday;
            this.Phone = Phone;
            this.NWLink = NWLink;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(City); sb.Append(" ");
            sb.Append(WorkAdress); sb.Append(" ");
            sb.Append(Profession); sb.Append(" ");
            sb.Append(Name); sb.Append(" ");
            sb.Append(Surname); sb.Append(" ");
            sb.Append(Birthday); sb.Append(" ");
            sb.Append(Phone); sb.Append(" ");
            sb.Append(NWLink); 
            return sb.ToString();
        }
    }

    public class HttpServer
    {
        private HttpListener server { get; set; }
        private AppSettings appSettings { get; set; }
        private bool stop {  get; set; }

        private static MyORM myORM = new MyORM();

        public HttpServer()
        {
            this.server =new HttpListener();
            stop = false;
            string appSettingsPath = @".\\appsetting.json";
            if (File.Exists(appSettingsPath))
            {
                var json = File.ReadAllText(appSettingsPath);
                appSettings = JsonSerializer.Deserialize<AppSettings>(json);
                var prefix = $"{appSettings.Address}:{appSettings.Port}/home/";
                Console.WriteLine($"Connected {prefix}");
                server.Prefixes.Add(prefix);
            }
            else
            {
                Console.WriteLine("Can not open the file \"appseting.json\"!");
                stop = true;
                return;
            }

        }

        async Task RequestHandler()
        {
            var context = await server.GetContextAsync();
            var request = context.Request;
            var response = context.Response;
            string url = (request.RawUrl).Substring(6);
            if (url == "")
            {
                url="index.html";
            }
            else if (url.Substring(0,10) == "sendEmail?")
            {
                string text = url.Substring(10);
                Console.WriteLine(text);
                string[] ans = text.Split('&','=');
                //City = A & WorkAdress = A & Profession = A & Name = A & Surname = A & Birthday = A & Phone = A & NWLink = A
                FormAnswer form = new FormAnswer(ans[1], ans[3], ans[5], ans[7], ans[9], ans[11], ans[13], ans[15]);
                Console.WriteLine(form);
                SendEmailAsync(appSettings,form).GetAwaiter();
                url = "index.html";
            }

            string filePath = appSettings.StaticFilePath+url;
            Console.WriteLine(url);
            //if (request.RawUrl == null)
            //else { filePath = request.RawUrl; }
            
            if (File.Exists(filePath))
            {
                Console.WriteLine("Finded source file");
                string responseText = CreateHTMl();
                byte[] buffer = Encoding.UTF8.GetBytes(responseText);
                //byte[] buffer = File.ReadAllBytes(filePath);

                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                await output.WriteAsync(buffer,0,buffer.Length);
                await output.FlushAsync();
                Console.WriteLine("Request processed");
            }
            else
            {
                Console.WriteLine("Can not find source file");
            }
        }
        async private Task<string[]> ParseRequest(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
                return Array.Empty<string>();

            var stream = new StreamReader(request.InputStream);
            var requestData = await stream.ReadToEndAsync();
            requestData = Uri.UnescapeDataString(Regex.Unescape(requestData));
            requestData = requestData.Replace("&", "\n");
            requestData = requestData.Replace("=", ": ");
            requestData = requestData.Replace("+", " ");
            var array = requestData.Split('\n', ':').ToArray();

            var classData = array.Where(val => Array.IndexOf(array, val) % 2 == 1).ToArray();
            return classData;
        }

        public string CreateHTMl()//object[] objects)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html>
<html>
<head>
    <title>BarTable</title>
</head>
<body>
    <table>
        <tbody>");
            for(int i=1;i<10; i++) 
            {
                object obj = myORM.SelectById<Cocktails>(i);
                sb.Append(obj.ToString()); 
            }
            sb.Append(@"
        </tbody>
    </table>
</body>
</html>");
            //StreamWriter sw = new StreamWriter($"..\\..\\static\\index.html");
            //sw.Write(sb.ToString());
            //sw.Close();
            return sb.ToString();
        }

        private static async Task SendEmailAsync(AppSettings appSettings, FormAnswer form)
        {
            MailAddress from = new MailAddress(appSettings.EmailAdress, "Klim");
            MailAddress to = new MailAddress("mklim04@mail.ru");
            MailMessage m = new MailMessage(from, to);
            //m.Attachments.Add(new Attachment(""));
            m.Subject = "Homework Form";
            m.Body = form.ToString();
            SmtpClient smtp = new SmtpClient("smtp.mail.com", 465);
            smtp.Credentials = new NetworkCredential(appSettings.EmailAdress, appSettings.EmailPassword);
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(m);
            Console.WriteLine("Email Sended");
        }

        public async Task StartServer()
        {
            string staticPath =appSettings.StaticFilePath;
            server.Start();
            Console.WriteLine("Server started, write \"stop\" to stop server.");

            Task.Run(() =>
            {
                while (true)
                {
                    string consoleInput = Console.ReadLine();
                    if (consoleInput == "stop")
                    {
                        Console.WriteLine("Stopping.");
                        stop = true;
                        server.Stop();
                        break;
                    }
                }
            });

            if (Directory.Exists(staticPath))
            {

                Console.WriteLine("find directory");
               while(!stop)
                {
                    await RequestHandler();
                }
            }
            else
            {
                Console.WriteLine("Can not find folder \"static\"!");
                Console.WriteLine("Creating folder \"static\".");
                Directory.CreateDirectory(staticPath);
            }
            //server.Stop();
            Console.WriteLine("Server stopped.");
            //Console.ReadLine();
        }
    }
}