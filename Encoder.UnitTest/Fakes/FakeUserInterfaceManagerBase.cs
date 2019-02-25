using System;
using System.Collections.Generic;
using Moq;

namespace EmergenceGuardian.Encoder.UnitTests {
    public class FakeUserInterfaceManagerBase : UserInterfaceManagerBase {
        public List<IUserInterfaceWindow> Instances = new List<IUserInterfaceWindow>();

        public override IUserInterfaceWindow CreateUI(string title, bool autoClose) {
            var Result = Mock.Of<IUserInterfaceWindow>();
            Instances.Add(Result);
            return Result;
        }

        public override void DisplayError(IProcessWorker host) { }
    }
}
