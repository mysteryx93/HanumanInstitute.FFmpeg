import vapoursynth as vs
# 10-bit 4:2:0 — avoids swscale; needs a high-bit-depth x264 build.
video = vs.core.std.BlankClip(length=100, format=vs.YUV420P10, color=[100, 512, 512])
video.set_output()
