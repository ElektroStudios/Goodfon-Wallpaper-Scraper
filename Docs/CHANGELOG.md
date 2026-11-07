# Goodfon Wallpaper Scraper Change Log 📋

## v1.0.2 **(current)** 🆕
 - Fixed a crash at the end of execution (NullReferenceException) when no user-account was configured, enabled or successfully logged-in.
 - Fixed a crash (NullReferenceException) when a page request failed with a non-timeout error, which aborted the whole execution with a fatal error.
 - Fixed timeout messages (login, page, wallpaper and image requests): they threw a FormatException instead of being displayed, due to an invalid "mm:ss" TimeSpan format.
 - Fixed login timeouts not being detected as timeouts.
 - Fixed a crash in Release builds when a configured username or password was shorter than 4 characters.
 - Fixed a potential infinite retry loop when an image download returned "403 Forbidden" and a user-account was available.
 - Fixed user-account rotation stopping on an account that already reached its daily limit, instead of moving to the next available account.
 - Fixed the user-account number shown in the "Download limit reached" message.
 - Fixed relative user profile links being treated as wallpaper pages in listing pages.
 - Fixed long path (\\?\) conversion producing invalid paths for root-relative and drive-relative paths.
 - Fixed exit codes and HRESULT values being displayed in decimal with a "0x" prefix; they are now displayed in hexadecimal.
 - Fixed user-account login failing since the website introduced a login rate-limit ("Too many attempts. Please wait a few minutes"). The program now detects it and waits before retrying.
 - Added configurable login settings in the "Accounts.json" file: 'RateLimitWaitSeconds' (wait time before retrying a rate-limited login), 'MaxLoginAttempts' (attempts per account) and 'DelayBetweenLoginsSeconds' (pause between account logins). The legacy "Accounts.json" format (a plain list of accounts) is still supported, using default settings.
 - Added a unit tests project (MSTest).
 - Refactored URL, HTML parsing, configuration parsing and user-account rotation logic into separate testable components.

## v1.0.1 🔁
 - Added full support for long file paths to bypass the 260-character MAX_PATH limitation.
 - Implemented automatic account limit reset after 24 hours, supporting continuous long-running execution for restricted content downloads.
 - Refined wallpaper discovery logic to display the exact number of available wallpapers instead of an approximation.
 - Minor source-code changes and optimizations.

## v1.0 🔁
Initial Release.