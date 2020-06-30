using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Witanra.WebScraper;
using Witanra.WebScraper.Models;

namespace Witanra.WebScraper.Extentions
{
    public static class WebDriverExtention
    {
        public static void GoToUrl(this IWebDriver driver, string url, int waitCompleteMaxInSeconds = 10, int waitRenderInSeconds = 1)
        {
            driver.Navigate().GoToUrl(url);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitCompleteMaxInSeconds));
            wait.Until((x) =>
            {
                Thread.Sleep(waitRenderInSeconds * 1000); //Wait for any animation or post loading scripts to run
                return ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete");
            });
        }

        public static WebpageResult GetWebPageResult(this IWebDriver webDriver, IWebElement focusedWebElement = null, IWebElement singleCaptureWebElement = null)
        {
            var focusedWebElements = new List<IWebElement>();
            if (focusedWebElement != null)
            {
                focusedWebElements.Add(focusedWebElement);
            }

            var singleCaptureWebElements = new List<IWebElement>();
            if (singleCaptureWebElement != null)
            {
                singleCaptureWebElements.Add(singleCaptureWebElement);
            }

            return webDriver.GetWebPageResult(focusedWebElements, singleCaptureWebElements);
        }

        public static WebpageResult GetWebPageResult(this IWebDriver webDriver, List<IWebElement> focusedWebElements, List<IWebElement> singleCaptureWebElements)
        {
            var Screenshots = new Dictionary<string, byte[]>();
            Screenshots.Add("_Full", webDriver.GetEntireScreenshot(singleCaptureWebElements).ToByteArray(ImageFormat.Png));

            var result = new WebpageResult
            {
                dateTime = DateTime.Now,
                HTML = webDriver.PageSource,
                URL = webDriver.Url,
                Screenshots = Screenshots
            };

            var i = 1;
            foreach (var focusedWebElement in focusedWebElements)
            {
                using (var ms = new MemoryStream(Screenshots["_Full"]))
                {
                    using (var bitmap = new Bitmap(Image.FromStream(ms)))
                    {
                        var width = focusedWebElement.Size.Width;
                        if (bitmap.Width < focusedWebElement.Location.X + focusedWebElement.Size.Width)
                        {
                            width = bitmap.Width - focusedWebElement.Location.X;
                        }
                        var height = focusedWebElement.Size.Height;
                        if (bitmap.Height < focusedWebElement.Location.Y + focusedWebElement.Size.Height)
                        {
                            height = bitmap.Height - focusedWebElement.Location.Y;
                        }
                        var rectangle = new Rectangle(
                            focusedWebElement.Location.X,
                            focusedWebElement.Location.Y,
                            width,
                            height
                            );
                        var screenshot_focused = bitmap.Clone(rectangle, bitmap.PixelFormat);
                        result.Screenshots.Add($"Focused_{i}", screenshot_focused.ToByteArray(ImageFormat.Png));
                    }
                }
                i++;
            }

            return result;
        }

        public static byte[] GetScreenshotFocused(this IWebDriver _driver, byte[] Full_Screenshot, string Focus_XPath)
        {
            byte[] result;

            var FocusedElement = _driver.FindElement(By.XPath(Focus_XPath));
            var Focus_X = FocusedElement.Location.X;
            var Focus_Y = FocusedElement.Location.Y;
            var Focus_Width = FocusedElement.Size.Width;
            var Focus_Height = FocusedElement.Size.Height;

            using (var ms = new MemoryStream(Full_Screenshot))
            {
                using (var bitmap = new Bitmap(Image.FromStream(ms)))
                {
                    if (bitmap.Width < Focus_X + Focus_Width)
                    {
                        Focus_Width = bitmap.Width - Focus_X;
                    }

                    if (bitmap.Height < Focus_Y + Focus_Height)
                    {
                        Focus_Height = bitmap.Height - Focus_Y;
                    }
                    var rectangle = new Rectangle(
                        Focus_X,
                        Focus_Y,
                        Focus_Width,
                        Focus_Height
                        );
                    var screenshot_focused = bitmap.Clone(rectangle, bitmap.PixelFormat);
                    result = screenshot_focused.ToByteArray(ImageFormat.Png);
                }
            }
            return result;
        }

        //public static byte[] GetScreenshotFocused(this IWebDriver _driver, byte[] Full_Screenshot, string iFrame_XPath, string Focus_XPath)
        //{
        //    //This doesn't work, the iframe x,y isn;t correct?
        //    byte[] result;

        //    _driver.SwitchTo().ParentFrame();
        //    ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, 0)");
        //    Thread.Sleep(200);

        //    var Frame_X = 0;
        //    var Frame_Y = 0;
        //    if (!String.IsNullOrEmpty(iFrame_XPath))
        //    {
        //        var FrameElement = _driver.FindElement(By.XPath(iFrame_XPath));
        //        Frame_X = FrameElement.Location.X;
        //        Frame_Y = FrameElement.Location.Y;
        //        _driver.SwitchTo().Frame(FrameElement);
        //    }

        //    var FocusedElement = _driver.FindElement(By.XPath(Focus_XPath));
        //    var Focus_X = FocusedElement.Location.X;
        //    var Focus_Y = FocusedElement.Location.Y;
        //    var Focus_Width = FocusedElement.Size.Width;
        //    var Focus_Height = FocusedElement.Size.Height;

        //    using (var ms = new MemoryStream(Full_Screenshot))
        //    {
        //        using (var bitmap = new Bitmap(Image.FromStream(ms)))
        //        {
        //            if (bitmap.Width < Frame_X + Focus_X + Focus_Width)
        //            {
        //                Focus_Width = bitmap.Width - Frame_X + Focus_X;
        //            }

        //            if (bitmap.Height < Frame_Y + Focus_Y + Focus_Height)
        //            {
        //                Focus_Height = bitmap.Height - Frame_Y + Focus_Y;
        //            }
        //            var rectangle = new Rectangle(
        //                Focus_X + Frame_X,
        //                Frame_Y + Focus_Y,
        //                Focus_Width,
        //                Focus_Height
        //                );
        //            var screenshot_focused = bitmap.Clone(rectangle, bitmap.PixelFormat);
        //            result = screenshot_focused.ToByteArray(ImageFormat.Png);
        //        }
        //    }
        //    return result;
        //}

        public static void ScrollIntoView(this IWebDriver webDriver, IWebElement focusedWebElement)
        {
            ((IJavaScriptExecutor)webDriver).ExecuteScript("arguments[0].scrollIntoView(true);", focusedWebElement);
            Thread.Sleep(200);
        }

        public static Bitmap GetEntireScreenshot(this IWebDriver _driver, List<IWebElement> singleCaptureWebElements = null)
        {
            _driver.SwitchTo().ParentFrame();

            ((IJavaScriptExecutor)_driver).ExecuteScript("return window.stop");
            ((IJavaScriptExecutor)_driver).ExecuteScript("window.stop();");

            ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, 0)");
            Thread.Sleep(200);

            Bitmap stitchedImage = null;
            try
            {
                long totalwidth1 = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return document.body.offsetWidth");//documentElement.scrollWidth");
                long totalHeight1 = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return  document.body.parentNode.scrollHeight");

                int totalWidth = (int)totalwidth1;
                int totalHeight = (int)totalHeight1;

                // Get the Size of the Viewport
                long viewportWidth1 = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return document.body.clientWidth");//documentElement.scrollWidth");
                long viewportHeight1 = (long)((IJavaScriptExecutor)_driver).ExecuteScript("return window.innerHeight");//documentElement.scrollWidth");

                int viewportWidth = (int)viewportWidth1;
                int viewportHeight = (int)viewportHeight1;

                // Split the Screen in multiple Rectangles
                List<Rectangle> rectangles = new List<Rectangle>();

                // Loop until the Total Height is reached
                for (int i = 0; i < totalHeight; i += viewportHeight)
                {
                    int newHeight = viewportHeight;
                    // Fix if the Height of the Element is too big
                    if (i + viewportHeight > totalHeight)
                    {
                        newHeight = totalHeight - i;
                    }
                    // Loop until the Total Width is reached
                    for (int ii = 0; ii < totalWidth; ii += viewportWidth)
                    {
                        int newWidth = viewportWidth;
                        // Fix if the Width of the Element is too big
                        if (ii + viewportWidth > totalWidth)
                        {
                            newWidth = totalWidth - ii;
                        }

                        // Create and add the Rectangle
                        Rectangle currRect = new Rectangle(ii, i, newWidth, newHeight);
                        rectangles.Add(currRect);
                    }
                }

                // Build the Image
                stitchedImage = new Bitmap(totalWidth, totalHeight);
                // Get all Screenshots and stitch them together
                Rectangle previous = Rectangle.Empty;
                foreach (var rectangle in rectangles)
                {
                    // Calculate the Scrolling (if needed)
                    if (previous != Rectangle.Empty)
                    {
                        int xDiff = rectangle.Right - previous.Right;
                        int yDiff = rectangle.Bottom - previous.Bottom;

                        // Scroll
                        //selenium.RunScript(String.Format("window.scrollBy({0}, {1})", xDiff, yDiff));
                        ((IJavaScriptExecutor)_driver).ExecuteScript(String.Format("window.scrollBy({0}, {1})", xDiff, yDiff));

                        Thread.Sleep(200);
                    }

                    // Take Screenshot
                    var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();

                    // Build an Image out of the Screenshot
                    Image screenshotImage;
                    using (MemoryStream memStream = new MemoryStream(screenshot.AsByteArray))
                    {
                        screenshotImage = Image.FromStream(memStream);
                    }

                    // Calculate the Source Rectangle
                    Rectangle sourceRectangle = new Rectangle(viewportWidth - rectangle.Width, viewportHeight - rectangle.Height, rectangle.Width, rectangle.Height);

                    // Copy the Image
                    using (Graphics g = Graphics.FromImage(stitchedImage))
                    {
                        g.DrawImage(screenshotImage, rectangle, sourceRectangle, GraphicsUnit.Pixel);
                    }

                    foreach (var webElement in singleCaptureWebElements)
                    {
                        if (_driver.isVisibleInViewport(webElement))
                        {
                            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].style.visibility='hidden'", webElement);
                        }
                    }

                    // Set the Previous Rectangle
                    previous = rectangle;
                }
            }
            catch (Exception ex)
            {
                // handle
            }

            //reset scroll position
            ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, 0)");
            Thread.Sleep(200);

            return stitchedImage;
        }

        public static Boolean isVisibleInViewport(this IWebDriver _driver, IWebElement element)
        {
            return (Boolean)((IJavaScriptExecutor)_driver).ExecuteScript(
                "var elem = arguments[0],                 " +
                "  box = elem.getBoundingClientRect(),    " +
                "  cx = box.left + box.width / 2,         " +
                "  cy = box.top + box.height / 2,         " +
                "  e = document.elementFromPoint(cx, cy); " +
                "for (; e; e = e.parentElement) {         " +
                "  if (e === elem)                        " +
                "    return true;                         " +
                "}                                        " +
                "return false;                            "
                , element);
        }
    }
}