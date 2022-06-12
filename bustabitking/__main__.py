import asyncio
from datetime import date, datetime
import traceback
from playwright.async_api import async_playwright
from bustabitking.alert import sendmail

class Gambler:
    def __init__(self, bet_base):
        self.loss_count = 0
        self.can_bet = False
        self.bet_base = bet_base

    def win(self):
        self.loss_count = 0
        self.can_bet = False

    def loss(self):
        self.loss_count += 1
        if self.loss_count == 5:
            self.bet_base = int(round(self.bet_base / 2, 0))
            if self.bet_base < 1:
                self.bet_base = 1
            self.loss_count = 0

    def bet(self):
        return self.bet_base * 2**self.loss_count

async def main():
    async with async_playwright() as p:
        browser = await p.chromium.launch(
            headless=False,
            # proxy={
            #     "server": "http://127.0.0.1:10809"
            # }
        )
        page = await browser.new_page()
        await page.goto('https://www.bustabit.com/play')
        await page.pause() #等待用户登陆
        login_btn_count = await page.locator("nav >> text=Login").count()
        if login_btn_count:
            raise Exception("用户登陆超时!");
        else:
            await page.locator("text=History").click()
            await page.wait_for_selector("//div[@class='switchable-area']/table/tbody/tr[1]")
            head = await page.locator("//div[@class='switchable-area']/table/tbody/tr[1]/td[1]").text_content()
            tail = await page.locator("//div[@class='switchable-area']/table/tbody/tr[1]/td[last()]/input").get_attribute('value')
            gambler = Gambler(1)
            last_sendemail_time = datetime.now()
            with open(r'log.txt', 'a') as log_file:
                while True:
                    await page.wait_for_timeout(1000)
                    now = datetime.now()
                    if now.minute in [15, 45] and now.second in list(range(50, 60)) and (now - last_sendemail_time).seconds > 10:
                        screenshot_path = f"{now.strftime('%Y-%m-%d-%H-%M-%S')}screenshot.png"
                        await page.screenshot(path=screenshot_path, full_page=True)
                        sendmail('Info', 'screenshot', screenshot_path)
                        last_sendemail_time = now
                    wait_tail = await page.locator("//div[@class='switchable-area']/table/tbody/tr[1]/td[last()]/input").get_attribute('value')
                    if tail != wait_tail:
                        tail = wait_tail
                        head = await page.locator("//div[@class='switchable-area']/table/tbody/tr[1]/td[1]").text_content()
                        in_bet = await page.locator("//div[@class='switchable-area']/table/tbody/tr[1]/td[2]").text_content()
                        print(f"BUST: {head} - In Bet: {in_bet} - HASH: {tail}", file=log_file, flush=True)
                        if in_bet == '—':
                            if float(head.strip('×').replace(',', '')) < 1.98:
                                gambler.can_bet = True
                        else:
                            if float(head.strip('×').replace(',', '')) >= 1.98:
                                gambler.win()
                            else:
                                gambler.loss()
                        if gambler.can_bet:
                            await page.locator("input[name=\"wager\"]").fill(str(gambler.bet()))
                            await page.wait_for_timeout(4000)
                            await page.locator("button:has-text(\"BET\")").click()
try:
    asyncio.run(main())
except Exception as e:
    error_info = traceback.format_exc()
    print(error_info)
    sendmail('ERROR', error_info)
    raise(e)
