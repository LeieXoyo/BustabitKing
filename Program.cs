using Microsoft.Playwright;
using BustabitKing;


using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchPersistentContextAsync(
    userDataDir: "./user_data_dir",
    new BrowserTypeLaunchPersistentContextOptions {
        Headless = false,
        Proxy = new Proxy {
            Server = Helper.config["ProxyServer"]
        }
    }
);
var page = await browser.NewPageAsync();
await page.GotoAsync("https://www.bustabit.com/play");
await page.PauseAsync();
var loginBtnCount = await page.Locator("nav >> text=Login").CountAsync();

if (loginBtnCount != 0)
{
    throw new Exception("用户登陆超时!");
}
else
{
    new Thread(Helper.Monitor).Start("node");
    await page.ScreenshotAsync(new PageScreenshotOptions{
        Path = $"./screenshots/{DateTime.Now.ToString().Replace(' ', '_')}_starting_screenshot.png",
        FullPage = true
    });
    await page.Locator("text=History").ClickAsync();
    await page.WaitForSelectorAsync("//div[@class='switchable-area']/table/tbody/tr[1]");
    var tail = await page.Locator("//div[@class='switchable-area']/table/tbody/tr[1]/td[last()]/input").GetAttributeAsync("value");
    var balance = Convert.ToDouble((await page.Locator("//a[@href='/account/stats']").InnerTextAsync()).Split(':')[^1].Replace(",",  " "));
    var gambler = new Gambler(balance);
    var targetCount = 0;
    Console.WriteLine($"Round Balance: {gambler.RoundBalance}");
    
    while (true)
    {
        await page.WaitForTimeoutAsync(1000);
        var waitTail = await page.Locator("//div[@class='switchable-area']/table/tbody/tr[1]/td[last()]/input").GetAttributeAsync("value");
        
        if (tail != waitTail)
        {
            tail = waitTail;
            var head = await page.Locator("//div[@class='switchable-area']/table/tbody/tr[1]/td[1]").TextContentAsync();
            var inBet = await page.Locator("//div[@class='switchable-area']/table/tbody/tr[1]/td[2]").TextContentAsync();
            balance = Convert.ToDouble((await page.Locator("//a[@href='/account/stats']").InnerTextAsync()).Split(':')[^1].Replace(",",  " "));
            var betting = Convert.ToDouble((await page.Locator("(//div[@class='table-responsive'])[2]/table/tbody/tr/td[3]").TextContentAsync())!.Split(':')[^1].Replace(",", "").Replace(" bits", ""));
            
            if (inBet == "—")
            {
                gambler.updateRoundBalance(balance);
                Console.WriteLine($"Round Balance: {gambler.RoundBalance}");
                
                if (Convert.ToDouble(head!.Replace("×", "").Replace(",", "")) < 10)
                {
                    targetCount += 1;

                    if (targetCount == 40)
                    {
                        gambler.CanBet = true;
                    }
                    else
                    {
                        targetCount = 0;
                    }
                }
                else
                {
                    var screenshotPath = $"./screenshots/{DateTime.Now.ToString().Replace(' ', '_')}_screenshot.png";
                    await page.ScreenshotAsync(new PageScreenshotOptions {
                        Path = screenshotPath,
                        FullPage = true
                    });

                    if (Convert.ToDouble(head.Replace("×", "").Replace(",", "")) >= 10)
                    {
                        gambler.Win(screenshotPath, targetCount);
                        targetCount = 0;
                    }
                    else
                    {
                        gambler.Loss(screenshotPath, targetCount);
                        targetCount += 1;
                    }
                }

                if (gambler.CanBet)
                {
                    await page.Locator("input[name=\"wager\"]").FillAsync(gambler.Bet().ToString());
                    await page.WaitForTimeoutAsync(4000);
                    await page.Locator("button:has-text(\"BET\")").ClickAsync();
                }

                var output = $"{DateTime.Now} - [{head}, {inBet}, {tail} - balance: {balance} - betting: {betting} - targetCount: {targetCount}]";  
                await File.AppendAllTextAsync("log.txt", output);
                Console.WriteLine(output);
            }
        }
    }
}