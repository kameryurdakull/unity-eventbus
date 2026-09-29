# Event Bus for Unity

Tür güvenli bir Unity Event Bus paketi. Senkron olayları `Publish`, UniTask ile asenkron olayları `PublishAsync` üzerinden yayınlar; VContainer ile tekil servis olarak kaydedilebilir.

## Kurulum

Unity 2022.3 veya üzeri ve Git gerekir. Unity Package Manager > **Add package from git URL...** üzerinden sırayla şu adresleri ekleyin:

1. UniTask: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11`
2. VContainer: `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.19.0`
3. Event Bus: `https://github.com/kameryurdakull/unity-eventbus.git?path=/Packages/com.kameryurdakull.eventbus#v1.0.0`

Unity, bir Git paketinin `package.json` dosyasından başka Git paketlerini otomatik kurmaz. Bu nedenle UniTask ve VContainer'ı Event Bus'tan önce projeye eklemek gerekir.

## Kullanım

Bir `LifetimeScope` içinde servisi kaydedin:

```csharp
using EventSystem;
using VContainer;
using VContainer.Unity;

public sealed class GameScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEventBus();
    }
}
```

Servisi constructor injection ile alın:

```csharp
using System;
using Cysharp.Threading.Tasks;
using EventSystem;
using UnityEngine;

public readonly struct PlayerDamaged
{
    public int Amount { get; }
    public PlayerDamaged(int amount) => Amount = amount;
}

public sealed class DamageListener : IDisposable
{
    private readonly IEventBus _eventBus;

    public DamageListener(IEventBus eventBus)
    {
        _eventBus = eventBus;
        _eventBus.Subscribe<PlayerDamaged>(OnDamage);
        _eventBus.SubscribeAsync<PlayerDamaged>(OnDamageAsync);
    }

    public void Dispose()
    {
        _eventBus.Unsubscribe<PlayerDamaged>(OnDamage);
        _eventBus.UnsubscribeAsync<PlayerDamaged>(OnDamageAsync);
    }

    private void OnDamage(PlayerDamaged message) => Debug.Log(message.Amount);
    private UniTask OnDamageAsync(PlayerDamaged message) => UniTask.CompletedTask;
}

// Bir başka sınıfta, enjekte edilen IEventBus ile:
// _eventBus.Publish(new PlayerDamaged(10));
// await _eventBus.PublishAsync(new PlayerDamaged(10));
```

`Publish` yalnızca senkron dinleyicileri çağırır. `PublishAsync` önce senkron, ardından asenkron dinleyicileri kayıt sırasıyla çağırıp bekler. Dinleyici hataları çağırana iletilir. Abonelikten yaşam döngüsü sonunda çıkın. Paket, Unity ana iş parçacığında kullanım için tasarlanmıştır.

Package Manager'daki **Samples > Basic Usage > Import** ile çalışır bir VContainer örneği alın. İçe aktarılan `ExampleLifetimeScope` bileşenini sahnedeki boş bir GameObject'e ekleyin.

Paketin kaynakları: [`Packages/com.kameryurdakull.eventbus`](Packages/com.kameryurdakull.eventbus). EditMode testleri paket içinde bulunur.
