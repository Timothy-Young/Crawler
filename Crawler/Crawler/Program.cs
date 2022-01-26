using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using PuppeteerSharp;
using System.Text;

namespace Crawler
{
  class Program
  {
    #region URLs
    public static string[] urls = {
      "https://www.google.com/search?q=best+movies+of+all+time",
      "https://www.imdb.com/chart/top",
      "https://www.imdb.com/title/godfather",
      "https://www.imdb.com/title/godfather-part-2",
      "https://www.imdb.com/title/lord-of-the-rings",
      "https://www.imdb.com/title/inception",
      "https://www.facebook.com/imdb",
      "https://www.facebook.com/rottentomatoes",
      "https://www.rottentomatoes.com/top/bestofrt",
      "https://www.rottentomatoes.com/m/godfather",
      "https://www.rottentomatoes.com/m/godfather-2",
      "https://www.rottentomatoes.com/m/lord-of-the-rings",
      "https://www.facebook.com/rottentomatoes",
      "https://www.facebook.com/imdb",
      "https://www.imdb.com/chart/top",
      "https://www.imdb.com/title/godfather",
      "https://www.imdb.com/title/godfather-part-2",
      "https://www.imdb.com/title/lord-of-the-rings",
      "https://www.imdb.com/title/inception",
      "https://www.facebook.com/imdb",
      "https://www.facebook.com/rottentomatoes",
      "https://www.rottentomatoes.com/top/bestofrt",
      "https://www.rottentomatoes.com/m/godfather",
      "https://www.rottentomatoes.com/m/godfather-2",
      "https://www.rottentomatoes.com/m/lord-of-the-rings"
    };
    #endregion
    public static List<string> urlsVisited = new List<string>();
    public static List<string> failedVisits = new List<string>();
    public static string ChromiumRevision = BrowserFetcher.DefaultChromiumRevision;
    public static string date = DateTime.Now.ToShortDateString().Replace('/', '-');

    static async Task Main(string[] args)
    {
      foreach (string url in urls)
      {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Crawling url... ");
        await new BrowserFetcher().DownloadAsync(ChromiumRevision);
        await LoadPage(url);
      }

      await LogResults(urlsVisited, "Visited");
      await LogResults(failedVisits, "Failed Visit");

      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("Crawl Complete!");
      Console.WriteLine($"View logs: {Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName}\\Log\\CrawlerLog-{date}.txt");
      Console.ReadKey();
    }

    private static async Task LoadPage(string url)
    {
      // Enabled headless option
      var launchOptions = new LaunchOptions { Headless = true };
      // Starting headless browser
      var browser = await Puppeteer.LaunchAsync(launchOptions);
      if (browser.IsConnected)
      {
        await WriteToLog($"Connected to {url}").ConfigureAwait(false);
      }
      // Get default pages 
      var pages = await browser.PagesAsync();
      if (pages.Length > 0 && pages[0] != null)
      {
        // Check for first page content and add to log
        if (!string.IsNullOrEmpty(await pages[0].GetContentAsync()))
        {
          // Could do work here if needed. For example, use a selector to grab some page data.
          // Since we're not doing any work, we're only looking at the first page to validate
          // that some it loaded (wasn't empty).

          // Log successful visit
          urlsVisited.Add(url);
          await WriteToLog($"Visited {url}").ConfigureAwait(false);
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"Successful crawl: {url} ");
          await pages[0].CloseAsync();
        }
        else
        {
          // Log visit failure
          failedVisits.Add(url);
          await WriteToLog($"Unable to visit {url}").ConfigureAwait(false);
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Failed to crawl: {url} ");
        }
      }
      else
      {
        // Log visit failure
        failedVisits.Add(url);
        await WriteToLog($"Unable to get pages for {url}").ConfigureAwait(false);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Failed to crawl: {url} ");
      }
      // Close browser
      await browser.CloseAsync();
    }
    public static async Task WriteToLog(string entry)
    {
      string now = DateTime.Now.ToString();
      string path = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName}\\Log";
      string filename = $"CrawlerLog-{date}.txt";
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
      await File.AppendAllTextAsync(Path.Combine(path, filename), $"\n{now} -- {entry}");
    }

    public static async Task LogResults(List<string> list, string type)
    {
      if (list.Count > 0)
      {
        StringBuilder sb = new StringBuilder();
        foreach (string url in list)
        {
          sb.AppendLine($"  - {url}");
        }

        await WriteToLog($"{type} Count: {list.Count}");
        await WriteToLog($"{type} URLs: \r\n{sb}");
      }
    }
  }
}
