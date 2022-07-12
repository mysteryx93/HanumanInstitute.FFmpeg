import vapoursynth as vs
video = vs.core.std.BlankClip(length=100, format=vs.YUV444P8, color=[100, 0, 0])
video.set_output()