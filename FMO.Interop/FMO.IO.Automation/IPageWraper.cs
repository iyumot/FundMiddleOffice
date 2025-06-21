using Microsoft.Playwright;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FMO.IO;

public class IPageWraper : IPage, IAsyncDisposable
{
    public required string Identifier { get; init; }

    public required IPage Page { get; set; }
    public bool IsNew { get; private set; }

    public IClock Clock => Page.Clock;

    [Obsolete]
    public IAccessibility Accessibility => Page.Accessibility;

    public IBrowserContext Context => Page.Context;

    public IReadOnlyList<IFrame> Frames => Page.Frames;

    public bool IsClosed => Page.IsClosed;

    public IKeyboard Keyboard => Page.Keyboard;

    public IFrame MainFrame => Page.MainFrame;

    public IMouse Mouse => Page.Mouse;

    public IAPIRequestContext APIRequest => Page.APIRequest;

    public ITouchscreen Touchscreen => Page.Touchscreen;

    public string Url => Page.Url;

    public IVideo? Video => Page.Video;

    public PageViewportSizeResult? ViewportSize => Page.ViewportSize;

    public IReadOnlyList<IWorker> Workers => Page.Workers;

    [SetsRequiredMembers]
    public IPageWraper(string identifier, IPage page, bool isNew = false)
    {
        Identifier = identifier;
        Page = page;
        IsNew = isNew;

        // 订阅所有Page事件
        Page.Close += OnPageClose;
        Page.Console += OnPageConsole;
        Page.Crash += OnPageCrash;
        Page.Dialog += OnPageDialog;
        Page.DOMContentLoaded += OnPageDOMContentLoaded;
        Page.Download += OnPageDownload;
        Page.FileChooser += OnPageFileChooser;
        Page.FrameAttached += OnPageFrameAttached;
        Page.FrameDetached += OnPageFrameDetached;
        Page.FrameNavigated += OnPageFrameNavigated;
        Page.Load += OnPageLoad;
        Page.PageError += OnPagePageError;
        Page.Popup += OnPagePopup;
        Page.Request += OnPageRequest;
        Page.RequestFailed += OnPageRequestFailed;
        Page.RequestFinished += OnPageRequestFinished;
        Page.Response += OnPageResponse;
        Page.WebSocket += OnPageWebSocket;
        Page.Worker += OnPageWorker;
    }

    // 事件处理转发
    private void OnPageClose(object? sender, IPage page) => Close?.Invoke(this, page);
    private void OnPageConsole(object? sender, IConsoleMessage msg) => Console?.Invoke(this, msg);
    private void OnPageCrash(object? sender, IPage page) => Crash?.Invoke(this, page);
    private void OnPageDialog(object? sender, IDialog dialog) => Dialog?.Invoke(this, dialog);
    private void OnPageDOMContentLoaded(object? sender, IPage page) => DOMContentLoaded?.Invoke(this, page);
    private void OnPageDownload(object? sender, IDownload download) => Download?.Invoke(this, download);
    private void OnPageFileChooser(object? sender, IFileChooser chooser) => FileChooser?.Invoke(this, chooser);
    private void OnPageFrameAttached(object? sender, IFrame frame) => FrameAttached?.Invoke(this, frame);
    private void OnPageFrameDetached(object? sender, IFrame frame) => FrameDetached?.Invoke(this, frame);
    private void OnPageFrameNavigated(object? sender, IFrame frame) => FrameNavigated?.Invoke(this, frame);
    private void OnPageLoad(object? sender, IPage page) => Load?.Invoke(this, page);
    private void OnPagePageError(object? sender, string error) => PageError?.Invoke(this, error);
    private void OnPagePopup(object? sender, IPage page) => Popup?.Invoke(this, page);
    private void OnPageRequest(object? sender, IRequest request) => Request?.Invoke(this, request);
    private void OnPageRequestFailed(object? sender, IRequest request) => RequestFailed?.Invoke(this, request);
    private void OnPageRequestFinished(object? sender, IRequest request) => RequestFinished?.Invoke(this, request);
    private void OnPageResponse(object? sender, IResponse response) => Response?.Invoke(this, response);
    private void OnPageWebSocket(object? sender, IWebSocket webSocket) => WebSocket?.Invoke(this, webSocket);
    private void OnPageWorker(object? sender, IWorker worker) => Worker?.Invoke(this, worker);

    // 事件声明
    public event EventHandler<IPage>? Close;
    public event EventHandler<IConsoleMessage>? Console;
    public event EventHandler<IPage>? Crash;
    public event EventHandler<IDialog>? Dialog;
    public event EventHandler<IPage>? DOMContentLoaded;
    public event EventHandler<IDownload>? Download;
    public event EventHandler<IFileChooser>? FileChooser;
    public event EventHandler<IFrame>? FrameAttached;
    public event EventHandler<IFrame>? FrameDetached;
    public event EventHandler<IFrame>? FrameNavigated;
    public event EventHandler<IPage>? Load;
    public event EventHandler<string>? PageError;
    public event EventHandler<IPage>? Popup;
    public event EventHandler<IRequest>? Request;
    public event EventHandler<IRequest>? RequestFailed;
    public event EventHandler<IRequest>? RequestFinished;
    public event EventHandler<IResponse>? Response;
    public event EventHandler<IWebSocket>? WebSocket;
    public event EventHandler<IWorker>? Worker;

    // IDisposable实现
    private bool _disposed;
    //public void Dispose()
    //{
    //    Automation.ReleasePage(Page, Identifier);

    //    if (!_disposed)
    //    {
    //        // 取消订阅所有事件
    //        Page.Close -= OnPageClose;
    //        Page.Console -= OnPageConsole;
    //        Page.Crash -= OnPageCrash;
    //        Page.Dialog -= OnPageDialog;
    //        Page.DOMContentLoaded -= OnPageDOMContentLoaded;
    //        Page.Download -= OnPageDownload;
    //        Page.FileChooser -= OnPageFileChooser;
    //        Page.FrameAttached -= OnPageFrameAttached;
    //        Page.FrameDetached -= OnPageFrameDetached;
    //        Page.FrameNavigated -= OnPageFrameNavigated;
    //        Page.Load -= OnPageLoad;
    //        Page.PageError -= OnPagePageError;
    //        Page.Popup -= OnPagePopup;
    //        Page.Request -= OnPageRequest;
    //        Page.RequestFailed -= OnPageRequestFailed;
    //        Page.RequestFinished -= OnPageRequestFinished;
    //        Page.Response -= OnPageResponse;
    //        Page.WebSocket -= OnPageWebSocket;
    //        Page.Worker -= OnPageWorker;

    //        _disposed = true;
    //    }
    //    GC.SuppressFinalize(this);
    //}


    public async ValueTask DisposeAsync()
    {
        await Automation.ReleasePage(Page, Identifier);

        if (!_disposed)
        {
            // 取消订阅所有事件
            Page.Close -= OnPageClose;
            Page.Console -= OnPageConsole;
            Page.Crash -= OnPageCrash;
            Page.Dialog -= OnPageDialog;
            Page.DOMContentLoaded -= OnPageDOMContentLoaded;
            Page.Download -= OnPageDownload;
            Page.FileChooser -= OnPageFileChooser;
            Page.FrameAttached -= OnPageFrameAttached;
            Page.FrameDetached -= OnPageFrameDetached;
            Page.FrameNavigated -= OnPageFrameNavigated;
            Page.Load -= OnPageLoad;
            Page.PageError -= OnPagePageError;
            Page.Popup -= OnPagePopup;
            Page.Request -= OnPageRequest;
            Page.RequestFailed -= OnPageRequestFailed;
            Page.RequestFinished -= OnPageRequestFinished;
            Page.Response -= OnPageResponse;
            Page.WebSocket -= OnPageWebSocket;
            Page.Worker -= OnPageWorker;

            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    // 转发所有IPage方法到Page实例
    #region 转发所有IPage方法到Page实例
    public Task AddInitScriptAsync(string? script = null, string? scriptPath = null) => Page.AddInitScriptAsync(script, scriptPath);

    public Task AddLocatorHandlerAsync(ILocator locator, Func<ILocator, Task> handler, PageAddLocatorHandlerOptions? options = null) => Page.AddLocatorHandlerAsync(locator, handler, options);

    public Task AddLocatorHandlerAsync(ILocator locator, Func<Task> handler, PageAddLocatorHandlerOptions? options = null) => Page.AddLocatorHandlerAsync(locator, _ => handler(), options);

    public Task<IElementHandle> AddScriptTagAsync(PageAddScriptTagOptions? options = null) => Page.AddScriptTagAsync(options);

    public Task<IElementHandle> AddStyleTagAsync(PageAddStyleTagOptions? options = null) => Page.AddStyleTagAsync(options);

    public Task BringToFrontAsync() => Page.BringToFrontAsync();

    public Task CheckAsync(string selector, PageCheckOptions? options = null) => Page.CheckAsync(selector, options);

    public Task ClickAsync(string selector, PageClickOptions? options = null) => Page.ClickAsync(selector, options);

    public Task CloseAsync(PageCloseOptions? options = null) => Page.CloseAsync(options);

    public Task<string> ContentAsync() => Page.ContentAsync();

    public Task DblClickAsync(string selector, PageDblClickOptions? options = null) => Page.DblClickAsync(selector, options);

    public Task DispatchEventAsync(string selector, string type, object? eventInit = null, PageDispatchEventOptions? options = null) => Page.DispatchEventAsync(selector, type, eventInit, options);

    public Task DragAndDropAsync(string source, string target, PageDragAndDropOptions? options = null) => Page.DragAndDropAsync(source, target, options);

    public Task EmulateMediaAsync(PageEmulateMediaOptions? options = null) => Page.EmulateMediaAsync(options);

    public Task<T> EvalOnSelectorAllAsync<T>(string selector, string expression, object? arg = null) => Page.EvalOnSelectorAllAsync<T>(selector, expression, arg);

    public Task<JsonElement?> EvalOnSelectorAllAsync(string selector, string expression, object? arg = null) => Page.EvalOnSelectorAllAsync(selector, expression, arg);

    public Task<T> EvalOnSelectorAsync<T>(string selector, string expression, object? arg = null, PageEvalOnSelectorOptions? options = null) => Page.EvalOnSelectorAsync<T>(selector, expression, arg, options);

    public Task<JsonElement?> EvalOnSelectorAsync(string selector, string expression, object? arg = null) => Page.EvalOnSelectorAsync(selector, expression, arg);

    public Task<T> EvaluateAsync<T>(string expression, object? arg = null) => Page.EvaluateAsync<T>(expression, arg);

    public Task<JsonElement?> EvaluateAsync(string expression, object? arg = null) => Page.EvaluateAsync(expression, arg);

    public Task<IJSHandle> EvaluateHandleAsync(string expression, object? arg = null) => Page.EvaluateHandleAsync(expression, arg);

    // ExposeBinding系列方法
    public Task ExposeBindingAsync(string name, Action callback, PageExposeBindingOptions? options = null) => Page.ExposeBindingAsync(name, callback, options);

    public Task ExposeBindingAsync(string name, Action<BindingSource> callback) => Page.ExposeBindingAsync(name, callback);

    public Task ExposeBindingAsync<T>(string name, Action<BindingSource, T> callback) => Page.ExposeBindingAsync(name, callback);

    public Task ExposeBindingAsync<TResult>(string name, Func<BindingSource, TResult> callback) => Page.ExposeBindingAsync(name, callback);

    public Task ExposeBindingAsync<TResult>(string name, Func<BindingSource, IJSHandle, TResult> callback) => Page.ExposeBindingAsync(name, callback);

    public Task ExposeBindingAsync<T, TResult>(string name, Func<BindingSource, T, TResult> callback) => Page.ExposeBindingAsync(name, callback);

    public Task ExposeBindingAsync<T1, T2, TResult>(string name, Func<BindingSource, T1, T2, TResult> callback) => Page.ExposeBindingAsync(name, callback);

    public Task ExposeBindingAsync<T1, T2, T3, TResult>(string name, Func<BindingSource, T1, T2, T3, TResult> callback) => Page.ExposeBindingAsync(name, callback);

    public Task ExposeBindingAsync<T1, T2, T3, T4, TResult>(string name, Func<BindingSource, T1, T2, T3, T4, TResult> callback) => Page.ExposeBindingAsync(name, callback);

    // ExposeFunction系列方法
    public Task ExposeFunctionAsync(string name, Action callback) => Page.ExposeFunctionAsync(name, callback);

    public Task ExposeFunctionAsync<T>(string name, Action<T> callback) => Page.ExposeFunctionAsync(name, callback);

    public Task ExposeFunctionAsync<TResult>(string name, Func<TResult> callback) => Page.ExposeFunctionAsync(name, callback);

    public Task ExposeFunctionAsync<T, TResult>(string name, Func<T, TResult> callback) => Page.ExposeFunctionAsync(name, callback);

    public Task ExposeFunctionAsync<T1, T2, TResult>(string name, Func<T1, T2, TResult> callback) => Page.ExposeFunctionAsync(name, callback);

    public Task ExposeFunctionAsync<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> callback) => Page.ExposeFunctionAsync(name, callback);

    public Task ExposeFunctionAsync<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T3, T4, TResult> callback) => Page.ExposeFunctionAsync(name, callback);

    public Task FillAsync(string selector, string value, PageFillOptions? options = null) => Page.FillAsync(selector, value, options);

    public Task FocusAsync(string selector, PageFocusOptions? options = null) => Page.FocusAsync(selector, options);

    public IFrame? Frame(string name) => Page.Frame(name);

    public IFrame? FrameByUrl(string url) => Page.FrameByUrl(url);

    public IFrame? FrameByUrl(Regex url) => Page.FrameByUrl(url);

    public IFrame? FrameByUrl(Func<string, bool> url) => Page.FrameByUrl(url);

    public IFrameLocator FrameLocator(string selector) => Page.FrameLocator(selector);

    public Task<string?> GetAttributeAsync(string selector, string name, PageGetAttributeOptions? options = null) => Page.GetAttributeAsync(selector, name, options);

    public ILocator GetByAltText(string text, PageGetByAltTextOptions? options = null) => Page.GetByAltText(text, options);

    public ILocator GetByAltText(Regex text, PageGetByAltTextOptions? options = null) => Page.GetByAltText(text, options);

    public ILocator GetByLabel(string text, PageGetByLabelOptions? options = null) => Page.GetByLabel(text, options);

    public ILocator GetByLabel(Regex text, PageGetByLabelOptions? options = null) => Page.GetByLabel(text, options);

    public ILocator GetByPlaceholder(string text, PageGetByPlaceholderOptions? options = null) => Page.GetByPlaceholder(text, options);

    public ILocator GetByPlaceholder(Regex text, PageGetByPlaceholderOptions? options = null) => Page.GetByPlaceholder(text, options);

    public ILocator GetByRole(AriaRole role, PageGetByRoleOptions? options = null) => Page.GetByRole(role, options);

    public ILocator GetByTestId(string testId) => Page.GetByTestId(testId);

    public ILocator GetByTestId(Regex testId) => Page.GetByTestId(testId);

    public ILocator GetByText(string text, PageGetByTextOptions? options = null) => Page.GetByText(text, options);

    public ILocator GetByText(Regex text, PageGetByTextOptions? options = null) => Page.GetByText(text, options);

    public ILocator GetByTitle(string text, PageGetByTitleOptions? options = null) => Page.GetByTitle(text, options);

    public ILocator GetByTitle(Regex text, PageGetByTitleOptions? options = null) => Page.GetByTitle(text, options);

    public Task<IResponse?> GoBackAsync(PageGoBackOptions? options = null) => Page.GoBackAsync(options);

    public Task<IResponse?> GoForwardAsync(PageGoForwardOptions? options = null) => Page.GoForwardAsync(options);

    public Task<IResponse?> GotoAsync(string url, PageGotoOptions? options = null) { IsNew = false; return Page.GotoAsync(url, options); }

    public Task HoverAsync(string selector, PageHoverOptions? options = null) => Page.HoverAsync(selector, options);

    public Task<string> InnerHTMLAsync(string selector, PageInnerHTMLOptions? options = null) => Page.InnerHTMLAsync(selector, options);

    public Task<string> InnerTextAsync(string selector, PageInnerTextOptions? options = null) => Page.InnerTextAsync(selector, options);

    public Task<string> InputValueAsync(string selector, PageInputValueOptions? options = null) => Page.InputValueAsync(selector, options);

    public Task<bool> IsCheckedAsync(string selector, PageIsCheckedOptions? options = null) => Page.IsCheckedAsync(selector, options);

    public Task<bool> IsDisabledAsync(string selector, PageIsDisabledOptions? options = null) => Page.IsDisabledAsync(selector, options);

    public Task<bool> IsEditableAsync(string selector, PageIsEditableOptions? options = null) => Page.IsEditableAsync(selector, options);

    public Task<bool> IsEnabledAsync(string selector, PageIsEnabledOptions? options = null) => Page.IsEnabledAsync(selector, options);

    public Task<bool> IsHiddenAsync(string selector, PageIsHiddenOptions? options = null) => Page.IsHiddenAsync(selector, options);

    public Task<bool> IsVisibleAsync(string selector, PageIsVisibleOptions? options = null) => Page.IsVisibleAsync(selector, options);

    public ILocator Locator(string selector, PageLocatorOptions? options = null) => Page.Locator(selector, options);

    public Task<IPage?> OpenerAsync() => Page.OpenerAsync();

    public Task PauseAsync() => Page.PauseAsync();

    public Task<byte[]> PdfAsync(PagePdfOptions? options = null) => Page.PdfAsync(options);

    public Task PressAsync(string selector, string key, PagePressOptions? options = null) => Page.PressAsync(selector, key, options);

    public Task<IReadOnlyList<IElementHandle>> QuerySelectorAllAsync(string selector) => Page.QuerySelectorAllAsync(selector);

    public Task<IElementHandle?> QuerySelectorAsync(string selector, PageQuerySelectorOptions? options = null) => Page.QuerySelectorAsync(selector, options);

    public Task<IResponse?> ReloadAsync(PageReloadOptions? options = null) => Page.ReloadAsync(options);

    public Task RemoveLocatorHandlerAsync(ILocator locator) => Page.RemoveLocatorHandlerAsync(locator);

    public Task RequestGCAsync() => Page.RequestGCAsync();

    public Task RouteAsync(string url, Action<IRoute> handler, PageRouteOptions? options = null) => Page.RouteAsync(url, handler, options);

    public Task RouteAsync(Regex url, Action<IRoute> handler, PageRouteOptions? options = null) => Page.RouteAsync(url, handler, options);

    public Task RouteAsync(Func<string, bool> url, Action<IRoute> handler, PageRouteOptions? options = null) => Page.RouteAsync(url, handler, options);

    public Task RouteAsync(string url, Func<IRoute, Task> handler, PageRouteOptions? options = null) => Page.RouteAsync(url, handler, options);

    public Task RouteAsync(Regex url, Func<IRoute, Task> handler, PageRouteOptions? options = null) => Page.RouteAsync(url, handler, options);

    public Task RouteAsync(Func<string, bool> url, Func<IRoute, Task> handler, PageRouteOptions? options = null) => Page.RouteAsync(url, handler, options);

    public Task RouteFromHARAsync(string har, PageRouteFromHAROptions? options = null) => Page.RouteFromHARAsync(har, options);

    public Task RouteWebSocketAsync(string url, Action<IWebSocketRoute> handler) => Page.RouteWebSocketAsync(url, handler);

    public Task RouteWebSocketAsync(Regex url, Action<IWebSocketRoute> handler) => Page.RouteWebSocketAsync(url, handler);

    public Task RouteWebSocketAsync(Func<string, bool> url, Action<IWebSocketRoute> handler) => Page.RouteWebSocketAsync(url, handler);

    public Task<IConsoleMessage> RunAndWaitForConsoleMessageAsync(Func<Task> action, PageRunAndWaitForConsoleMessageOptions? options = null) => Page.RunAndWaitForConsoleMessageAsync(action, options);

    public Task<IDownload> RunAndWaitForDownloadAsync(Func<Task> action, PageRunAndWaitForDownloadOptions? options = null) => Page.RunAndWaitForDownloadAsync(action, options);

    public Task<IFileChooser> RunAndWaitForFileChooserAsync(Func<Task> action, PageRunAndWaitForFileChooserOptions? options = null) => Page.RunAndWaitForFileChooserAsync(action, options);

    [Obsolete]
    public Task<IResponse?> RunAndWaitForNavigationAsync(Func<Task> action, PageRunAndWaitForNavigationOptions? options = null) => Page.RunAndWaitForNavigationAsync(action, options);

    public Task<IPage> RunAndWaitForPopupAsync(Func<Task> action, PageRunAndWaitForPopupOptions? options = null) => Page.RunAndWaitForPopupAsync(action, options);

    public Task<IRequest> RunAndWaitForRequestAsync(Func<Task> action, string urlOrPredicate, PageRunAndWaitForRequestOptions? options = null) => Page.RunAndWaitForRequestAsync(action, urlOrPredicate, options);

    public Task<IRequest> RunAndWaitForRequestAsync(Func<Task> action, Regex urlOrPredicate, PageRunAndWaitForRequestOptions? options = null) => Page.RunAndWaitForRequestAsync(action, urlOrPredicate, options);

    public Task<IRequest> RunAndWaitForRequestAsync(Func<Task> action, Func<IRequest, bool> urlOrPredicate, PageRunAndWaitForRequestOptions? options = null) => Page.RunAndWaitForRequestAsync(action, urlOrPredicate, options);

    public Task<IRequest> RunAndWaitForRequestFinishedAsync(Func<Task> action, PageRunAndWaitForRequestFinishedOptions? options = null) => Page.RunAndWaitForRequestFinishedAsync(action, options);

    public Task<IResponse> RunAndWaitForResponseAsync(Func<Task> action, string urlOrPredicate, PageRunAndWaitForResponseOptions? options = null) => Page.RunAndWaitForResponseAsync(action, urlOrPredicate, options);

    public Task<IResponse> RunAndWaitForResponseAsync(Func<Task> action, Regex urlOrPredicate, PageRunAndWaitForResponseOptions? options = null) => Page.RunAndWaitForResponseAsync(action, urlOrPredicate, options);

    public Task<IResponse> RunAndWaitForResponseAsync(Func<Task> action, Func<IResponse, bool> urlOrPredicate, PageRunAndWaitForResponseOptions? options = null) => Page.RunAndWaitForResponseAsync(action, urlOrPredicate, options);

    public Task<IWebSocket> RunAndWaitForWebSocketAsync(Func<Task> action, PageRunAndWaitForWebSocketOptions? options = null) => Page.RunAndWaitForWebSocketAsync(action, options);

    public Task<IWorker> RunAndWaitForWorkerAsync(Func<Task> action, PageRunAndWaitForWorkerOptions? options = null) => Page.RunAndWaitForWorkerAsync(action, options);

    public Task<byte[]> ScreenshotAsync(PageScreenshotOptions? options = null) => Page.ScreenshotAsync(options);

    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, string values, PageSelectOptionOptions? options = null) => Page.SelectOptionAsync(selector, values, options);

    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IElementHandle values, PageSelectOptionOptions? options = null) => Page.SelectOptionAsync(selector, values, options);

    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IEnumerable<string> values, PageSelectOptionOptions? options = null) => Page.SelectOptionAsync(selector, values, options);

    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, SelectOptionValue values, PageSelectOptionOptions? options = null) => Page.SelectOptionAsync(selector, values, options);

    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IEnumerable<IElementHandle> values, PageSelectOptionOptions? options = null) => Page.SelectOptionAsync(selector, values, options);

    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IEnumerable<SelectOptionValue> values, PageSelectOptionOptions? options = null) => Page.SelectOptionAsync(selector, values, options);

    public Task SetCheckedAsync(string selector, bool checkedState, PageSetCheckedOptions? options = null) => Page.SetCheckedAsync(selector, checkedState, options);

    public Task SetContentAsync(string html, PageSetContentOptions? options = null) => Page.SetContentAsync(html, options);

    public void SetDefaultNavigationTimeout(float timeout) => Page.SetDefaultNavigationTimeout(timeout);

    public void SetDefaultTimeout(float timeout) => Page.SetDefaultTimeout(timeout);

    public Task SetExtraHTTPHeadersAsync(IEnumerable<KeyValuePair<string, string>> headers) => Page.SetExtraHTTPHeadersAsync(headers);

    public Task SetInputFilesAsync(string selector, string files, PageSetInputFilesOptions? options = null) => Page.SetInputFilesAsync(selector, files, options);

    public Task SetInputFilesAsync(string selector, IEnumerable<string> files, PageSetInputFilesOptions? options = null) => Page.SetInputFilesAsync(selector, files, options);

    public Task SetInputFilesAsync(string selector, FilePayload files, PageSetInputFilesOptions? options = null) => Page.SetInputFilesAsync(selector, files, options);

    public Task SetInputFilesAsync(string selector, IEnumerable<FilePayload> files, PageSetInputFilesOptions? options = null) => Page.SetInputFilesAsync(selector, files, options);

    public Task SetViewportSizeAsync(int width, int height) => Page.SetViewportSizeAsync(width, height);

    public Task TapAsync(string selector, PageTapOptions? options = null) => Page.TapAsync(selector, options);

    public Task<string?> TextContentAsync(string selector, PageTextContentOptions? options = null) => Page.TextContentAsync(selector, options);

    public Task<string> TitleAsync() => Page.TitleAsync();

    [Obsolete]
    public Task TypeAsync(string selector, string text, PageTypeOptions? options = null) => Page.TypeAsync(selector, text, options);

    public Task UncheckAsync(string selector, PageUncheckOptions? options = null) => Page.UncheckAsync(selector, options);

    public Task UnrouteAllAsync(PageUnrouteAllOptions? options = null) => Page.UnrouteAllAsync(options);

    public Task UnrouteAsync(string url, Action<IRoute>? handler = null) => Page.UnrouteAsync(url, handler);

    public Task UnrouteAsync(Regex url, Action<IRoute>? handler = null) => Page.UnrouteAsync(url, handler);

    public Task UnrouteAsync(Func<string, bool> url, Action<IRoute>? handler = null) => Page.UnrouteAsync(url, handler);

    public Task UnrouteAsync(string url, Func<IRoute, Task> handler) => Page.UnrouteAsync(url, handler);

    public Task UnrouteAsync(Regex url, Func<IRoute, Task> handler) => Page.UnrouteAsync(url, handler);

    public Task UnrouteAsync(Func<string, bool> url, Func<IRoute, Task> handler) => Page.UnrouteAsync(url, handler);

    public Task<IConsoleMessage> WaitForConsoleMessageAsync(PageWaitForConsoleMessageOptions? options = null) => Page.WaitForConsoleMessageAsync(options);

    public Task<IDownload> WaitForDownloadAsync(PageWaitForDownloadOptions? options = null) => Page.WaitForDownloadAsync(options);

    public Task<IFileChooser> WaitForFileChooserAsync(PageWaitForFileChooserOptions? options = null) => Page.WaitForFileChooserAsync(options);

    public Task<IJSHandle> WaitForFunctionAsync(string expression, object? arg = null, PageWaitForFunctionOptions? options = null) => Page.WaitForFunctionAsync(expression, arg, options);

    public Task WaitForLoadStateAsync(LoadState? state = null, PageWaitForLoadStateOptions? options = null) => Page.WaitForLoadStateAsync(state, options);

    [Obsolete]
    public Task<IResponse?> WaitForNavigationAsync(PageWaitForNavigationOptions? options = null) => Page.WaitForNavigationAsync(options);

    public Task<IPage> WaitForPopupAsync(PageWaitForPopupOptions? options = null) => Page.WaitForPopupAsync(options);

    public Task<IRequest> WaitForRequestAsync(string urlOrPredicate, PageWaitForRequestOptions? options = null) => Page.WaitForRequestAsync(urlOrPredicate, options);

    public Task<IRequest> WaitForRequestAsync(Regex urlOrPredicate, PageWaitForRequestOptions? options = null) => Page.WaitForRequestAsync(urlOrPredicate, options);

    public Task<IRequest> WaitForRequestAsync(Func<IRequest, bool> urlOrPredicate, PageWaitForRequestOptions? options = null) => Page.WaitForRequestAsync(urlOrPredicate, options);

    public Task<IRequest> WaitForRequestFinishedAsync(PageWaitForRequestFinishedOptions? options = null) => Page.WaitForRequestFinishedAsync(options);

    public Task<IResponse> WaitForResponseAsync(string urlOrPredicate, PageWaitForResponseOptions? options = null) => Page.WaitForResponseAsync(urlOrPredicate, options);

    public Task<IResponse> WaitForResponseAsync(Regex urlOrPredicate, PageWaitForResponseOptions? options = null) => Page.WaitForResponseAsync(urlOrPredicate, options);

    public Task<IResponse> WaitForResponseAsync(Func<IResponse, bool> urlOrPredicate, PageWaitForResponseOptions? options = null) => Page.WaitForResponseAsync(urlOrPredicate, options);

    public Task<IElementHandle?> WaitForSelectorAsync(string selector, PageWaitForSelectorOptions? options = null) => Page.WaitForSelectorAsync(selector, options);

    public Task WaitForTimeoutAsync(float timeout) => Page.WaitForTimeoutAsync(timeout);

    public Task WaitForURLAsync(string url, PageWaitForURLOptions? options = null) => Page.WaitForURLAsync(url, options);

    public Task WaitForURLAsync(Regex url, PageWaitForURLOptions? options = null) => Page.WaitForURLAsync(url, options);

    public Task WaitForURLAsync(Func<string, bool> url, PageWaitForURLOptions? options = null) => Page.WaitForURLAsync(url, options);

    public Task<IWebSocket> WaitForWebSocketAsync(PageWaitForWebSocketOptions? options = null) => Page.WaitForWebSocketAsync(options);

    public Task<IWorker> WaitForWorkerAsync(PageWaitForWorkerOptions? options = null) => Page.WaitForWorkerAsync(options);

    #endregion

}


