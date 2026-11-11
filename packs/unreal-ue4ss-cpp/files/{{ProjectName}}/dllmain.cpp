// {{ModName}} {{Version}} by {{Author}}
// A UE4SS C++ mod. UE4SS loads dlls\main.dll from the mod folder, calls start_mod() to create the mod object
// and uninstall_mod() to destroy it.

#include <Mod/CppUserModBase.hpp>
#include <DynamicOutput/DynamicOutput.hpp>
#include <Unreal/UObjectGlobals.hpp>
#include <Unreal/UObject.hpp>

using namespace RC;
using namespace RC::Unreal;

class {{ProjectName}} : public RC::CppUserModBase
{
public:
    {{ProjectName}}() : CppUserModBase()
    {
        ModName = STR("{{ProjectName}}");
        ModVersion = STR("{{Version}}");
        ModDescription = STR("{{Description|str}}");
        ModAuthors = STR("{{Author|str}}");
        // Do not change this unless you want to target a UE4SS version
        // other than the one you're currently building with somehow.
        //ModIntendedSDKVersion = STR("2.6");

        // Runs while UE4SS is still starting. Nothing from the Unreal namespace may be used yet.
        // The message goes to UE4SS.log and to both UE4SS consoles.
        Output::send<LogLevel::Verbose>(STR("[{{ProjectName}}] loaded {{Version}}\n"));
    }

    ~{{ProjectName}}() override
    {
    }

    // The Unreal module is ready. From here on the Unreal namespace can be used.
    auto on_unreal_init() -> void override
    {
        auto Object = UObjectGlobals::StaticFindObject<UObject*>(nullptr, nullptr, STR("/Script/CoreUObject.Object"));
        Output::send<LogLevel::Verbose>(STR("[{{ProjectName}}] Object Name: {}\n"), Object->GetFullName());

        // TODO: your code here
    }

    // Called by UE4SS on every one of its update ticks.
    auto on_update() -> void override
    {
    }
};

#define {{ApiMacro}} __declspec(dllexport)
extern "C"
{
    {{ApiMacro}} RC::CppUserModBase* start_mod()
    {
        return new {{ProjectName}}();
    }

    {{ApiMacro}} void uninstall_mod(RC::CppUserModBase* mod)
    {
        delete mod;
    }
}
