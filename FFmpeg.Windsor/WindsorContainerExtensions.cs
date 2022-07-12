using System;
using Castle.Windsor;

namespace HanumanInstitute.FFmpeg;

public static class WindsorContainerExtensions
{
    public static void AddFFmpeg(this IWindsorContainer container)
    {
        container.Install(new FFmpegWindsorInstaller());
    }
}