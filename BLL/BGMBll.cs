using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Reflection;

namespace WhisperHime.BLL
{
    public class BGMBll
    {
        public static void PageScreenshot(string url, string path)
        {
            ChromeDriver driver = null;
            try
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArguments("headless", "disable-gpu");
                driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), options);
                driver.Navigate().GoToUrl(url);
                string width = driver.ExecuteScript("return document.body.scrollWidth").ToString();
                string height = driver.ExecuteScript("return document.body.scrollHeight").ToString();
                driver.Manage().Window.Size = new System.Drawing.Size(int.Parse(width), int.Parse(height)); //=int.Parse( height);
                var screenshot = (driver as ITakesScreenshot).GetScreenshot();
                screenshot.SaveAsFile(path);
            }
            catch (Exception)
            {
                //logger.Error(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                if (driver != null)
                {
                    driver.Close();
                    driver.Quit();
                }
            }
        }
    }
}
