using System;
using System.Collections.Generic;
using Moq;

namespace HanumanInstitute.FFmpeg.UnitTests
{
    public class FakeUserInterfaceManagerBase : UserInterfaceManagerBase
    {
        public List<IUserInterfaceWindow> Instances { get; private set; } = new List<IUserInterfaceWindow>();

        public override IUserInterfaceWindow CreateUI(object owner, string title, bool autoClose)
        {
            var result = Mock.Of<IUserInterfaceWindow>();
            Instances.Add(result);
            return result;
        }

        public override void DisplayError(object owner, IProcessWorker host) { }
    }
}
