using System;
using System.Threading.Tasks;

namespace Resolved.Scripts;

class Debouncer<InType,OutType>(Func<InType,OutType> func) where InType : class
{
    readonly Func<InType,OutType> func = func;

    private InType? _current = null;
    public InType? Current {
        get => _current;
        set {
            _current = value;
            updateTask ??= Task.Run(Update);
        }
    }
    public event EventHandler<OutType>? OnResult = null;
    Task? updateTask = null;

    private void Update()
    {
        while(_current != null)
        {
            InType input = _current;
            OutType ret = func(input);
            if (input.Equals(_current))
            {
                _current = null;
                OnResult?.Invoke(null , ret);
            }
        }

        updateTask = null;
    }
}
