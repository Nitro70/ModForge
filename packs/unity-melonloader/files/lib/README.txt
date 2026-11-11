Game dlls go here.

No game folder was set when this project was made, so it cannot see the game's own classes yet.
{{#if Backend == il2cpp}}
The project still builds: code inside "#if HAS_UNITY" is simply left out until the dlls are here.

An IL2CPP game has no Assembly-CSharp.dll of its own. MelonLoader generates .NET versions of Unity and
the game's classes the first time the game starts with it installed, and writes them to:

    <game folder>\MelonLoader\Il2CppAssemblies

Copy every .dll from that folder into this folder (UnityEngine.CoreModule.dll and Il2Cppmscorlib.dll
must be among them; Assembly-CSharp.dll holds the game's own code).

Copy them again after a game update.
{{#else}}

Copy these from the game's <GameName>_Data\Managed folder into this folder:

    Assembly-CSharp.dll            the game's code (needed to use or patch game classes)
    Assembly-CSharp-firstpass.dll  only if the game has one
{{/if}}

Every dll in this folder is referenced automatically. They are not copied to the build output and
are ignored by git: do not redistribute them.
