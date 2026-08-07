import vapoursynth as vs
# YUV420 so x264 can take Y4M without swscale (many distro x264 builds lack it).
video = vs.core.std.BlankClip(length=100, format=vs.YUV420P8, color=[100, 128, 128])
video.set_output()
