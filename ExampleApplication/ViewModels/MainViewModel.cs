using System.Collections.Generic;
using System.IO;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HanumanInstitute.Validators;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HanumanInstitute.FFmpegExampleApplication.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly IDialogService _dialogService;
    private readonly IUserInterfaceManager _uiManager;
    private readonly IMediaEncoder _encoder;
    private readonly IMediaMuxer _muxer;
    private static int s_jobId;

    public MainViewModel(IDialogService dialogService, IUserInterfaceManager uiManager, IEncoderService EncoderService)
    {
        _dialogService = dialogService;
        _uiManager = uiManager;
        _encoder = EncoderService.GetMediaEncoder(this);
        _muxer = EncoderService.GetMediaMuxer(this);
    }

    [Reactive]
    public string SourcePath { get; set; } = @"/run/media/hanuman/Storage-ntfs/NaturalGrounding/AOA/Confused.mp4";

    [Reactive]
    public string DestinationPath { get; set; } = @"/home/hanuman/Downloads/TestOut.mp4";

    public ICommand ShowOpenFile => _showOpenFile ??= ReactiveCommand.CreateFromTask(ShowOpenFileImpl);
    private ICommand? _showOpenFile;
    private async Task ShowOpenFileImpl()
    {
        var defaultPath = TryGetDirectory(SourcePath);
        var settings = new OpenFileDialogSettings()
        {
            InitialDirectory = defaultPath.HasValue() ? Path.GetDirectoryName(defaultPath)! : string.Empty
        };
        var result = await _dialogService.ShowOpenFileDialogAsync(this, settings).ConfigureAwait(true);

        if (result != null)
        {
            SourcePath = result;
        }
    }

    public ICommand ShowSaveFile => _showSaveFile ??= ReactiveCommand.CreateFromTask(ShowSaveFileImpl);
    private ICommand? _showSaveFile;
    private async Task ShowSaveFileImpl()
    {
        var defaultPath = TryGetDirectory(DestinationPath);
        var settings = new SaveFileDialogSettings()
        {
            Filters = new List<FileFilter>(new[]
            {
               new FileFilter("MP4 Files", "mp4"), 
               new FileFilter("All Files", "*")
            }),
            InitialDirectory = defaultPath.HasValue() ? Path.GetDirectoryName(defaultPath)! : string.Empty
        };
        var result = await _dialogService.ShowSaveFileDialogAsync(this, settings).ConfigureAwait(true);

        if (result != null)
        {
            DestinationPath = result;
        }
    }

    private static string? TryGetDirectory(string path)
    {
        try
        {
            return Path.GetDirectoryName(path);
        }
        catch (ArgumentException) { }
        catch (PathTooLongException) { }
        return null;
    }

    private bool Validate()
    {
        return !string.IsNullOrEmpty(SourcePath) && File.Exists(SourcePath) && !string.IsNullOrEmpty(DestinationPath) && TryGetDirectory(SourcePath) != null && TryGetDirectory(DestinationPath) != null;
    }

    public ICommand RunSimpleTask => _runSimpleTask ??= ReactiveCommand.CreateFromTask(RunSimpleTaskImpl);
    private ICommand? _runSimpleTask;
    private async Task RunSimpleTaskImpl()
    {
        if (Validate())
        {
            var options = new ProcessOptionsEncoder(ProcessDisplayMode.Interface, "Encoding to H264/AAC (Simple)");
            await Task.Run(() =>
            {
                _encoder.EncodeFFmpeg(SourcePath, DestinationPath, "h264", "aac", null, options);
            }).ConfigureAwait(false);
        }
    }

    public ICommand RunComplexTask => _runComplexTask ??= ReactiveCommand.CreateFromTask(RunComplexTaskImpl);
    private ICommand? _runComplexTask;
    private async Task RunComplexTaskImpl()
    {
        if (Validate())
        {
            var result = await Task.Run(() => ExecuteComplex(SourcePath, DestinationPath)).ConfigureAwait(false);
            await _dialogService.ShowMessageBoxAsync(this, result.ToString(), "Encoding Result").ConfigureAwait(false);
        }
    }

    private CompletionStatus ExecuteComplex(string src, string dst)
    {
        var dstEncode = GetPathWithoutExtension(dst) + "_.mp4";
        var dstExtract = GetPathWithoutExtension(dst) + "_.mkv";
        var dstAac = GetPathWithoutExtension(dst) + "_.aac";
        s_jobId++;

        _uiManager.Start(this, s_jobId, "Encoding to H264/AAC (Complex)");

        var optionsMain = new ProcessOptionsEncoder(s_jobId, "", true);
        IProcessWorker? processMain = null;
        var taskMain = Task.Run(() => _encoder.EncodeFFmpeg(src, dstEncode, "h264", null, "", optionsMain));

        var options = new ProcessOptionsEncoder(s_jobId, "Extracting Audio", false);
        var result = _muxer.ExtractAudio(src, dstExtract, options);
        if (result == CompletionStatus.Success)
        {
            options.Title = "Encoding Audio";
            result = _encoder.EncodeFFmpeg(dstExtract, dstAac, null, "aac", null, options,
                (_, p) => processMain = p.ProcessWorker);
        }

        if (result != CompletionStatus.Success)
        {
            processMain?.Cancel();
        }

        taskMain.Wait();
        var result2 = taskMain.Result;

        if (result == CompletionStatus.Success && result2 == CompletionStatus.Success)
        {
            options.Title = "Muxing Audio and Video";
            result = _muxer.Muxe(dstEncode, dstAac, dst, options);
        }

        File.Delete(dstEncode);
        File.Delete(dstExtract);
        File.Delete(dstAac);
        _uiManager.Close(s_jobId);

        return GetStatus(result, result2);
    }

    private static CompletionStatus GetStatus(CompletionStatus status1, CompletionStatus status2)
    {
        return HasStatus(CompletionStatus.Timeout) ??
               HasStatus(CompletionStatus.Failed) ??
               HasStatus(CompletionStatus.Cancelled) ??
               HasStatus(CompletionStatus.Success) ??
               CompletionStatus.None;

        CompletionStatus? HasStatus(CompletionStatus result) => status1 == result || status2 == result ? result : null;
    }

    private static string GetPathWithoutExtension(string path)
    {
        var pos = path.LastIndexOf('.');
        return pos == -1 ? path : path.Substring(0, pos);
    }

}
