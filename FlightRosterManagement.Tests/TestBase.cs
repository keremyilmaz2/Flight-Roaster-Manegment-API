namespace FlightRosterManagement.Tests;

/// <summary>
/// Test sınıfları için temel sınıf.
/// Ortak setup ve teardown işlemlerini içerir.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected TestBase()
    {
        // Test başlangıç zamanını kaydet
        TestStartTime = DateTime.UtcNow;
    }

    protected DateTime TestStartTime { get; }

    public virtual void Dispose()
    {
        // Cleanup işlemleri
        GC.SuppressFinalize(this);
    }
}
