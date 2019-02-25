import vapoursynth as vs
core = vs.get_core()
video = core.std.BlankClip(length=100, format=vs.YUV444P8, color=[100, 0, 0])
video.set_output()