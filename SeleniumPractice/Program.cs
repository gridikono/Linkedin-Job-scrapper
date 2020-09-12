
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using HtmlAgilityPack;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.IO;
using System.Reflection;

namespace LinkedinScrapper
{
    public class Post
    {
        public string Name { get; set; }
    }

    public static class MyExtensions
    {
        public static bool ContainsSubstring(this string str, string compareValue)
        {
            const int charsToCompare = 5;
            var subString = compareValue.Substring(0, Math.Min(charsToCompare, compareValue.Length));
            if (str.Contains(subString))
            {
                return true;
            }
            else if (compareValue.Length > charsToCompare)
            {
                return str.ContainsSubstring(compareValue.Substring(1));
            }
            return false;
        }

        class Program
        {
            static void WriteOutPutInFile(ArrayList names)
            {
                try
                {
                    //string Path = "C:\\Users\\User\\source\\repos\\SeleniumPractice\\SeleniumPractice\\results\\";
                    String Path = getPathFromDB();
                    System.IO.StreamWriter crlFile;
                    string crlId = Guid.NewGuid().ToString();
                    string extension = ".txt";

                    string FullPath = Path + crlId + extension;
                    if (!System.IO.File.Exists(FullPath))
                    {
                        crlFile = new System.IO.StreamWriter(FullPath);
                    }
                    else
                    {
                        crlFile = System.IO.File.AppendText(FullPath);
                    }
                    var distArr = names.ToArray().Distinct();
                    foreach (var item in distArr)
                    {
                        crlFile.WriteLine(item);
                    }

                    crlFile.WriteLine("___________________________________________");
                    crlFile.WriteLine("Crawled date: " + DateTime.Now);
                    crlFile.WriteLine("\n");
                    crlFile.WriteLine(distArr.Count() + " result(s)");
                    // Close the stream:
                    crlFile.Close();


                }

                catch (Exception e)
                {
                    throw e;
                }
            }


            static string getPathFromDB()
            {
                string statusReturned = "";
                string connectionString = @"Data Source=mysql.frostyserver.com; Initial Catalog=gridi;User Id=gridi_user;Password=bs@Apxi4jq";


                using (MySqlConnection sqlConnection2 = new MySqlConnection(connectionString))
                {
                    string query = "select Path from Path";
                    MySqlCommand sqlComm2 = new MySqlCommand(query, sqlConnection2);
                    try
                    {
                        sqlConnection2.Open();
                        var returnValue = sqlComm2.ExecuteScalar();
                            if(returnValue != null)
                               statusReturned = returnValue.ToString();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                return statusReturned;
            }


            static void Main(string[] args)
            {
                Console.WriteLine("==================================LINKEDIN SCRAPPER================================\n");
                Console.Write("Enter Keyword: ");
                string kw = Console.ReadLine();

                Console.WriteLine("Your input: {0}", kw);

                Console.Write("Enter Number of scrolling down times (enter a number > 50 and < 450): ");
                string pgNum = Console.ReadLine();

                Console.Write("Enter Your Linkedin Email: ");
                string username = Console.ReadLine();
                ////////////////////////////////////////////////////////////////////////////////////



                string pass = "";
                Console.Write("Enter Your Linkedin Password: ");
                ConsoleKeyInfo key;

                do
                {
                    key = Console.ReadKey(true);

                    // Backspace Should Not Work
                    if (key.Key != ConsoleKey.Backspace)
                    {
                        pass += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        pass = pass.Remove(pass.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                // Stops Receving Keys Once Enter is Pressed
                while (key.Key != ConsoleKey.Enter);
                Console.WriteLine("\nLogging...  \n");
                Console.WriteLine("\n==================================================================================");
                Console.WriteLine("Crawl just started!");
                Console.WriteLine("\n==================================================================================\n\n\n");
                IWebDriver driver = new ChromeDriver(@"C:\\geckodriver\\");
                //Starts the driver and loads Wiki Tournament Page
                driver.Url = @"https://www.linkedin.com/uas/login?session_redirect=%2Fvoyager%2FloginRedirect%2Ehtml&amp;fromSignIn=true&amp;trk=cold_join_sign_in";
                driver.FindElement(By.Id("username")).SendKeys(username);
                driver.FindElement(By.Id("password")).SendKeys(pass);
                driver.FindElement(By.CssSelector(".btn__primary--large.from__button--floating")).Submit(); // from__button--floating
                System.Threading.Thread.Sleep(1000);
                driver.Navigate().GoToUrl(@"https://www.linkedin.com/search/results/content/?facetSortBy=date_posted&keywords=" + kw.Trim() + "&origin=SORT_RESULTS");


                //search-results__list list-style-none mt2
                var html = driver.FindElement(By.TagName("html"));
                for (int i = 0; i < Convert.ToInt32(pgNum); i++)
                {
                    html.SendKeys(Keys.PageDown);
                }
                var namesList = new ArrayList();
                IList<IWebElement> AllPostContainers = html.FindElements(By.ClassName("search-content__result"));
                //Loops through each Post
                foreach (var post in AllPostContainers)
                {
                    Console.WriteLine("Processing...\n");
                    // Instantiate a post
                    Post p = new Post();
                    IWebElement actorName = post.FindElement(By.ClassName("feed-shared-actor__name"));
                    IWebElement name = actorName.FindElement(By.TagName("span"));

                    // truncate feed-shared-text-view white-space-pre-wrap break-words ember-view
                    IWebElement jobTitle = post.FindElement(By.CssSelector(".truncate.feed-shared-text-view.white-space-pre-wrap.break-words.ember-view"));
                    IWebElement job = jobTitle.FindElement(By.TagName("span"));

                    // truncate feed-shared-text-view white-space-pre-wrap break-words ember-view
                    // feed-shared-text__text-view feed-shared-text-view white-space-pre-wrap break-words ember-view

                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
                    IWebElement jobContent = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".feed-shared-text__text-view.feed-shared-text-view.white-space-pre-wrap.break-words.ember-view")));



                    IWebElement content = jobContent.FindElement(By.TagName("span"));


                    if ((content.Text.ContainsSubstring(job.Text) == true && content.Text.ContainsSubstring(kw)
                        || content.Text.ContainsSubstring("#"+ kw)) 
                        || job.Text.ContainsSubstring("followers")) { 
                        p.Name = name.Text;
                        namesList.Add(p.Name);
                    }
                }


                WriteOutPutInFile(namesList);
               // driver.Quit();
                Console.WriteLine("==================================================================================\n");
                Console.WriteLine("Crawl end!");

                Console.ReadKey();
            }
        }
    }
}