using ImageAutomate.Execution.MemoryAndAccessTests.LargeSources;

namespace ImageAutomate.Execution.MemoryAndAccessTests;

// Matrix:
// Resolutions: LowRes (640x480), MidRes (1280x720), HighRes (1920x1080)
// Workloads:
//   Small: Count=1, Batch=1
//   Medium: Count=20, Batch=5
//   High: Count=100, Batch=10
//   HighPlus: Count=100, Batch=50
//   Extreme: Count=500, Batch=100 (pushes ~4.5GB)
//   Ultra: Count=1000, Batch=200 (pushes ~9GB)
//   Maximum: Count=1300, Batch=250 (pushes ~12GB, near 16GB hard limit)

#region LowRes (640x480)

public class LowRes_Small : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 1;
    protected override int BatchSize => 1;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 640, 480, ExpectedTotalItems);
}

public class LowRes_Medium : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 20;
    protected override int BatchSize => 5;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 640, 480, ExpectedTotalItems);
}

public class LowRes_High : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 100;
    protected override int BatchSize => 10;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 640, 480, ExpectedTotalItems);
}

public class LowRes_HighPlus : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 100;
    protected override int BatchSize => 50;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 640, 480, ExpectedTotalItems);
}

#endregion

#region MidRes (1280x720)

public class MidRes_Small : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 1;
    protected override int BatchSize => 1;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1280, 720, ExpectedTotalItems);
}

public class MidRes_Medium : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 20;
    protected override int BatchSize => 5;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1280, 720, ExpectedTotalItems);
}

public class MidRes_High : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 100;
    protected override int BatchSize => 10;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1280, 720, ExpectedTotalItems);
}

public class MidRes_HighPlus : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 100;
    protected override int BatchSize => 50;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1280, 720, ExpectedTotalItems);
}

#endregion

#region HighRes (1920x1080)

public class HighRes_Small : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 1;
    protected override int BatchSize => 1;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1920, 1080, ExpectedTotalItems);
}

public class HighRes_Medium : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 20;
    protected override int BatchSize => 5;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1920, 1080, ExpectedTotalItems);
}

public class HighRes_High : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 100;
    protected override int BatchSize => 10;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1920, 1080, ExpectedTotalItems);
}

public class HighRes_HighPlus : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 100;
    protected override int BatchSize => 50;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1920, 1080, ExpectedTotalItems);
}

public class HighRes_Extreme : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 500;
    protected override int BatchSize => 100;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1920, 1080, ExpectedTotalItems);
}

public class HighRes_Ultra : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 1000;
    protected override int BatchSize => 200;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1920, 1080, ExpectedTotalItems);
}

public class HighRes_Maximum : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 1300;
    protected override int BatchSize => 250;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1920, 1080, ExpectedTotalItems);
}

public class HighRes_MaximumThrottled : PerformanceTestBase
{
    protected override int ExpectedTotalItems => 1300;
    protected override int BatchSize => 16;
    protected override LargeSource CreateSource(string name) => new LargeSource(name, 1920, 1080, ExpectedTotalItems);
}

#endregion
