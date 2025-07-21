## Core Playwright Containers

1. **PlaywrightWaitContainer** - Wait for specific Playwright conditions (page load, elements, network requests,
   responses)
2. **PlaywrightIterateContainer** - Iterate through page elements, frames, tabs, or data sets with Playwright actions
3. **PlaywrightParallelContainer** - Execute Playwright actions in parallel across multiple tabs/contexts/browsers
4. **PlaywrightConditionalContainer** - Conditional execution based on page state, element visibility, or browser
   conditions
5. **PlaywrightCatchContainer** - Handle Playwright-specific exceptions (timeouts, navigation errors, element not found)
6. **PlaywrightRepeatUntilTrueContainer** - Repeat Playwright actions until page condition is met (element appears, URL
   changes, etc.)
7. **PlaywrightRepeatOnErrorUntilTrueContainer** - Retry Playwright actions on failures until success or max attempts

## Page/Navigation Containers

1. **PlaywrightPageSequenceContainer** - Execute actions across multiple pages in sequence
2. **PlaywrightTabContainer** - Manage actions across multiple browser tabs
3. **PlaywrightFrameContainer** - Execute actions within specific iframes
4. **PlaywrightContextContainer** - Manage browser contexts (incognito, different sessions)

## UI-Specific Containers

1. **PlaywrightFormContainer** - Handle complete form workflows (fill, validate, submit)
2. **PlaywrightModalContainer** - Handle modal dialogs, popups, and alerts
3. **PlaywrightTableContainer** - Iterate through table rows/columns with actions
4. **PlaywrightUploadContainer** - Handle file uploads and downloads
5. **PlaywrightDragDropContainer** - Handle drag and drop operations

## Testing Containers

1. **PlaywrightScreenshotContainer** - Take screenshots at specific test points
2. **PlaywrightVideoContainer** - Record video during test execution
3. **PlaywrightNetworkContainer** - Monitor and validate network requests/responses
4. **PlaywrightPerformanceContainer** - Measure and validate page performance metrics

## Advanced Containers

1. **PlaywrightMobileContainer** - Execute mobile-specific actions (swipe, pinch, rotate)
2. **PlaywrightGeolocationContainer** - Handle geolocation-based testing
3. **PlaywrightCookieContainer** - Manage cookies and session state
4. **PlaywrightStorageContainer** - Handle localStorage, sessionStorage operations
5. **PlaywrightApiContainer** - Combine API calls with UI actions for hybrid testing

These containers would extend `AbstractPlaywrightActionContainer` and provide specialized functionality for common
Playwright testing patterns while maintaining consistency with the core Agenix container architecture.
