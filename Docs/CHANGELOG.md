# Goodfon Wallpaper Scraper Change Log 📋

## v1.0.3 **(current)** 🆕
 - Apply account fallback at the download entry point, including queued downloads after the anonymous IP limit is reached.
 - Retry the same wallpaper with the next available account when the current account reaches its daily limit.
 - Explain when account fallback is unavailable, distinguishing disabled accounts, failed logins and exhausted logged-in accounts, with configured/enabled/logged-in counts.
 - Avoid further anonymous wallpaper requests while the IP is known to be exhausted and no account is available; page-level VPN recovery attempts retain their existing cooldown.
 - Recheck the VPN retry cooldown after acquiring the rotation flag to prevent concurrent callers from repeating a failed rotation immediately.
 - Add regression tests for the download fallback flow using simulated HTTP responses.

## v1.0.2 🔁
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
 - Login attempts that fail due to a network error (e.g., "The underlying connection was closed") or a timeout are now retried after a configurable wait time ('NetworkErrorWaitSeconds' in Accounts.json). The console now shows the account index and the remaining accounts while logging-in.
 - Fixed free wallpapers not being downloaded anymore once the daily limit of 80 downloads per IP was reached and the Proton VPN server could not be rotated. The configured user-accounts are now used to keep downloading them (each account has its own daily download limit).
 - The Proton VPN rotation is no longer repeated for every wallpaper after a failure: it is retried every 5 minutes, and anonymous downloads are resumed as soon as it succeeds (saving the user-accounts' daily downloads).
 - Downloads performed with a user-account no longer count towards the daily download limit of the current IP.
 - Added a counter to the user-accounts login procedure (current account, remaining accounts and logged-in accounts), an estimated remaining time (ETA), and a message while waiting between logins.
 - Added an estimated remaining time (ETA) for the current query (user profile, tag or search), based on the processing speed of the most recent pages.
 - The GitHub repository URL is now displayed at startup, below the application title.
 - Updated the console introduction, README, Accounts.json and manual (English and Spanish) to explain when the user-accounts are used.
 - Added a unit tests project (MSTest).
 - Refactored URL, HTML parsing, configuration parsing and user-account rotation logic into separate testable components.

## v1.0.1 🔁
 - Added full support for long file paths to bypass the 260-character MAX_PATH limitation.
 - Implemented automatic account limit reset after 24 hours, supporting continuous long-running execution for restricted content downloads.
 - Refined wallpaper discovery logic to display the exact number of available wallpapers instead of an approximation.
 - Minor source-code changes and optimizations.

## v1.0 🔁
Initial Release.