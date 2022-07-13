using System.Diagnostics;
using System.Reflection;

namespace HanumanInstitute.FFmpeg.UnitTests;

public class FakeProcessService : ProcessService
{

    public FakeProcessService() : base(new FakeProcessManager(null), null, new FileInfoParserFactory(), new FakeProcessFactory(), new FakeFileSystemService())
    {
    }

    /// <summary>
    /// Returns the list of instances that were created by the factory.
    /// </summary>
    public List<IProcessWorker> Instances { get; private set; } = new();

    public override IProcessWorker CreateProcess(object owner, ProcessOptions options = null, ProcessStartedEventHandler callback = null)
    {
        var result = base.CreateProcess(owner, options, callback);
        Instances.Add(result);
        return result;
    }

    public override IProcessWorkerEncoder CreateEncoder(object owner = null, ProcessOptionsEncoder options = null, ProcessStartedEventHandler callback = null)
    {
        var result = base.CreateEncoder(owner, options, callback);
        result.ProcessCompleted += (_, _) =>
        {
            if (result.FileInfo is FileInfoFFmpeg info && !info.FileStreams.Any())
            {
                // If no data was fed into the process, this will initialize FileStreams.
                var mockP = Mock.Get(result.WorkProcess);
                mockP.Raise(x => x.ErrorDataReceived += null, CreateMockDataReceivedEventArgs(null));
                mockP.Raise(x => x.OutputDataReceived += null, CreateMockDataReceivedEventArgs(null));
                if (!info.FileStreams.Any())
                {
                    info.FileStreams.Add(new MediaVideoStreamInfo());
                    info.FileStreams.Add(new MediaAudioStreamInfo());
                }
            }
        };
        Instances.Add(result);
        return result;
    }

    /// <summary>
    /// Feeds a sample output into a mock process.
    /// </summary>
    /// <param name="p">The process manager to feed data into..</param>
    /// <param name="output">The sample output to feed.</param>
    public static void FeedOutputToProcess(IProcessWorker p, string output)
    {
        var mockP = Mock.Get<IProcess>(p.WorkProcess);
        using (var sr = new StringReader(output))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                mockP.Raise(x => x.ErrorDataReceived += null, CreateMockDataReceivedEventArgs(line));
            }
        }
        mockP.Raise(x => x.ErrorDataReceived += null, CreateMockDataReceivedEventArgs(null));
    }

    /// <summary>
    /// Since DataReceivedEventArgs can't be directly created, create an instance through reflection.
    /// </summary>
    public static DataReceivedEventArgs CreateMockDataReceivedEventArgs(string testData)
    {
        var mockEventArgs =
            (DataReceivedEventArgs)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(DataReceivedEventArgs));

        var eventFields = typeof(DataReceivedEventArgs)
            .GetFields(
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

        if (eventFields.Length > 0)
        {
            eventFields[0].SetValue(mockEventArgs, testData);
        }
        else
        {
            throw new ApplicationException("Failed to find _data field!");
        }

        return mockEventArgs;
    }
}
