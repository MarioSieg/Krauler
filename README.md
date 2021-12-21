# Krauler

Webcrawling using public available proxies for several websites.

## Required Drivers
- [Mozilla Geckodriver] (https://github.com/mozilla/geckodriver/releases) in `C:\git\krauler\Krauler\bin\Debug\net5.0`
- [Google Chromedriver] (https://chromedriver.chromium.org/downloads) in `C:\git\krauler\Krauler\bin\Debug\net5.0`

## Tested Crawlers
- Youtube
- Google

## Folder Structure
- `/Config`: Selenium config files per crawler
- `/Resources`: Hardcoded definitions for proxies and user agents
- `/Logs`: The logs are stored here
- `/Crawlers`: The crawlers are implemented here