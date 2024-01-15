
using SmartUnzip.Localization.Localization;
using Volo.Abp.Localization;

namespace SmartUnzip.Blazor;

public abstract class SmartUnzipComponentBase: AbpComponentBase
{
    protected SmartUnzipComponentBase()
    {
        LocalizationResource = typeof(SmartUnzipResource);
    }
}
