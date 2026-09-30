using System;
using System.Threading;
using System.Threading.Tasks;

namespace Resolved.Scripts;

/// <summary>
/// 마지막 입력 후 <paramref name="delay"/> 동안 새 입력이 없을 때만 <paramref name="func"/>를 실행합니다.
/// 그 사이에 새 입력이 들어오면 이전 결과는 버립니다.
/// UI 스레드에서 호출하면 <see cref="OnResult"/>와 <see cref="OnError"/>도 UI 스레드에서 발생합니다.
/// </summary>
class Debouncer<InType,OutType>(Func<InType , CancellationToken , Task<OutType>> func , TimeSpan delay)
{
    private CancellationTokenSource? _cts = null;

    public event EventHandler<OutType>? OnResult = null;
    public event EventHandler<Exception>? OnError = null;

    public async void Push(InType input)
    {
        _cts?.Cancel();
        CancellationTokenSource cts = _cts = new();
        try
        {
            await Task.Delay(delay , cts.Token);
            OutType ret = await func(input , cts.Token);
            if (!cts.IsCancellationRequested)
                OnResult?.Invoke(this , ret);
        } catch (OperationCanceledException)
        {
        } catch (Exception ex)
        {
            if (!cts.IsCancellationRequested)
                OnError?.Invoke(this , ex);
        } finally
        {
            if (_cts == cts)
                _cts = null;
            cts.Dispose();
        }
    }

    public void Cancel()
    {
        _cts?.Cancel();
        _cts = null;
    }
}
