<!-- Common Project Tags:
badfon 
cli 
command-line 
console-applications 
dotnet 
goodfon 
images 
netframework 
netframework48 
scraper 
tool 
tools 
vbnet 
visualstudio 
wallpaper 
wallpaper-app 
wallpapers 
web-crawler 
web-crawling 
web-scraper 
web-scraping 
webcrawler 
webcrawling 
webscraper 
webscraping 
windows 
windows-app 
windows-application 
windows-applications 
 -->

<div align="center">
  <img src="/Images/App.ico" width="150" alt="Goodfon Wallpaper Scraper Logo">
  
  <h1>Goodfon Wallpaper Scraper</h1>

### A rule-based, multi-threaded console application designed to scrape and download original-resolution wallpapers from [Goodfon.com](https://www.goodfon.com/) website. 

</div>

------------------
------------------

## 👋 Introduction

**Goodfon Wallpaper Scraper** is a fully automated, rule-based wallpaper downloader built for mass-harvesting original-resolution wallpapers from the **Goodfon.com** website without manual intervention. By combining user-defined keyword filtering with automated session and IP management, the program handles catalog discovery and wallpaper extraction completely on autopilot, ensuring you only pull the content you actually want while handling all edge cases in the background.

## 👌 Features

* **Threaded Concurrency:** Processes catalog pages sequentially while spinning up to 24 concurrent worker tasks per page to validate, process, and download wallpapers simultaneously.
* **Smart Account Rotation:** Seamlessly manages multiple GoodFon accounts. Accounts are kept strictly in a browsing session state to navigate unrestricted un-censored (NSFW) indices, and are dynamically selected for downloading only when a direct IP request hits a `403 (Forbidden)` barrier requiring authorized rights.
* **Bypass Server Limits:** Incorporates an automated VPN control mechanism that monitors IP usage and proactively forces a VPN server rotation before hitting GoodFon's hard daily limit of 80 anonymous downloads per IP.
* **Advanced Content Filtering:** Avoids unwanted content using robust, case-insensitive keyword filtering rules (`Forbidden Keywords.txt`) across all resolved image URLs.
* **Duplication Guard:** Tracks download history across sessions via MD5 URL hashing, completely avoiding redundant requests or re-downloading existing local files.

## 🖼️ Screenshots

![screenshot](/Images/screenshot1.png)

![screenshot](/Images/screenshot2.png)

## 🎦 Videos

[Goodfon Wallpaper Scraper DEMO VIDEO](https://github.com/user-attachments/assets/b8abcb2d-a941-4061-b205-5e454e8d3db6)

*Note: This video showcases an early alpha version of the program. The terminal output layout, log performance indicators, or general UI structures might slightly differ from the current production codebase.*

## 📝 Requirements

- Microsoft Windows OS.
- [Proton VPN client for Windows (free or paid plan).](https://protonvpn.com/download)

## 🤖 Getting Started

Download the latest release by clicking [here](https://github.com/ElektroStudios/Goodfon-Wallpaper-Scraper/releases/latest) and start using it!.

Also, you can read a manual in HTML format by clicking [here](https://htmlpreview.github.io/?https://github.com/ElektroStudios/Goodfon-Wallpaper-Scraper/blob/main/Source/Goodfon%20Wallpaper%20Scraper/manual/Goodfon%20Wallpaper%20Scraper%20Manual.html).

## 💡 Tips

* **Bulk Account Creation:** You can use temporary mail services like [temp-mail.org](https://temp-mail.org/) to quickly register multiple user accounts on Goodfon in just a few minutes. 
* **Maximize Restricted Pulls:** Since every registered account grants you an individual allocation of 100 daily downloads for mature/restricted content, adding a batch of configured credentials to your `Accounts.json` file ensures a massive threshold of restricted assets can be downloaded concurrently during a single session.

## ⚠️ Limitations

* **Proton VPN Client Dependency:** While the program can technically interface with the free tier of the Proton VPN client for Windows, doing so is highly unstable. The free version imposes severe multi-minute cooldown penalties for server switching, which forces the scraper to halt or time out during a rotation cycle. For an uninterrupted automated experience capable of pulling tens of thousands of wallpapers in a matter of hours, **a paid Proton VPN plan is heavily recommended**.
* **Hard Single-Session Cap:** Without any running instance of Proton VPN, the scraper is strictly limited by the web-server's native restrictions: 80 daily downloads for guest IPs, or 100 downloads if a registered Goodfon account is provided.
* **No 24h+ Standby Cycle:** The application is architected as an intensive, single-run batch processor. It is not designed to run permanently as a 24/7 background service; internal metrics, download counters, and account-exhaustion states (`DailyLimitReached`) are tracked in volatile memory and do not automatically reset or refresh after a 24-hour clock cycle without a clean app restart.

## 🔄 Change Log

Explore the complete list of changes, bug fixes, and improvements across different releases by clicking [here](/Docs/CHANGELOG.md).

## 🏆 Credits

This work relies on the following resources: 

 - [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework)
 - [HtmlAgilityPack](https://html-agility-pack.net/)

## ⚠️ Disclaimer:

This Work (the repository and the content provided in) is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the Work or the use or other dealings in the Work.

This Work has no affiliation, approval or endorsement by the author(s) of the third-party libraries used by this Work.

## 💪 Contributing

Your contribution is highly appreciated!. If you have any ideas, suggestions, or encounter issues, feel free to open an issue by clicking [here](https://github.com/ElektroStudios/Goodfon-Wallpaper-Scraper/issues/new/choose). 

Your input helps make this Work better for everyone. Thank you for your support! 🚀

## 💰 Beyond Contribution 

This work is distributed for educational purposes and without any profit motive. However, if you find value in my efforts and wish to support and motivate my ongoing work, you may consider contributing financially through the following options:

<br></br>
<p align="center"><img src="/Images/github_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Becoming my sponsor on Github:</h3>
<p align="center">You can show me your support by clicking <a href="https://github.com/sponsors/ElektroStudios/">here</a>, <br align="center">contributing any amount you prefer, and unlocking rewards!</br></p>
<br></br>

<p align="center"><img src="/Images/paypal_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Making a Paypal Donation:</h3>
<p align="center">You can donate to me any amount you like via Paypal by clicking <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=E4RQEV6YF5NZY">here</a>.</p>
<br></br>

<p align="center"><img src="/Images/envato_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Purchasing software of mine at Envato's Codecanyon marketplace:</h3>
<p align="center">If you are a .NET developer, you may want to explore '<b>DevCase Class Library for .NET</b>', <br align="center">a huge set of APIs that I have on sale. Check out the product by clicking <a href="https://codecanyon.net/item/elektrokit-class-library-for-net/19260282">here</a></br><br align="center"><i>It also contains all piece of reusable code that you can find across the source code of my open source works.</i></p>
<br></br>

<h2 align="center"><u>Your support means the world to me! Thank you for considering it!</u> 👍</h2>
