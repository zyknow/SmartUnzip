using Microsoft.Extensions.Localization;
using Volo.Abp.Localization;
using Volo.Abp;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmartUnzip;
public abstract class AbpComponentBase : OwningComponentBase
{

    protected ILoggerFactory LoggerFactory => LazyGetRequiredService(ref _loggerFactory)!;
    private ILoggerFactory? _loggerFactory;

    [Inject]
    protected IServiceProvider NonScopedServices { get; set; } = default!;


    protected IStringLocalizerFactory StringLocalizerFactory => LazyGetRequiredService(ref _stringLocalizerFactory)!;
    private IStringLocalizerFactory? _stringLocalizerFactory;

    protected IStringLocalizer L
    {
        get
        {
            if (_localizer == null)
            {
                _localizer = CreateLocalizer();
            }

            return _localizer;
        }
    }
    private IStringLocalizer? _localizer;

    private Type? _localizationResource = typeof(DefaultResource);

    protected Type? LocalizationResource
    {
        get => _localizationResource;
        set
        {
            _localizationResource = value;
            _localizer = null;
        }
    }

    protected virtual IStringLocalizer CreateLocalizer()
    {
        if (LocalizationResource != null)
        {
            return StringLocalizerFactory.Create(LocalizationResource);
        }

        var localizer = StringLocalizerFactory.CreateDefaultOrNull();
        if (localizer == null)
        {
            throw new AbpException($"Set {nameof(LocalizationResource)} or define the default localization resource type (by configuring the {nameof(AbpLocalizationOptions)}.{nameof(AbpLocalizationOptions.DefaultResourceType)}) to be able to use the {nameof(L)} object!");
        }

        return localizer;
    }

    protected TService LazyGetRequiredService<TService>(ref TService reference) => LazyGetRequiredService(typeof(TService), ref reference);


    protected TRef LazyGetRequiredService<TRef>(Type serviceType, ref TRef reference)
    {
        if (reference == null)
        {
            reference = (TRef)ScopedServices.GetRequiredService(serviceType);
        }

        return reference;
    }

    protected TService? LazyGetService<TService>(ref TService? reference) => LazyGetService(typeof(TService), ref reference);

    protected TRef? LazyGetService<TRef>(Type serviceType, ref TRef? reference)
    {
        if (reference == null)
        {
            reference = (TRef?)ScopedServices.GetService(serviceType);
        }

        return reference;
    }



    protected TService LazyGetNonScopedRequiredService<TService>(ref TService reference) => LazyGetNonScopedRequiredService(typeof(TService), ref reference);

    protected TRef LazyGetNonScopedRequiredService<TRef>(Type serviceType, ref TRef reference)
    {
        if (reference == null)
        {
            reference = (TRef)NonScopedServices.GetRequiredService(serviceType);
        }

        return reference;
    }

    protected TService? LazyGetNonScopedService<TService>(ref TService? reference) => LazyGetNonScopedService(typeof(TService), ref reference);

    protected TRef? LazyGetNonScopedService<TRef>(Type serviceType, ref TRef? reference)
    {
        if (reference == null)
        {
            reference = (TRef?)NonScopedServices.GetService(serviceType);
        }

        return reference;
    }


}
