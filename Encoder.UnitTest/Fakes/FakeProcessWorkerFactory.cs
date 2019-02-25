using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using EmergenceGuardian.Encoder.Services;
using Moq;

namespace EmergenceGuardian.Encoder.UnitTests {
    public class FakeProcessWorkerFactory : ProcessWorkerFactory {

        public FakeProcessWorkerFactory() : base(new FakeMediaConfig(), new FileInfoParserFactory(), new FakeProcessFactory(), new FakeFileSystemService()) {
        }

        /// <summary>
        /// Returns the list of instances that were created by the factory.
        /// </summary>
        public List<IProcessWorker> Instances { get; private set; } = new List<IProcessWorker>();

        public override IProcessWorker Create(ProcessOptions options = null, ProcessStartedEventHandler callback = null) {
            var Result = base.Create(options, callback);
            Instances.Add(Result);
            return Result;
        }

        public override IProcessWorkerEncoder CreateEncoder(ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null) {
            var Result = base.CreateEncoder(options, callback);
            Result.ProcessCompleted += (s, e) => {
                FileInfoFFmpeg Info = Result.FileInfo as FileInfoFFmpeg;
                if (Info != null && Info.FileStreams == null) {
                    // If no data was fed into the process, this will initialize FileStreams.
                    var MockP = Mock.Get<IProcess>(Result.WorkProcess);
                    MockP.Raise(x => x.ErrorDataReceived += null, CreateMockDataReceivedEventArgs(null));
                    MockP.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs(null));
                    if (Info.FileStreams != null) {
                        Info.FileStreams.Add(new MediaVideoStreamInfo());
                        Info.FileStreams.Add(new MediaAudioStreamInfo());
                    }
                }
            };
            Instances.Add(Result);
            return Result;
        }

        /// <summary>
        /// Feeds a sample output into a mock process.
        /// </summary>
        /// <param name="p">The process manager to feed data into..</param>
        /// <param name="output">The sample output to feed.</param>
        public static void FeedOutputToProcess(IProcessWorker p, string output) {
            var MockP = Mock.Get<IProcess>(p.WorkProcess);
            using (StringReader sr = new StringReader(output)) {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    MockP.Raise(x => x.ErrorDataReceived += null, CreateMockDataReceivedEventArgs(line));
                }
            }
            MockP.Raise(x => x.ErrorDataReceived += null, CreateMockDataReceivedEventArgs(null));
        }

        /// <summary>
        /// Since DataReceivedEventArgs can't be directly created, create an instance through reflection.
        /// </summary>
        public static DataReceivedEventArgs CreateMockDataReceivedEventArgs(string TestData) {
            DataReceivedEventArgs MockEventArgs =
                (DataReceivedEventArgs)System.Runtime.Serialization.FormatterServices
                 .GetUninitializedObject(typeof(DataReceivedEventArgs));

            FieldInfo[] EventFields = typeof(DataReceivedEventArgs)
                .GetFields(
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);

            if (EventFields.Length > 0) {
                EventFields[0].SetValue(MockEventArgs, TestData);
            } else {
                throw new ApplicationException(
                    "Failed to find _data field!");
            }

            return MockEventArgs;
        }
    }
}
