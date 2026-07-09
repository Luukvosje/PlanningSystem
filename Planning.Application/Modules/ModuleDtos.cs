using Planning.Domain.Enums;
using Planning.Domain.Modules;

namespace Planning.Application.Modules;

public sealed record ModuleSettingResponse(AppModule Key, bool IsEnabled);

public sealed record UpdateModulesRequest(bool Planning, bool Klant, bool Beheer);
