using tetofo.DesignPattern;
using tetofo.EventBus;
using Microsoft.Extensions.Hosting;

namespace tetofo.Stem;

public interface IStem : IAuthor, ICallback, IDisposable, IEventBus, ISupplier<IHost>
{
    bool IsDisposed { get; set; }
    void Subscribe(IStem stem);
    void Subscribe(IStem stem, IEvent iEvent);
    void Unsubscribe(IStem stem);
    void Unsubscribe(IStem stem, IEvent iEvent);
}