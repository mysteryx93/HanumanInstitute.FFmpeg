namespace HanumanInstitute.FFmpeg.IntegrationTests;

public class MediaScriptTests
{
    private readonly OutputFeeder _feed;

    public MediaScriptTests(ITestOutputHelper output)
    {
        _feed = new OutputFeeder(output);
    }

    private IMediaScript SetupScript()
    {
        var factory = FactoryConfig.CreateWithConfig();
        return new MediaScript(factory, new FileSystemService());
    }

    [Theory]
    [InlineData(AppPaths.Avisynth)]
    public void RunAvisynth_Valid_ResultSuccess(string source)
    {
        var script = SetupScript();
        var src = AppPaths.GetInputFile(source);

        var result = script.RunAvisynth(src, null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
    }

    [Theory]
    [InlineData(AppPaths.VapourSynth)]
    public void RunVapourSynth_Valid_ResultSuccess(string source)
    {
        var script = SetupScript();
        var src = AppPaths.GetInputFile(source);

        var result = script.RunVapourSynth(src, null, _feed.RunCallback);

        Assert.Equal(CompletionStatus.Success, result);
    }


}
