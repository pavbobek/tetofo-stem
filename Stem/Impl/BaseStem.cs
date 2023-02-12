using tetofo.EventBus;
using tetofo.EventBus.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tetofo.DesignPattern;
using Microsoft.Extensions.Logging;

namespace tetofo.Stem.Impl;

public abstract class BaseStem : IStem
{
    private ISet<IStem>? _globalSubscription;
    private IDictionary<Type, ISet<IStem>>? _eventSubscription;

    public IHost Host { get; set; }
    public bool IsDisposed { get; set; }
    public ISet<Type>? WhiteList { get; set; }

    public BaseStem()
    {
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
        .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Trace))
        .ConfigureServices((_, services) => 
        {
            services.AddSingleton<IAuthor>(this);
            services.AddSingleton<ICallback>(this);
            services.AddSingleton<IEventBus, BaseEventBus>();
            services.AddSingleton<ISupplier<IHost>>(this);
            RegisterServices(services);
        })
        .Build();
        Host.RunAsync();
    }

    public IHost Get()
    {
        return Host;
    }

    protected abstract void RegisterServices(IServiceCollection serviceCollection);

    public void Callback(IEvent t)
    {
        if(!this.Equals(t.Author))
        {
            return;
        }
        if (WhiteList != null && WhiteList.Count != 0)
        {
            if (!WhiteList.Contains(t.GetType()))
            {
                return;
            }
        }
        StemSetCallback(_globalSubscription, t);
        StemSetCallback(_eventSubscription?[t.GetType()],t);
    }

    private void StemSetCallback(ISet<IStem>? stems, IEvent iEvent)
    {
        if (stems == null)
        {
            return;
        }
        foreach (IStem stem in stems)
        {
            if (!stem.IsDisposed)
            {
                IEventBus? eventBus = stem.Get().Services.GetService<IEventBus>();
                eventBus?.Event(iEvent);
            }
        }
    }

    public void Dispose()
    {
        IsDisposed = true;
        _globalSubscription?.Clear();
        _eventSubscription?.Clear();
        Host.Dispose();
    }

    public void Subscribe(IStem stem)
    {
        if (_globalSubscription == null)
        {
            _globalSubscription = new HashSet<IStem>();
        }
        _globalSubscription.Add(stem);
    }

    public void Subscribe(IStem stem, IEvent iEvent)
    {
        if (_eventSubscription == null)
        {
            _eventSubscription = new Dictionary<Type, ISet<IStem>>();
        }
        if (!_eventSubscription.ContainsKey(iEvent.GetType()))
        {
            _eventSubscription.Add(iEvent.GetType(), new HashSet<IStem>());
        }
        _eventSubscription[iEvent.GetType()].Add(stem);
    }

    public void Unsubscribe(IStem stem)
    {
        if (_globalSubscription == null)
        {
            return;
        }
        _globalSubscription.Remove(stem);
        if (_globalSubscription.Count == 0)
        {
            _globalSubscription = null;
        }
    }

    public void Unsubscribe(IStem stem, IEvent iEvent)
    {
        if (_eventSubscription == null)
        {
            return;
        }
        if (!_eventSubscription.ContainsKey(iEvent.GetType()))
        {
            return;
        }
        _eventSubscription[iEvent.GetType()].Remove(stem);
        if (_eventSubscription[iEvent.GetType()].Count == 0)
        {
            _eventSubscription.Remove(iEvent.GetType());
        }
        if (_eventSubscription.Count == 0)
        {
            _eventSubscription = null;
        }
    }

    public void Event<S>(S s) where S : IEvent
    {
        IEventBus? eventBus = Host.Services.GetService<IEventBus>();
        eventBus?.Event(s);
    }
}