# Lock-Provider
A generic resource locking mechanism to handle concurrency. The key base lock provider facilitates to block resources safely per session.

## Usage
```csharp
using (var locker = new LockProvider<string>(key))
{
    // Add logic to be blocking
}
```
