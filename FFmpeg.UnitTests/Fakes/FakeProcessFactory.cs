using System.Diagnostics;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace HanumanInstitute.FFmpeg.UnitTests;

public class FakeProcessFactory : IProcessFactory
{
    public virtual IProcess Create() => Create(null);

    public virtual IProcess Create(Process process)
    {
        var result = new Mock<IProcess>();
        result.Setup(x => x.StartInfo).Returns(new ProcessStartInfo());
        result.Setup(x => x.HasExited).Returns(false);
        result.Setup(x => x.WaitForExit(It.IsAny<int>())).Callback(() => result.Setup(x => x.HasExited).Returns(true));
        return result.Object;
    }
}
