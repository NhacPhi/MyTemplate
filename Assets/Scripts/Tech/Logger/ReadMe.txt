Improves game performance

When building the game, all Debug.Log statements are removed, preventing unnecessary resource usage.

Logging frequently in a game can slow down performance.

Keeps logs in Editor for debugging

Logs are still visible while developing in Unity Editor.

Easier log management

Instead of writing Debug.Log everywhere, you only need to call LogCommon.Log().

If you need to change how logging works, you only modify this class, not every log statement.

