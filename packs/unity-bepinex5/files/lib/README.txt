Game dlls go here.

No game folder was set when this project was made, so it cannot see the game's own classes yet.

Copy these from the game's <GameName>_Data\Managed folder into this folder:

    Assembly-CSharp.dll            the game's code (needed to use or patch game classes)
    Assembly-CSharp-firstpass.dll  only if the game has one

Every dll in this folder is referenced automatically. They are not copied to the build output and
are ignored by git: do not redistribute them.
